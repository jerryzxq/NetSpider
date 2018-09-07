using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using MongoDB.Driver;
using Vcredit.NetSpider.PluginManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class MobileMongo
    {
        BaseMongo baseMongo = null;
        string dbName = "netspider_";
        public MobileMongo(DateTime date)
        {
            string serverName = dbName + date.ToString(Consts.DateFormatString7);
            ////测试
            //if (date < DateTime.Parse("2016-03-30 18:00:00"))
            //    serverName = "netspider";
            //生产
            if (date < DateTime.Parse("2016-04-11 14:25:00"))
                serverName = "netspider";
            baseMongo = new BaseMongo(serverName, "AnalysisMongoDB");
        }

        public void SaveBasic(Basic basic)
        {
            var query = Query.EQ("Token", basic.Token);
            baseMongo.Remove<Basic>(query, "mobile_basic");
            baseMongo.Remove<Summary>(query, "mobile_summary");
            //保存抓取信息
            baseMongo.Insert<Basic>(basic, "mobile_basic");
            //保存汇总信息
            SaveSummary(basic);
        }

        public void DropMobile()
        {
            string serverName = string.Empty;
            DateTime date = DateTime.Now.AddMonths(-3);
            for (int i = 1; i <= 3; i++)
            {
                serverName = dbName + date.AddMonths(i).ToString(Consts.DateFormatString7);
            }
            if (!serverName.IsEmpty())
                baseMongo.Drop(serverName);

        }

        public Basic GetBasicByIdcard(string Idcard)
        {
            Basic basic = null;
            try
            {
                var query = Query.EQ("Idcard", Idcard);
                baseMongo.Find<Basic>(query, "mobile_basic");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return basic;
        }

        public Basic GetBasicByToken(string Token)
        {
            Basic basic = null;
            try
            {
                var query = Query.EQ("Token", Token);
                basic = baseMongo.FindOne<Basic>(query, "mobile_basic");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return basic;
        }

        public List<Call> GetBasicCallByToken(string Token)
        {
            var basic = GetBasicByToken(Token);
            return basic != null ? basic.CallList : null;
        }

        public Summary GetSummaryByToken(string Token)
        {
            Summary summary = null;
            try
            {
                var query = Query.EQ("Token", Token);
                summary = baseMongo.FindOne<Summary>(query, "mobile_summary");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return summary;
        }

        public Basic GetBasicByIdcardModile(string Idcard, string Modile)
        {
            Basic basic = null;
            try
            {
                var query = Query.And(Query.EQ("BusIdentityCard", Idcard), Query.EQ("Modile", Modile));
                basic = baseMongo.FindOne<Basic>(query, "mobile_basic");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return basic;
        }

        #region 私有方法

        /// <summary>
        /// 根据基本信息汇总信息并保存
        /// </summary>
        /// <param name="basic"></param>
        private void SaveSummary(Basic basic)
        {
            Summary summary = new Summary();
            //int isRealNameAuth = 0;
            DateTime nowDate = DateTime.Parse(basic.UpdateTime);

            try
            {
                nowDate = ((DateTime)(nowDate.Year + "-" + nowDate.Month + "-1").ToDateTime(Consts.DateFormatString)).AddMonths(1);
                summary.Token = basic.Token;
                summary.Name = basic.Name;
                summary.IdentityCard = basic.Idcard;
                summary.Mobile = basic.Mobile;
                summary.Regdate = basic.Regdate;
                if (basic.Regdate.ToTrim().IsEmpty())
                {
                    var startcall = basic.CallList.Where(o => !o.StartTime.IsEmpty()).OrderBy(o => o.StartTime).FirstOrDefault();
                    if (startcall != null)
                    {
                        summary.Regdate = DateTime.Parse(startcall.StartTime).ToString(Consts.DateFormatString11);
                    }
                }

                try
                {
                    var city = GetCityByMobile(basic.Mobile);
                    summary.City = city;
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("从淘宝接口获取手机归属地失败", ex);
                }

                summary.IsRealNameAuth = CommonFun.IsAuth(basic.BusIdentityCard, basic.Idcard, basic.BusName, basic.Name) ? 1 : 0;
                //统计通话次数
                if (basic.CallList != null)
                {
                    summary.OneMonthCallRecordCount = basic.CallList.Where(x => !x.StartTime.IsEmpty() && DateTime.Parse(x.StartTime) > nowDate.AddMonths(-1)).Count();
                    summary.ThreeMonthCallRecordCount = basic.CallList.Where(x => !x.StartTime.IsEmpty() && DateTime.Parse(x.StartTime) > nowDate.AddMonths(-3)).Count();
                    summary.SixMonthCallRecordCount = basic.CallList.Where(x => !x.StartTime.IsEmpty() && DateTime.Parse(x.StartTime) > nowDate.AddMonths(-6)).Count();
                }
                //统计金额
                if (basic.BillList != null)
                {
                    nowDate = nowDate.AddDays(-1);
                    summary.OneMonthCallRecordAmount = basic.BillList.Where(x => !x.BillCycle.IsEmpty() && DateTime.Parse(x.BillCycle) > nowDate.AddMonths(-1) && x.TotalAmt != null).Sum(x => x.TotalAmt.ToDecimal(0));
                    summary.ThreeMonthCallRecordAmount = basic.BillList.Where(x => !x.BillCycle.IsEmpty() && DateTime.Parse(x.BillCycle) > nowDate.AddMonths(-3) && x.TotalAmt != null).Sum(x => x.TotalAmt.ToDecimal(0));
                    summary.SixMonthCallRecordAmount = basic.BillList.Where(x => !x.BillCycle.IsEmpty() && DateTime.Parse(x.BillCycle) > nowDate.AddMonths(-6) && x.TotalAmt != null).Sum(x => x.TotalAmt.ToDecimal(0));
                }

                try
                {
                    SaveMobileVariable(summary, basic);
                }
                catch (Exception e)
                {
                    Log4netAdapter.WriteError("手机变量计算出错：", e);
                }
                //保存
                baseMongo.Insert<Summary>(summary, "mobile_summary");
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("手机账单统计信息保存出错", e);
            }
        }

        /// <summary>
        /// 计算手机变量
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="basic"></param>
        private void SaveMobileVariable(Summary summary, Basic basic)
        {
            //更新时间
            DateTime updateTime = DateTime.Now;

            #region 基本信息类

            if (basic != null)
            {
                updateTime = DateTime.Parse(basic.UpdateTime);
                //手机开卡时长至申请时间月数间隔
                summary.PH_USE_MONS = GetMonth(DateTime.Parse(summary.Regdate), updateTime);
            }

            #endregion

            #region 通话类

            if (basic.CallList != null)
            {
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
                summary.CallCntAvgBJ = bjTian == 0 ? 0 : (bjCall / (decimal)bjTian);

                //被叫联系人个数占比,被叫号码总个数/通话号码总个数
                var callCount = basic.CallList.Where(e => !e.UseTime.IsEmpty()
                    || (!e.InitType.IsEmpty() && e.InitType.Contains("漫游"))
                    && Sum(e.UseTime) < 36000 && e.SubTotal < 500 && e.SubTotal < (decimal)(((double)Sum(e.UseTime) / 6 + 1) * 0.8 + 3)
                    ).Count();//通话总数
                summary.PhoneNbrBjRate = callCount == 0 ? 0 : bjCall / (decimal)callCount;

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
                summary.CallLenNitRate = callTotalUseTime == 0 ? 0 : callYeJainUseTime / (decimal)callTotalUseTime;

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
                var zjAoumt = newlist.Sum(e => e.SubTotal == null ? 0 : (decimal)e.SubTotal);
                summary.ZjsjPhnCrgAvg = zjTian == 0 ? 0 : (zjAoumt / (decimal)zjTian);

                //同业催收
                summary.CallTimes = CallTimes(basic);

                #endregion

                #region 联系人个数

                //所有联系人个数
                summary.CALL_PHONE_CNT = basic.CallList.Select(e => e.OtherCallPhone).Distinct().Count();

                // 被叫联系人个数
                summary.CALLED_PHONE_CNT = basic.CallList.Where(e => !e.InitType.IsEmpty() && e.InitType.Contains("被"))
                    .Select(e => e.OtherCallPhone).Distinct().Count();

                // 主叫联系人个数
                summary.CALLING_PHONE_CNT = basic.CallList.Where(e => !e.InitType.IsEmpty() && e.InitType.Contains("主"))
                    .Select(e => e.OtherCallPhone).Distinct().Count();

                #endregion

                #region 手机话费

                // 主叫手机话费总额
                summary.CALLING_SUBTTL = basic.CallList.Where(e => !e.InitType.IsEmpty()
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
                summary.DAY180_CALLING_SUBTTL = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主"))
                    .Sum(e => (e.SubTotal.ToString().ToDecimal(0)));
                summary.DAY180_CALLING_SUBTTL = summary.DAY180_CALLING_SUBTTL > 0 ? summary.DAY180_CALLING_SUBTTL : -9999999;

                // 近90天内主叫话费
                summary.CALL_LSTM3_SMY_CALL_MN = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主"))
                    .Sum(e => (e.SubTotal.ToString().ToDecimal(0)));
                #endregion

                #region 通话天数

                //通话天数
                summary.CTT_DAYS_CNT = basic.CallList.Where(e => !e.StartTime.IsEmpty())
                    .Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                //有被叫的天数
                summary.CALLED_CTT_DAYS_CNT = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("被"))
                    .Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                //有主叫的通话天数
                summary.CALLING_CTT_DAYS_CNT = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("主"))
                    .Select(e =>
                    {
                        return e.StartTime.ToDateTime().Value.ToString(Consts.DateFormatString2);
                    }).Distinct().Count();

                // 近180天被叫通话天数
                summary.ANS_DAY_CNT = basic.CallList.Where(e =>
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
                summary.ALL_CALL_TIMES = basic.CallList.Count();

                //累计被叫次数
                summary.CALLED_TIMES = basic.CallList.Where(e => !e.InitType.IsEmpty() && e.InitType.Contains("被")).Count();
             
                // 近180天内通话次数
                summary.CALL_SMY_CTT_CNT_TOTAL = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    ).Count();

                // 近180天内主叫次数
                summary.CALL_LSTM6_CALL_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主")).Count();

                //近180天被叫次数
                summary.CALL_D180_CALLED_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("被")).Count();

                // 近180天内夜晚通话次数(23:00-9:00)
                summary.CALL_SMY_CTT_NIGHT_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)).Count();

                // 近180天内夜晚主叫次数(23:00-9:00)
                summary.CALL_SMY_CALL_NIGHT_CNT = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)
                    && e.InitType.Contains("主")).Count();

                // 近90天内通话次数
                summary.DAY90_CALL_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    ).Count();

                // 90天内主叫次数
                summary.DAY90_CALLING_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主")).Count();

                //累计上午被叫次数(9:00-13:00)
                summary.MRNG_CALLED_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("9:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("13:00").Hour)
                    && e.InitType.Contains("被")
                    ).Count();

                //累计下午通话次数(14:00-18:00)
                summary.AFTN_CALL_TIMES = basic.CallList.Where(e => !e.StartTime.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("14:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                    ).Count();

                //累计漫游通话次数
                summary.ROAMING_CALL_TIMES = basic.CallList.Where(e => !e.CallType.IsEmpty() && e.CallType.Contains("漫游")).Count();

                //连续超过3天未通话次数
                DateTime prestart;
                DateTime start;
                TimeSpan timespan;
                summary.DAY3_CHECK_CALL_TIMES = 0;
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
                        summary.DAY3_CHECK_CALL_TIMES += 1;
                    }
                }

                #endregion

                #region 通话时长

                //累计通话时长
                summary.CALL_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty()).Sum(e => { return Sum(e.UseTime); });

                //累计被叫时长
                summary.CALLED_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("被")).Sum(e => { return Sum(e.UseTime); });

                //累计主叫时长
                summary.CALLING_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && e.InitType.Contains("主"))
                    .Sum(e => { return Sum(e.UseTime); });

                // 本地通话时长汇总
                summary.LOCAL_CALL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.CallType.IsEmpty()
                   && e.CallType.Contains("本地")).Sum(e => { return Sum(e.UseTime); });

                //近180天被叫时长
                summary.CALL_D180_CALLED_TIME = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("被")
                    ).Sum(e => { return Sum(e.UseTime); });

                // 近180天主叫时长
                summary.CALL_D180_CALL_TIME = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主")
                      ).Sum(e => { return Sum(e.UseTime); });

                // 近180天晚间被叫时长(18:00-23:00)
                summary.D180_NIGHT_CALLED_TIME = basic.CallList.Where(e =>
                    !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("18:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("23:00").Hour)
                    && e.InitType.Contains("被")
                    ).Sum(e => { return Sum(e.UseTime); });

                // 近90天内累计通话时长
                summary.DAY90_CALL_TTL_TIME = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                     && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    ).Sum(e => { return Sum(e.UseTime); });

                // 近90天内主叫通话时长
                summary.DAY90_CALLING_TTL_TIME = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-90).ToString("yyyy-MM-dd 00:00:00"))
                    && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                    && e.InitType.Contains("主"))
                    .Sum(e => { return Sum(e.UseTime); });

                // 累计白天主叫时长(9:00-18:00)
                summary.DAY_CALLING_TTL_TIME = basic.CallList.Where(e => !e.StartTime.IsEmpty() && !e.UseTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("9:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                    && e.InitType.Contains("主")
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计晚间被叫时长(18:00-23:00)
                summary.NIGHT_CALLED_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("18:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("23:00").Hour)
                   && e.InitType.Contains("被")
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计下午通话时长(13:00-18:00)
                summary.AFTN_CALL_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("13:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计下午主叫时长(13:00-18:00)
                summary.AFTN_CALLING_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("13:00").Hour
                    && DateTime.Parse(e.StartTime).Hour < DateTime.Parse("18:00").Hour)
                   && e.InitType.Contains("主")
                    ).Sum(e => { return Sum(e.UseTime); });

                //累计夜晚通话时长(23:00-9:00)
                summary.NIGHT_CALL_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    || DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)
                     ).Sum(e => { return Sum(e.UseTime); });

                //累计夜晚主叫时长(23:00-9:00)
                summary.NIGHT_CALLING_TTL_TIME = basic.CallList.Where(e => !e.UseTime.IsEmpty() && !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
                    && (DateTime.Parse(e.StartTime).Hour >= DateTime.Parse("23:00").Hour
                    || DateTime.Parse(e.StartTime).Hour < DateTime.Parse("9:00").Hour)
                    && e.InitType.Contains("主"))
                    .Sum(e => { return Sum(e.UseTime); });

                #endregion

                //30天内危险号码被叫次数
                summary.CALLED_TIMES_IN30DAY = CallTimes(basic, "被叫", 30);

                //15天内危险号码被叫次数
                summary.CALLED_TIMES_IN15DAY = CallTimes(basic, "被叫", 15);

                //30天内灰色号码被叫次数
                summary.CALLED_TIMES_IN30DAY_FOR_GRAY = CallTimesForGray(basic, "被叫", 30);

                //15天内灰色号码被叫次数
                summary.CALLED_TIMES_IN15DAY_FOR_GRAY = CallTimesForGray(basic, "被叫", 15);

            }

            #endregion

            #region 短信类
            if (basic.SmsList != null)
            {
                //总短信联系人个数
                summary.SMS_PHONE_CNT = basic.SmsList.Select(e => e.OtherSmsPhone).Distinct().Count();
                //其他时间发送短信联系人个数
                var weekList = new List<DayOfWeek>() { 
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                };
                summary.OTHER_SMS_PHONE_CNT = basic.SmsList.Where(e => !e.StartTime.IsEmpty() && !e.InitType.IsEmpty()
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
                summary.NET_LSTM6_ONL_FLOW = basic.NetList.Where(e => !e.StartTime.IsEmpty() && e.SubFlow != null
                       && DateTime.Parse(e.StartTime) >= DateTime.Parse(updateTime.AddDays(-180).ToString("yyyy-MM-dd 00:00:00"))
                       && DateTime.Parse(e.StartTime) < DateTime.Parse(updateTime.ToString("yyyy-MM-dd 00:00:00"))
                       ).Sum(e => (e.SubFlow.ToString().ToDecimal(0)));
                summary.NET_LSTM6_ONL_FLOW = summary.NET_LSTM6_ONL_FLOW > 0 ? summary.NET_LSTM6_ONL_FLOW : -9999999;
            }
            #endregion

            #region 月账单类

            if (basic.BillList != null)
            {
                //账单中最大的套餐金额
                summary.MaxPlanAmt = basic.BillList.Where(e => !e.PlanAmt.ToTrim().IsEmpty()).Max(e => e.PlanAmt.ToDecimal().Value);
                summary.MaxPlanAmt = summary.MaxPlanAmt != null ? summary.MaxPlanAmt : 0;
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
                Log4netAdapter.WriteError("从百度获取手机归属地", ex);
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

        #endregion

    }
}
