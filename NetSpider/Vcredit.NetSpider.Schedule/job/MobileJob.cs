using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using System.IO;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.Schedule.job
{
    class MobileJob : IJob
    {
        IMobileExecutor mobileExecutor = ExecutorManager.GetMobileExecutor();
        ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件

        public void Execute(IJobExecutionContext context)
        {
            BaseRes Res = new BaseRes();
            IList<Spd_applyEntity> applyList = new List<Spd_applyEntity>();
            MobileReq fundReq = null;

            applyList = applyService.GetApplyListByCrawlStatus(10, ServiceConsts.CrawlerStatusCode_CrawlerSuccess);

            if (applyList.Count > 0)
            {
                foreach (Spd_applyEntity apply in applyList)
                {
                    apply.Crawl_status = ServiceConsts.CrawlerStatusCode_Analysising;
                    apply.Description = ServiceConsts.Analysising;
                    applyService.SaveOrUpdate(apply);
                }

                foreach (Spd_applyEntity apply in applyList)
                {
                    fundReq = new MobileReq()
                    {
                        Token = apply.Token,
                        Mobile = apply.Mobile,
                        Name = apply.Name,
                        IdentityCard = apply.Identitycard,
                        Website = apply.Website,
                    };
                    Res = mobileExecutor.MobileAnalysis(fundReq, apply.CreateTime);
                    apply.Apply_status = Res.StatusCode;
                    if (Res.StatusCode == ServiceConsts.StatusCode_success)
                    {
                        Basic mobile = jsonParser.DeserializeObject<Basic>(Res.Result);
                        if (mobile.CallList.Count > 0)
                        {
                            SaveSummary(mobile, apply);//手机变量汇总保存
                            apply.Crawl_status = ServiceConsts.CrawlerStatusCode_AnalysisSuccess;
                        }
                        else
                        {
                            apply.Crawl_status = ServiceConsts.CrawlerStatusCode_AnalysisFail_NoCalls;
                            apply.Apply_status = ServiceConsts.StatusCode_fail;
                        }
                    }
                    else
                    {
                        apply.Crawl_status = ServiceConsts.CrawlerStatusCode_AnalysisFail;
                    }
                    apply.Description = Res.StatusDescription;
                    applyService.SaveOrUpdate(apply);
                }
            }
        }

        /// <summary>
        /// 手机变量汇总保存
        /// </summary>
        /// <param name="basic"></param>
        private void SaveSummary(Basic basic, Spd_applyEntity apply)
        {
            IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
            Variable_mobile_summaryEntity entity = new Variable_mobile_summaryEntity();
            DateTime nowDate = DateTime.Parse(basic.UpdateTime);

            try
            {
                entity.Mobile = basic.Mobile;
                entity.BusName = basic.BusName;
                entity.BusIdentityCard = basic.BusIdentityCard;
                entity.Name = basic.Name;
                entity.IdentityCard = basic.Idcard;
                entity.BusId = null;
                entity.BusType = apply.AppId;
                entity.SourceId = apply.ApplyId;
                entity.SourceType = "vcredit";
                entity.Regdate = basic.Regdate;
                entity.UpdateTime = DateTime.Parse(basic.UpdateTime);
                entity.CreateTime = DateTime.Now;
                if (basic.Regdate.ToTrim().IsEmpty())
                {
                    var startcall = basic.CallList.Where(o => !o.StartTime.IsEmpty()).OrderBy(o => o.StartTime).FirstOrDefault();
                    if (startcall != null)
                    {
                        entity.Regdate = DateTime.Parse(startcall.StartTime).ToString(Consts.DateFormatString11);
                    }
                }
                try
                {
                    var city = GetCityByMobile(basic.Mobile);
                    entity.City = city;
                }
                catch (Exception ex) { }

                entity.IsRealNameAuth = CommonFun.IsAuth(basic.BusIdentityCard, basic.Idcard, basic.BusName, basic.Name) ? 1 : 0;

                try
                {
                    SaveMobileVariable(entity, basic);
                }
                catch (Exception e)
                {
                    Log4netAdapter.WriteError("手机变量计算出错：", e);
                }
                //删除之前数据
                variableService.DeleteBySourceIdAndSourceType(apply.ApplyId.ToString(), "vcredit");
                //保存
                variableService.Save(entity);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("手机账单统计信息保存出错", e);
            }
        }

        /// <summary>
        /// 手机变量计算
        /// </summary>
        /// <param name="basic">手机的基本信息</param>
        private void SaveMobileVariable(Variable_mobile_summaryEntity entity, Basic basic)
        {
            DateTime updateTime = DateTime.Parse(basic.UpdateTime); ;  //更新时间
            DateTime nowDate = DateTime.Parse(basic.UpdateTime);
            nowDate = ((DateTime)(nowDate.Year + "-" + nowDate.Month + "-1").ToDateTime(Consts.DateFormatString)).AddMonths(1);

            //基本信息类
            if (basic != null)
            {
                //手机开卡时长至申请时间月数间隔
                entity.PH_USE_MONS = GetMonth(DateTime.Parse(entity.Regdate), updateTime);
            }

            #region 基本信息类

            if (basic != null)
            {
                //手机开卡时长至申请时间月数间隔
                entity.PH_USE_MONS = GetMonth(DateTime.Parse(entity.Regdate), updateTime);
            }

            #endregion

            #region 通话类

            if (basic.CallList != null)
            {
                entity.One_Month_Call_Record_Count = basic.CallList.Where(x => !x.StartTime.IsEmpty() && DateTime.Parse(x.StartTime) > nowDate.AddMonths(-1)).Count();
                entity.Three_Month_Call_Record_Count = basic.CallList.Where(x => !x.StartTime.IsEmpty() && DateTime.Parse(x.StartTime) > nowDate.AddMonths(-3)).Count();
                entity.Six_Month_Call_Record_Count = basic.CallList.Where(x => !x.StartTime.IsEmpty() && DateTime.Parse(x.StartTime) > nowDate.AddMonths(-6)).Count();

                #region

                //日均被叫通话次数,被叫总通话号码个数/被叫天数
                var bjList = basic.CallList.Where(e => !e.InitType.IsEmpty() && !e.UseTime.IsEmpty()
                    && (e.InitType.Contains("被叫") || e.InitType.Contains("漫游"))
                    && Sum(e.UseTime) < 36000 && e.SubTotal < 500 && e.SubTotal < (decimal)(((double)Sum(e.UseTime) / 6 + 1) * 0.8 + 3));
                var bjCall = bjList.Count();//被叫数总数
                var bjTian = bjList.Where(e => !e.StartTime.IsEmpty()).Select(e =>
                {
                    return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                }).Distinct().Count(); //被叫总天数
                entity.CALL_CNT_AVG_BJ = bjTian == 0 ? 0 : (bjCall / (decimal)bjTian);

                //被叫联系人个数占比,被叫号码总个数/通话号码总个数
                var callCount = basic.CallList.Where(e => !e.UseTime.IsEmpty()
                    || (!e.InitType.IsEmpty() && e.InitType.Contains("漫游"))
                    && Sum(e.UseTime) < 36000 && e.SubTotal < 500 && e.SubTotal < (decimal)(((double)Sum(e.UseTime) / 6 + 1) * 0.8 + 3)
                    ).Count();//通话总数
                entity.PHONE_NBR_BJ_RATE = callCount == 0 ? 0 : bjCall / (decimal)callCount;

                //夜间通话总时长占比,夜间（0:00-6:00）通话总时长/通话总时长
                var callYeJainUseTime = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty()
                    && e.StartTime.ToDateTime().Value >= (e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2) + " 00:00:00").ToDateTime().Value
                    && e.StartTime.ToDateTime() <= (e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2) + " 00:06:00").ToDateTime().Value
                    || (!e.InitType.IsEmpty() && e.InitType.Contains("漫游"))
                    && Sum(e.UseTime) < 36000 && e.SubTotal < 500 && e.SubTotal < (decimal)(((double)Sum(e.UseTime) / 6 + 1) * 0.8 + 3))
                    .Sum(e => { return Sum(e.UseTime); });

                //通话总时长
                var callTotalUseTime = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty()
                    || (!e.InitType.IsEmpty() && e.InitType.Contains("漫游"))
                    && Sum(e.UseTime) < 36000 && e.SubTotal < 500 && e.SubTotal < (decimal)(((double)Sum(e.UseTime) / 6 + 1) * 0.8 + 3)
                    ).Sum(e => { return Sum(e.UseTime); });
                //夜间通话总时长占比
                entity.CALL_LEN_NIT_RATE = callTotalUseTime == 0 ? 0 : callYeJainUseTime / (decimal)callTotalUseTime;

                //日均主叫手机话费 ,（被叫方非固话）主叫手机话费/主叫天数
                var zjlist = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.OtherCallPhone.IsEmpty() && !e.InitType.IsEmpty()
                    && (e.InitType.Contains("主叫") || e.InitType.Contains("漫游"))
                    && Sum(e.UseTime) < 36000 && e.SubTotal < 500 && e.SubTotal < (decimal)(((double)Sum(e.UseTime) / 6 + 1) * 0.8 + 3));
                var newlist = new List<Call>();
                foreach (var e in zjlist)
                {
                    if ((!e.OtherCallPhone.StartsWith("0") && e.OtherCallPhone.Length == 11)
                       || (e.OtherCallPhone.StartsWith("0086") && e.OtherCallPhone.Length == 15)
                       || (e.OtherCallPhone.StartsWith("86") && e.OtherCallPhone.Length == 13)
                       || (e.OtherCallPhone.StartsWith("17951") && e.OtherCallPhone.Length == 16)
                       || (e.OtherCallPhone.StartsWith("17911") && e.OtherCallPhone.Length == 16)
                       || (e.OtherCallPhone.StartsWith("17901") && e.OtherCallPhone.Length == 16)
                       || (e.OtherCallPhone.StartsWith("10193") && e.OtherCallPhone.Length == 16)
                       || (e.OtherCallPhone.StartsWith("95013") && e.OtherCallPhone.Length == 16)
                       || (e.OtherCallPhone.StartsWith("6") && e.OtherCallPhone.Length < 5))
                    {
                        newlist.Add(e);
                    }
                }
                var zjTian = newlist.Select(e =>
                {
                    return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                }).Distinct().Count(); //主叫总天数
                var zjAoumt = newlist.Sum(e => e.SubTotal);
                entity.ZJSJ_PHN_CRG_AVG = zjTian == 0 ? 0 : (zjAoumt / (decimal)zjTian);

                //同业催收
                entity.CallTimes = CallTimes(basic);

                #endregion

                #region 联系人个数

                //所有联系人个数
                entity.CALL_PHONE_CNT = basic.CallList.Select(e => e.OtherCallPhone).Distinct().Count();

                // 被叫联系人个数
                entity.CALLED_PHONE_CNT = basic.CallList.Where(e => !e.InitType.IsEmpty() && e.InitType.Contains("被"))
                    .Select(e => e.OtherCallPhone).Distinct().Count();

                // 主叫联系人个数
                entity.CALLING_PHONE_CNT = basic.CallList.Where(e => !e.InitType.IsEmpty() && e.InitType.Contains("主"))
                    .Select(e => e.OtherCallPhone).Distinct().Count();

                #endregion

                #region 手机话费

                // 主叫手机话费总额
                entity.CALLING_SUBTTL = basic.CallList.Where(e => !e.InitType.IsEmpty()
                     && ((!e.OtherCallPhone.StartsWith("0") && e.OtherCallPhone.Length == 11)
                     || (e.OtherCallPhone.StartsWith("0086") && e.OtherCallPhone.Length == 15)
                     || (e.OtherCallPhone.StartsWith("86") && e.OtherCallPhone.Length == 13)
                     || (e.OtherCallPhone.StartsWith("17951") && e.OtherCallPhone.Length == 16)
                     || (e.OtherCallPhone.StartsWith("17911") && e.OtherCallPhone.Length == 16)
                     || (e.OtherCallPhone.StartsWith("17901") && e.OtherCallPhone.Length == 16)
                     || (e.OtherCallPhone.StartsWith("10193") && e.OtherCallPhone.Length == 16)
                     || (e.OtherCallPhone.StartsWith("95013") && e.OtherCallPhone.Length == 16)
                     || (e.OtherCallPhone.StartsWith("6") && e.OtherCallPhone.Length < 5))
                    && e.InitType.Contains("主"))
                    .Sum(e => (e.SubTotal.ToString().ToDecimal(0)));

                // 近180天内累计主叫套餐外话费
                entity.DAY180_CALLING_SUBTTL = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主"))
                    .Sum(e => (e.SubTotal.ToString().ToDecimal(0)));
                entity.DAY180_CALLING_SUBTTL = entity.DAY180_CALLING_SUBTTL > 0 ? entity.DAY180_CALLING_SUBTTL : -9999999;

                // 近90天内主叫话费
                entity.CALL_LSTM3_SMY_CALL_MN = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主"))
                    .Sum(e => (e.SubTotal.ToString().ToDecimal(0)));
                #endregion

                #region 通话天数

                //通话天数
                entity.CTT_DAYS_CNT = basic.CallList.Where(e => !e.StartTime.IsEmpty())
                    .Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                //有被叫的天数
                entity.CALLED_CTT_DAYS_CNT = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("被"))
                    .Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                //有主叫的通话天数
                entity.CALLING_CTT_DAYS_CNT = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("主"))
                    .Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                // 近180天被叫通话天数
                entity.ANS_DAY_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("被")).Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                #endregion

                #region 通话次数

                //通话总次数
                entity.ALL_CALL_TIMES = basic.CallList.Count();

                //累计被叫次数
                entity.CALLED_TIMES = basic.CallList.Where(e => !e.InitType.IsEmpty() && e.InitType.Contains("被")).Count();

                // 近180天内通话次数
                entity.CALL_SMY_CTT_CNT_TOTAL = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    ).Count();

                // 近180天内主叫次数
                entity.CALL_LSTM6_CALL_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主")).Count();

                //近180天被叫次数
                entity.CALL_D180_CALLED_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("被")).Count();

                // 近180天内夜晚通话次数(23:00-9:00)
                entity.CALL_SMY_CTT_NIGHT_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)).Count();

                // 近180天内夜晚主叫次数(23:00-9:00)
                entity.CALL_SMY_CALL_NIGHT_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)
                    && e.InitType.Contains("主")).Count();

                // 近90天内通话次数
                entity.DAY90_CALL_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    ).Count();

                // 90天内主叫次数
                entity.DAY90_CALLING_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主")).Count();

                //累计上午被叫次数(9:00-13:00)
                entity.MRNG_CALLED_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("9:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("13:00").Hour)
                    && e.InitType.Contains("被")
                    ).Count();

                //累计下午通话次数(14:00-18:00)
                entity.AFTN_CALL_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("14:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                    ).Count();

                //累计漫游通话次数
                entity.ROAMING_CALL_TIMES = basic.CallList.Where(e => !e.CallType.IsEmpty() && e.CallType.Contains("漫游")).Count();

                //连续超过3天未通话次数
                DateTime prestart;
                DateTime start;
                TimeSpan timespan;
                entity.DAY3_CHECK_CALL_TIMES = 0;
                DateTime lastDate = DateTime.Parse(updateTime.AddDays(-1).ToString(Consts.DateFormatString2));
                var callDtlList = basic.CallList.Where(e => !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) <= lastDate
                    ).OrderByDescending(x => DateTime.Parse(x.StartTime)).ToList();
                for (int i = 0; i < callDtlList.Count(); i++)
                {
                    if (i == 0)
                        prestart = lastDate;
                    else
                        prestart = DateTime.Parse(DateTime.Parse(callDtlList[i - 1].StartTime).ToString(Consts.DateFormatString2));
                    start = DateTime.Parse(DateTime.Parse(callDtlList[i].StartTime).ToString(Consts.DateFormatString2));
                    timespan = prestart - start;
                    if (timespan.TotalDays - 1 >= 3)
                    {
                        entity.DAY3_CHECK_CALL_TIMES += 1;
                    }
                }

                #endregion

                #region 通话时长

                //累计通话时长
                entity.CALL_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty()).Sum(e => { return Sum(e.UseTime); });

                //累计被叫时长
                entity.CALLED_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("被")).Sum(e => { return Sum(e.UseTime); });

                //累计主叫时长
                entity.CALLING_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("主"))
                    .Sum(e => { return Sum(e.UseTime); });

                // 本地通话时长汇总
                entity.LOCAL_CALL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.CallType.IsEmpty()
                   && e.CallType.Contains("本地")).Sum(e => { return Sum(e.UseTime); });

                //近180天被叫时长
                entity.CALL_D180_CALLED_TIME = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("被")
                    ).Sum(e => { return Sum(e.UseTime); });

                // 近180天主叫时长
                entity.CALL_D180_CALL_TIME = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主")
                      ).Sum(e => { return Sum(e.UseTime); });

                // 近180天晚间被叫时长(18:00-23:00)
                entity.D180_NIGHT_CALLED_TIME = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("18:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("23:00").Hour)
                    && e.InitType.Contains("被")
                    ).Sum(e => { return Sum(e.UseTime); });

                // 近90天内累计通话时长
                entity.DAY90_CALL_TTL_TIME = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                     && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    ).Sum(e => { return Sum(e.UseTime); });

                // 近90天内主叫通话时长
                entity.DAY90_CALLING_TTL_TIME = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主"))
                    .Sum(e => { return Sum(e.UseTime); });

                // 累计白天主叫时长(9:00-18:00)
                entity.DAY_CALLING_TTL_TIME = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("9:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                    && e.InitType.Contains("主")
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计晚间被叫时长(18:00-23:00)
                entity.NIGHT_CALLED_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("18:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("23:00").Hour)
                   && e.InitType.Contains("被")
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计下午通话时长(13:00-18:00)
                entity.AFTN_CALL_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("13:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计下午主叫时长(13:00-18:00)
                entity.AFTN_CALLING_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("13:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                   && e.InitType.Contains("主")
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计夜晚通话时长(23:00-9:00)
                entity.NIGHT_CALL_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    || DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)
                     ).Sum(e => { return Sum(e.UseTime); });

                //累计夜晚主叫时长(23:00-9:00)
                entity.NIGHT_CALLING_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    || DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)
                    && e.InitType.Contains("主"))
                    .Sum(e => { return Sum(e.UseTime); });

                #endregion

                //30天内危险号码被叫次数
                entity.CALLED_TIMES_IN30DAY = CallTimes(basic, "被叫", 30);

                //15天内危险号码被叫次数
                entity.CALLED_TIMES_IN15DAY = CallTimes(basic, "被叫", 15);

                //30天内灰色号码被叫次数
                entity.CALLED_TIMES_IN30DAY_Gray = CallTimesForGray(basic, "被叫", 30);

                //15天内灰色号码被叫次数
                entity.CALLED_TIMES_IN15DAY_Gray = CallTimesForGray(basic, "被叫", 15);

            }

            #endregion

            #region 短信类
            if (basic.SmsList != null)
            {
                //总短信联系人个数
                entity.SMS_PHONE_CNT = basic.SmsList.Select(e => e.OtherSmsPhone).Distinct().Count();
                //其他时间发送短信联系人个数
                var weekList = new List<DayOfWeek>() { 
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                };
                entity.OTHER_SMS_PHONE_CNT = basic.SmsList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                  && e.InitType.Contains("发")
                  && !((DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("00:00").Hour
                  && DateTime.Parse(e.StartTime).Hour <= DateTime.Parse("6:00").Hour)
                  || (weekList.Contains(DateTime.Parse(e.StartTime).DayOfWeek)
                  && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("8:00").Hour
                  && DateTime.Parse(e.StartTime).Hour <= DateTime.Parse("18:00").Hour)))
                  ).Select(e => e.OtherSmsPhone).Distinct().Count();
            }
            #endregion

            #region 上网类
            if (basic.NetList != null)
            {
                // 近六月内累计上网流量
                entity.NET_LSTM6_ONL_FLOW = basic.NetList.Where(e => !e.StartTime.IsEmpty() && e.SubFlow != null
                       && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                       && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                       ).Sum(e => (e.SubFlow.ToString().ToDecimal(0)));
                entity.NET_LSTM6_ONL_FLOW = entity.NET_LSTM6_ONL_FLOW > 0 ? entity.NET_LSTM6_ONL_FLOW : -9999999;
            }
            #endregion

            #region 月账单类

            if (basic.BillList != null)
            {
                nowDate = nowDate.AddDays(-1);
                entity.One_Month_Call_Record_Amount = basic.BillList.Where(x => !x.BillCycle.IsEmpty() && DateTime.Parse(x.BillCycle) > nowDate.AddMonths(-1) && x.TotalAmt != null).Sum(x => x.TotalAmt.ToDecimal(0));
                entity.Three_Month_Call_Record_Amount = basic.BillList.Where(x => !x.BillCycle.IsEmpty() && DateTime.Parse(x.BillCycle) > nowDate.AddMonths(-3) && x.TotalAmt != null).Sum(x => x.TotalAmt.ToDecimal(0));
                entity.Six_Month_Call_Record_Amount = basic.BillList.Where(x => !x.BillCycle.IsEmpty() && DateTime.Parse(x.BillCycle) > nowDate.AddMonths(-6) && x.TotalAmt != null).Sum(x => x.TotalAmt.ToDecimal(0));
                //账单中最大的套餐金额
                entity.MAX_PLAN_AMT = basic.BillList.Where(e => !e.PlanAmt.ToTrim().IsEmpty()).Max(e => e.PlanAmt.ToDecimal().Value);
                entity.MAX_PLAN_AMT = entity.MAX_PLAN_AMT != null ? entity.MAX_PLAN_AMT : 0;
            }

            #endregion
        }

        private decimal Sum(string value)
        {
            var list = value.GetNumerFromString();
            var sum = 0.00m;
            if (list.Count > 0)
            {
                if (list.Count == 3)
                {
                    sum += list[0] * 60 * 60;
                    sum += list[1] * 60;
                    sum += list[2];
                }
                else if (list.Count == 2)
                {
                    sum += list[0] * 60;
                    sum += list[1];
                }
                else
                {
                    sum += list[0];
                }
            }

            return sum;
        }

        /// <summary>
        /// 同业催收
        /// </summary>
        /// <param name="basic"></param>
        /// <returns></returns>
        private int CallTimes(Basic basic)
        {
            try
            {
                ///客户催收数
                IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口

                string xxqdWeChatService = AppSettings.xxqdWeChatService;
                HttpItem httpItem = new HttpItem()
                {
                    Method = "GET",
                    URL = xxqdWeChatService
                };
                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                List<string> mobileList = jsonService.GetArrayFromParse(httpResults.Html, "Content");
                int times = mobileList.Sum(e => basic.CallList.Count(o => o.OtherCallPhone == e && Convert.ToDateTime(o.StartTime) >= DateTime.Now.AddMonths(-3)));
                return times;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// 危险号码次数
        /// </summary>
        /// <param name="basic">基本信息</param>
        /// <param name="init_type">通话类型</param>
        /// <param name="days">天</param>
        /// <returns></returns>
        private int CallTimes(Basic basic, string init_type, int days)
        {
            try
            {
                ///客户催收数
                IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
                string collectionChatService = AppSettings.collectionWebChatService;
                HttpItem httpItem = new HttpItem()
                {
                    Method = "GET",
                    URL = collectionChatService
                };
                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                List<string> mobileList = jsonService.GetArrayFromParse(httpResults.Html, "Result");
                int times = mobileList.Sum(mobile => basic.CallList.Count(o => o.OtherCallPhone == mobile
                    && !o.InitType.IsEmpty() && o.InitType == init_type
                    && DateTime.Parse(o.StartTime) >= DateTime.Parse(DateTime.Parse(basic.UpdateTime).AddDays(-days).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(o.StartTime) < DateTime.Parse(DateTime.Parse(basic.UpdateTime).ToString("yyyy-MM-dd 00:00:00"))));

                return times;
            }
            catch (Exception ex)
            {
            }
            return 0;

        }

        /// <summary>
        /// 灰色号码次数
        /// </summary>
        /// <param name="basic">基本信息</param>
        /// <param name="init_type">通话类型</param>
        /// <param name="days">天</param>
        /// <returns></returns>
        private int CallTimesForGray(Basic basic, string init_type, int days)
        {
            try
            {
                ///客户催收数
                IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
                string grayNumberService = AppSettings.grayNumberService;
                HttpItem httpItem = new HttpItem()
                {
                    Method = "GET",
                    URL = grayNumberService
                };
                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                List<string> mobileList = jsonService.GetArrayFromParse(httpResults.Html, "Result");
                int times = mobileList.Sum(mobile => basic.CallList.Count(o => o.OtherCallPhone == mobile
                    && !o.InitType.IsEmpty() && o.InitType == init_type
                    && DateTime.Parse(o.StartTime) >= DateTime.Parse(DateTime.Parse(basic.UpdateTime).AddDays(-days).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(o.StartTime) < DateTime.Parse(DateTime.Parse(basic.UpdateTime).ToString("yyyy-MM-dd 00:00:00"))));

                return times;
            }
            catch (Exception ex)
            {

            }
            return 0;

        }

        /// <summary>
        /// 手机归属地
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        private string GetCityByMobile(string mobile)
        {
            try
            {
                string Url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query={0}&co=&resource_id=6004&t=1451031584105&ie=utf8&oe=gbk&cb=op_aladdin_callback&format=json&tn=baidu&cb=&_=1451031555214";

                HttpHelper httpHelper = new HttpHelper();
                HttpItem httpItem = new HttpItem()
                {
                    URL = String.Format(Url, mobile)
                };
                var httpResult = httpHelper.GetHtml(httpItem);
                var obj = JsonConvert.DeserializeObject(httpResult.Html);
                JObject js = obj as JObject;
                JArray bdp = js["data"] as JArray;
                if (bdp.Count > 0)
                {
                    return bdp[0]["city"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("从百度获取手机归属地：" + ex.Message);
            }
            return "";
        }

        /// <summary>
        /// 获取月份差
        /// </summary>
        /// <param name="dtbegin"></param>
        /// <param name="dtend"></param>
        /// <returns></returns>
        private int GetMonth(DateTime dtbegin, DateTime dtend)
        {
            int Month = 0;

            if ((dtend.Year - dtbegin.Year) == 0)
            {
                Month = dtend.Month - dtbegin.Month;
            }
            if ((dtend.Year - dtbegin.Year) >= 1)
            {
                if (dtend.Month - dtbegin.Month < 0)
                {
                    Month = (dtend.Year - dtbegin.Year - 1) * 12 + (12 - dtbegin.Month) + dtend.Month;
                }
                else
                {
                    Month = (dtend.Year - dtbegin.Year) * 12 + dtend.Month - dtbegin.Month;
                }
            }
            return Month;
        }
    }
}
