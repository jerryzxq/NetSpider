using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading;
using System.Web;
//using System.Web.Providers.Entities;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.WorkFlow;
using Vcredit.NetSpider.Processor;
using Vcredit.WeiXin.RestService.Contracts;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.PluginManager;
using System.Xml.Linq;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Cn.Vcredit.VBS.Model;
using Vcredit.NetSpider.Entity;
using Cn.Vcredit.VBS.BusinessLogic;
using Cn.Vcredit.VBS.BusinessLogic.Entity;
using Vcredit.Common.Helper;
using Vcredit.Common.Constants;
using Vcredit.WeiXin.RestService.Operation;
using Cn.Vcredit.VBS.Interface;
using Cn.Vcredit.VBS.BLL;
using System.Drawing;
using Cn.Vcredit.VBS.PostLoan.FinanceConfig.Action;
using Cn.Vcredit.VBS.PostLoan.OrderInfo;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.WeiXin.RestService.Models.RC;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Mobile;

namespace Vcredit.WeiXin.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class RCService : IRCService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口
        IChsiExecutor chsiExecutor = ExecutorManager.GetChsiExecutor();

        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        BaseMongo baseMongo = new BaseMongo();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public RCService()
        {
        }
        #endregion

        #region 星星钱袋授信
        public BaseRes XXQDCreditForForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                RCWechatReq Req = reqText.DeserializeXML<RCWechatReq>();
                baseRes = XXQDCredit(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes XXQDCreditForJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                RCWechatReq Req = jsonService.DeserializeObject<RCWechatReq>(reqText);
                baseRes = XXQDCredit(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        public BaseRes XXQDCredit(RCWechatReq Req)
        {

            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：XXQDCredit；客户端IP:" + CommonFun.GetClientIP() + "；" + guid+ ",参数：" + jsonService.SerializeObject(Req, true));

            BaseRes baseRes = new BaseRes();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            List<RCRule> RejectReasons = null;
            int useMonths = 0;
            string rc = string.Empty;
            try
            {
                #region 查询是否有未结清贷款
                Log4netAdapter.WriteInfo("有未结清贷款查询开始，" + guid);
                VbsCustomerOpr opr = new VbsCustomerOpr();
                var bidList = opr.GetCustomerHistoryRecord(Req.Identitycard);
                KaKaDai kkd = new KaKaDai();
                bool hasClear = kkd.HasIsRepayment(bidList);
                Log4netAdapter.WriteInfo("有未结清贷款查询结束，" + guid);

                #endregion

                ISummary summaryService = NetSpiderFactoryManager.GetSummaryService();
                ICalls callsService = NetSpiderFactoryManager.GetCallsService();//
                //SummaryEntity summaryEntity = GetMobileSummary(Req.Identitycard, Req.Mobile);
                SummaryEntity summaryEntity = GetMobileSummary(Req.MobileId);

                if (summaryEntity == null)
                {
                    baseRes.StatusDescription = "手机账单未返回";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                if (!summaryEntity.Regdate.IsEmpty())
                {
                    useMonths = CommonFun.GetIntervalOf2DateTime(DateTime.Now, DateTime.Parse(summaryEntity.Regdate), "M");
                }

                #region 变量赋值
                xmllist.Clear();
                xmllist.Add(new VBXML { VB_COL = "Mobile_Is_Checked", VB_VALUE = Req.Mobile_Is_Checked == null ? "" : Req.Mobile_Is_Checked });//[申请人]地区（线上）
                xmllist.Add(new VBXML { VB_COL = "region_online", VB_VALUE = Req.Region == null ? "" : Req.Region });//[申请人]地区（线上）
                xmllist.Add(new VBXML { VB_COL = "Mobile_Score", VB_VALUE = Req.MobileScore.ToString() });//手机评分
                xmllist.Add(new VBXML { VB_COL = "XXQD_AGE", VB_VALUE = ChinaIDCard.GetAge(Req.Identitycard).ToString() });//[星星]借款人年龄
                xmllist.Add(new VBXML { VB_COL = "XXQD_SEX", VB_VALUE = ChinaIDCard.GetSex(Req.Identitycard) });//[星星]借款人性别
                xmllist.Add(new VBXML { VB_COL = "XXQD_HOUSEHOLD", VB_VALUE = Req.IsLocal });//[星星]是否本地籍
                xmllist.Add(new VBXML { VB_COL = "XXQD_HAS_UNSETTLED_LOAN", VB_VALUE = hasClear ? "是" : "否" });//[星星]是否有未结清贷款
                xmllist.Add(new VBXML { VB_COL = "XXQD_IS985", VB_VALUE = Req.IS985.IsEmpty() ? "" : Req.IS985 });//是否985高校
                xmllist.Add(new VBXML { VB_COL = "XXQD_IS211", VB_VALUE = Req.IS211.IsEmpty() ? "" : Req.IS211 });//是否211高校

                //2016-6-3 新增
                xmllist.Add(new VBXML { VB_COL = "XXQD_Customer_Channel", VB_VALUE = Req.Customer_Channel == null ? "" : Req.Customer_Channel });//[星星]获客渠道

                xmllist.Add(new VBXML { VB_COL = "MOBILE_ISREALNAME", VB_VALUE = summaryEntity.IsRealNameAuth == 1 ? "是" : "否" });//[手机]手机是否实名
                xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_MONTHS", VB_VALUE = useMonths.ToString() });//[手机]手机使用年限
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = summaryEntity.ThreeMonthCallRecordAmount.ToString() });//[手机]最近3个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_6MTH", VB_VALUE = summaryEntity.SixMonthCallRecordAmount.ToString() });//[手机]最近6个月消费金额
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = summaryEntity.ThreeMonthCallRecordCount.ToString() });//[手机]最近3个月通话次数
                xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_6MTH", VB_VALUE = summaryEntity.SixMonthCallRecordCount.ToString() });//[手机]最近6个月通话次数

                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_NAME", VB_VALUE = Req.University });//[学信网]大学名称
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_ISFULLTIME", VB_VALUE = Req.University_Isfulltime });//[学信网]是否全日制
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_GRADE", VB_VALUE = Req.University_Grade });//[学信网]大学年级
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_GRADUATE_REMAINMONTH", VB_VALUE = Req.University_Graduate_Remainmonth });//[学信网]距离毕业月数
                xmllist.Add(new VBXML { VB_COL = "XXQD_PRODUCT_TYPE", VB_VALUE = Req.ProductType });//[星星]产品类型
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_ISNEWSTUDENT", VB_VALUE = Req.University_IsNewstudent });// [学信网]是否新生
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_ISATSCHOOL", VB_VALUE = Req.University_IsAtSchool });//[学信网]是否在校学生
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_LEVEL", VB_VALUE = Req.University_level });//[学信网]层次
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_ISONSERVICE", VB_VALUE = Req.University_IsOnService });//[学信网]所在高校是否开通服务

                double DurationOverTime = 0;
                if (Req.Enrollment_Date != null && Req.Leaving_Date != null)
                {
                    DurationOverTime = CommonFun.GetIntervalOf2DateTime((DateTime)Req.Leaving_Date, (DateTime)Req.Enrollment_Date, "M");
                    DurationOverTime = DurationOverTime / 12 - Req.Schooling_Length - 0.25;
                }
                xmllist.Add(new VBXML { VB_COL = "UNIVERSITY_DurationOverTime", VB_VALUE = DurationOverTime > 0 ? "是" : "否" });//[学信网]学制年限超时

                #region 2015-12-09 新增决策变量
                MobileOpr mobileOpr = new MobileOpr();
                IList<Call> callsList = mobileOpr.GetCallsById(Req.MobileId);
                string xxqdWeChatService = AppSettings.xxqdWeChatService;
                HttpItem httpItem = new HttpItem()
               {
                   Method = "GET",
                   URL = xxqdWeChatService
               };
                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                List<string> mobileList = jsonService.GetArrayFromParse(httpResults.Html, "Content");
                int times = mobileList.Sum(mobile => callsList.Count(o => o.OtherCallPhone == mobile && o.InitType == "被叫" && Convert.ToDateTime(o.StartTime) >= DateTime.Now.AddMonths(-3)));
                
                xmllist.Add(new VBXML { VB_COL = "XXQD_CALL_TIMES", VB_VALUE = times.ToString() });//[星星]同业催收次数
                xmllist.Add(new VBXML { VB_COL = "XXQD_EXXMINE_TIME", VB_VALUE = DateTime.Now.Month.ToString() });//[星星]审批时间
                #endregion

                #region 2016-2-23 添加鹏元变量
                xmllist.Add(new VBXML { VB_COL = "SRC_PENGYUAN_SCORE", VB_VALUE = Req.Pengyuan_Score });//[学籍认证]鹏元评分
                xmllist.Add(new VBXML { VB_COL = "SRC_PENGYUAN_AUTH_MODE", VB_VALUE = Req.Auth_Mode });//[学籍认证]认证模式
                #endregion

                Log4netAdapter.WriteInfo("决策开始:" +guid );
                rc = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_XXQD, Req.Identitycard, "", ServiceConstants.RC_Model_XXQD, SerializationHelper.SerializeToXml(xmllist));
                Log4netAdapter.WriteInfo("决策结束:" + guid);
                decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                //BaseMongo baseMongo = new BaseMongo();
                //RCStorage rcStoreage = new RCStorage();
                //rcStoreage.Params = xmllist;
                //rcStoreage.Result = decisionResult;
                //rcStoreage.BusId = Req.Identitycard;
                //rcStoreage.BusType = "XXQD";
                //baseMongo.Insert<RCStorage>(rcStoreage, "XXQD_RCStorage" + DateTime.Now.ToString(Consts.DateFormatString7));

                decimal Formalities_purse = 0;//手续费_零钱包
                decimal Formalities_stage = 0;//手续费_分期
                decimal MonthlyInterestRate_purse = 0;//月利率_零钱包
                decimal MonthlyInterestRate_stage = 0;//月利率_分期
                decimal MonthlyServiceRate_purse = 0;//月服务费率_零钱包
                decimal MonthlyServiceRate_stage = 0;//月服务费率_分期
                decimal CreditAmount = 0;//审批金额
                decimal LoanServiceAmount_purse = 0;//借款服务费_零钱包
                decimal LoanFormalitiesRate_purse = 0;//借款手续费率_零钱包
                decimal LoanServiceRate_purse = 0;//借款服务费率_零钱包
                if (decisionResult != null)
                {
                    if (decisionResult.Result == "通过")
                    {
                        //借款服务费_零钱包
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*借款服务费_零钱包"))
                        {
                            LoanServiceAmount_purse = decisionResult.RuleResultCanShowSets["*借款服务费_零钱包"].ToDecimal(0);
                        }
                        //手续费
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*借款手续费率_零钱包"))
                        {
                            LoanFormalitiesRate_purse = decisionResult.RuleResultCanShowSets["*借款手续费率_零钱包"].ToDecimal(0);
                        }
                        //手续费
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*借款服务费率_零钱包"))
                        {
                            LoanServiceRate_purse = decisionResult.RuleResultCanShowSets["*借款服务费率_零钱包"].ToDecimal(0);
                        }
                        //手续费
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*手续费_零钱包"))
                        {
                            Formalities_purse = decisionResult.RuleResultCanShowSets["*手续费_零钱包"].ToDecimal(0);
                        }
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*手续费_分期"))
                        {
                            Formalities_stage = decisionResult.RuleResultCanShowSets["*手续费_分期"].ToDecimal(0);
                        }
                        //月利率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*月利率_零钱包"))
                        {
                            MonthlyInterestRate_purse = decisionResult.RuleResultCanShowSets["*月利率_零钱包"].ToDecimal(0);
                        }
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*月利率_分期"))
                        {
                            MonthlyInterestRate_stage = decisionResult.RuleResultCanShowSets["*月利率_分期"].ToDecimal(0);
                        }
                        //月服务费率
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*月服务费率_零钱包"))
                        {
                            MonthlyServiceRate_purse = decisionResult.RuleResultCanShowSets["*月服务费率_零钱包"].ToDecimal(0);
                        }
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*月服务费率_分期"))
                        {
                            MonthlyServiceRate_stage = decisionResult.RuleResultCanShowSets["*月服务费率_分期"].ToDecimal(0);
                        }
                        //审批金额
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_ExamineLoanMoney))
                        {
                            CreditAmount = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_ExamineLoanMoney].ToDecimal(0);
                        }

                        baseRes.StatusCode = ServiceConsts.StatusCode_success;
                        baseRes.StatusDescription = guid + "授信成功";
                    }
                    else
                    {
                        RejectReasons = new List<RCRule>();
                        var RuleResultSets = decisionResult.RuleResultSets;

                        RCRule RCRuleModel = null;
                        foreach (var item in RuleResultSets)
                        {
                            //查询拒绝组拒绝的原因
                            if (item.Value.Result == "1" && item.Value.Rule.RU_GP_TP == 1)
                            {
                                RCRuleModel = new RCRule();
                                RCRuleModel.RuleName = item.Value.Rule.RU_NM;
                                RCRuleModel.RuleDesc = item.Value.Rule.RU_DESC;
                                RejectReasons.Add(RCRuleModel);
                            }
                        }

                        baseRes.StatusDescription = guid + "授信失败";
                    }
                }

                var rcResult = new
                {
                    LoanServiceAmount_purse = LoanServiceAmount_purse,
                    LoanFormalitiesRate_purse = LoanFormalitiesRate_purse,
                    LoanServiceRate_purse = LoanServiceRate_purse,
                    Formalities_purse = Formalities_purse,
                    Formalities_stage = Formalities_stage,
                    MonthlyInterestRate_purse = MonthlyInterestRate_purse,
                    MonthlyInterestRate_stage = MonthlyInterestRate_stage,
                    MonthlyServiceRate_purse = MonthlyServiceRate_purse,//月利率
                    MonthlyServiceRate_stage = MonthlyServiceRate_stage,//月服务费率
                    CreditAmount = CreditAmount,//授信金额
                    ValidDate = DateTime.Now.AddMonths(12).ToString(Consts.DateFormatString2),//有效期
                    RejectReasons = RejectReasons,//拒绝原因
                };

                #endregion


                baseRes.Result = jsonService.SerializeObject(rcResult);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(guid + "决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(guid + "接口：XXQDCredit,调用结束");
            return baseRes;
        }
        public BaseRes XXQDEduloan(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {


                //获得请求内容
                string reqText = stream.AsStringText(true);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                RCWechatReq Req = jsonService.DeserializeObject<RCWechatReq>(reqText);
                Log4netAdapter.WriteInfo(string.Format("接口：XXQDEduloan；客户端IP:{0},参数：{1}", CommonFun.GetClientIP(), reqText));

                #region 查询是否有未结清贷款
                VbsCustomerOpr opr = new VbsCustomerOpr();
                var bidList = opr.GetCustomerHistoryRecord(Req.Identitycard);
                KaKaDai kkd = new KaKaDai();
                bool hasClear = kkd.HasIsRepayment(bidList);
                #endregion

                DecisionResult decisionResult = new DecisionResult();
                RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
                List<VBXML> xmllist = new List<VBXML>();
                List<RCRule> RejectReasons = null;
                int mobileScore = 0;
                int useMonths = 0;
                string rc = string.Empty;
                try
                {
                    ISummary summaryService = NetSpiderFactoryManager.GetSummaryService();
                    ICalls callsService = NetSpiderFactoryManager.GetCallsService();//
                    ISocialSecurity socialService = NetSpiderFactoryManager.GetSocialSecurityService();
                    IProvidentFund providentService = NetSpiderFactoryManager.GetProvidentFundService();
                    ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();

                    SummaryEntity summaryEntity = GetMobileSummary(Req.MobileId);
                    CRD_HD_REPORTEntity reportEntity = reportService.GetByReportSn(Req.ReportSn, true);
                    #region 数据校验
                    if (summaryEntity == null)
                    {
                        baseRes.StatusDescription = "无手机账单数据";
                        return baseRes;
                    }
                    if (reportEntity == null)
                    {
                        baseRes.StatusDescription = "无征信数据";
                        return baseRes;
                    }
                    //校验人员姓名与身份证与注册信息是否一致
                    if (reportEntity.CertNo.Substring(14) != Req.Identitycard.Substring(14) || reportEntity.Name != Req.Name)
                    {
                        Log4netAdapter.WriteInfo("警告：征信姓名、身份证号与注册信息不一致");
                        baseRes.StatusDescription = "注册信息与征信数据不一致";
                        return baseRes;
                    }
                    #endregion

                    #region 学历映射
                    switch (Req.Education)
                    {
                        case "博士研究生": Req.Education = "硕士及上"; break;
                        case "硕士研究生": Req.Education = "硕士及上"; break;
                        case "本科": Req.Education = "本科"; break;
                        case "第二学士学位": Req.Education = "本科"; break;
                        case "专升本": Req.Education = "本科"; break;
                        case "专科": Req.Education = "大专"; break;
                        case "第二专科": Req.Education = "大专"; break;
                        case "专科(高职)": Req.Education = "大专"; break;
                    }
                    #endregion

                    if (!summaryEntity.Regdate.IsEmpty())
                    {
                        useMonths = CommonFun.GetIntervalOf2DateTime(DateTime.Now, DateTime.Parse(summaryEntity.Regdate), "M");
                    }

                    #region 手机评分
                    xmllist.Clear();
                    xmllist.Add(new VBXML { VB_COL = "Education", VB_VALUE = Req.Education });//[申请人]学历
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALLING_TIMES", VB_VALUE = summaryEntity.DAY90_CALLING_TIMES != null ? ((int)summaryEntity.DAY90_CALLING_TIMES).ToString() : "0" });//[评分_手机]90天内主叫次数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_PHONE_CNT", VB_VALUE = summaryEntity.CALLED_PHONE_CNT != null ? ((int)summaryEntity.CALLED_PHONE_CNT).ToString() : "0" });//[评分_手机]被叫联系人个数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_LOCAL_CALL_TIME", VB_VALUE = summaryEntity.LOCAL_CALL_TIME != null ? ((decimal)summaryEntity.LOCAL_CALL_TIME).ToString() : "0" });//[评分_手机]本地通话时长汇总
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_180_CALLING_SUBTTL", VB_VALUE = summaryEntity.DAY180_CALLING_SUBTTL != null ? ((decimal)summaryEntity.DAY180_CALLING_SUBTTL).ToString() : "0" });//[评分_手机]近180天内累计主叫套餐外话费
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALL_TTL_TIME", VB_VALUE = summaryEntity.DAY90_CALL_TTL_TIME != null ? ((decimal)summaryEntity.DAY90_CALL_TTL_TIME).ToString() : "0" });//[评分_手机]近90天内累计通话时长
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALL_TIMES", VB_VALUE = summaryEntity.DAY90_CALL_TIMES != null ? ((int)summaryEntity.DAY90_CALL_TIMES).ToString() : "0" });//[评分_手机]近90天内通话次数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_90DAY_CALLING_TTL_TIME", VB_VALUE = summaryEntity.DAY90_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.DAY90_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]近90天内主叫通话时长
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NET_LSTM6_ONL_FLOW", VB_VALUE = summaryEntity.NET_LSTM6_ONL_FLOW != null ? ((decimal)summaryEntity.NET_LSTM6_ONL_FLOW).ToString() : "0" });//[评分_手机]近六月内累计上网流量
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_DAY_CALLING_TTL_TIME", VB_VALUE = summaryEntity.DAY_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.DAY_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计白天主叫时长(9:00-18:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_TIMES", VB_VALUE = summaryEntity.CALLED_TIMES != null ? ((int)summaryEntity.CALLED_TIMES).ToString() : "0" });//[评分_手机]累计被叫次数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_TTL_TIME", VB_VALUE = summaryEntity.CALLED_TTL_TIME != null ? ((decimal)summaryEntity.CALLED_TTL_TIME).ToString() : "0" });//[评分_手机]累计被叫时长
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_MRNG_CALLED_TIMES", VB_VALUE = summaryEntity.MRNG_CALLED_TIMES != null ? ((int)summaryEntity.MRNG_CALLED_TIMES).ToString() : "0" });//[评分_手机]累计上午被叫次数(9:00-13:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALL_TTL_TIME", VB_VALUE = summaryEntity.CALL_TTL_TIME != null ? ((decimal)summaryEntity.CALL_TTL_TIME).ToString() : "0" });//[评分_手机]累计通话时长
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NIGHT_CALLED_TTL_TIME", VB_VALUE = summaryEntity.NIGHT_CALLED_TTL_TIME != null ? ((decimal)summaryEntity.NIGHT_CALLED_TTL_TIME).ToString() : "0" });//[评分_手机]累计晚间被叫时长(18:00-23:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_AFTN_CALL_TTL_TIME", VB_VALUE = summaryEntity.AFTN_CALL_TTL_TIME != null ? ((decimal)summaryEntity.AFTN_CALL_TTL_TIME).ToString() : "0" });//[评分_手机]累计下午通话时长(13:00-18:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_AFTN_CALLING_TTL_TIME", VB_VALUE = summaryEntity.AFTN_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.AFTN_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计下午主叫时长(13:00-18:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NIGHT_CALL_TTL_TIME", VB_VALUE = summaryEntity.NIGHT_CALL_TTL_TIME != null ? ((decimal)summaryEntity.NIGHT_CALL_TTL_TIME).ToString() : "0" });//[评分_手机]累计夜晚通话时长(23:00-9:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_NIGHT_CALLING_TTL_TIME", VB_VALUE = summaryEntity.NIGHT_CALLING_TTL_TIME != null ? ((decimal)summaryEntity.NIGHT_CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计夜晚主叫时长(23:00-9:00)
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLING_TTL_TIME", VB_VALUE = summaryEntity.CALLING_TTL_TIME != null ? ((decimal)summaryEntity.CALLING_TTL_TIME).ToString() : "0" });//[评分_手机]累计主叫时长
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_PH_USE_MONS", VB_VALUE = summaryEntity.PH_USE_MONS != null ? ((decimal)summaryEntity.PH_USE_MONS).ToString() : "0" });//[评分_手机]手机开卡时长至申请时间月数间隔
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALL_PHONE_CNT", VB_VALUE = summaryEntity.CALL_PHONE_CNT != null ? ((int)summaryEntity.CALL_PHONE_CNT).ToString() : "0" });//[评分_手机]所有联系人个数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CTT_DAYS_CNT", VB_VALUE = summaryEntity.CTT_DAYS_CNT != null ? ((int)summaryEntity.CTT_DAYS_CNT).ToString() : "0" });//[评分_手机]通话天数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLED_CTT_DAYS_CNT", VB_VALUE = summaryEntity.CALLED_CTT_DAYS_CNT != null ? ((int)summaryEntity.CALLED_CTT_DAYS_CNT).ToString() : "0" });//[评分_手机]有被叫的天数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_CALLING_CTT_DAYS_CNT", VB_VALUE = summaryEntity.CALLING_CTT_DAYS_CNT != null ? ((int)summaryEntity.CALLING_CTT_DAYS_CNT).ToString() : "0" });//[评分_手机]有主叫的通话天数
                    xmllist.Add(new VBXML { VB_COL = "SCR_CELL_MAX_PLAN_AMT", VB_VALUE = summaryEntity.MAX_PLAN_AMT != null ? ((decimal)summaryEntity.MAX_PLAN_AMT).ToString() : "0" });//[评分_手机]最大套餐金额

                    rc = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_XXQD, Req.Identitycard, "", ServiceConstants.RC_Model_KKD_MobileScore, SerializationHelper.SerializeToXml(xmllist));
                    decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();
                    if (decisionResult != null)
                    {
                        if (decisionResult.RuleResultCanShowSets.ContainsKey(ServiceConstants.RC_Result_KKD_MobileScore))
                        {
                            mobileScore = decisionResult.RuleResultCanShowSets[ServiceConstants.RC_Result_KKD_MobileScore].ToInt(0);
                        }
                    }
                    #endregion

                    #region 变量赋值
                    xmllist.Clear();
                    xmllist.Add(new VBXML { VB_COL = "region_online", VB_VALUE = Req.Region == null ? "" : Req.Region });//[申请人]地区（线上）
                    xmllist.Add(new VBXML { VB_COL = "Mobile_Score", VB_VALUE = mobileScore.ToString() });//手机评分
                    xmllist.Add(new VBXML { VB_COL = "Education", VB_VALUE = Req.Education });//学历
                    xmllist.Add(new VBXML { VB_COL = "IsVcreditNull", VB_VALUE = reportEntity.IsCreditBlank ? "1" : "0" });//[征信]征信空白
                    xmllist.Add(new VBXML { VB_COL = "VCREDIT_ADDLOANAGAIN", VB_VALUE = hasClear ? "是" : "否" });//[维信]是否加贷客户
                    xmllist.Add(new VBXML { VB_COL = "CustomerScore ", VB_VALUE = Req.CreditScore.ToString() });//[申请人]客户实际评分
                    xmllist.Add(new VBXML { VB_COL = "AGE", VB_VALUE = ChinaIDCard.GetAge(Req.Identitycard).ToString() });//[星星]借款人年龄
                    xmllist.Add(new VBXML { VB_COL = "XXQD_IS985", VB_VALUE = Req.IS985.IsEmpty() ? "" : Req.IS985 });//是否985高校
                    xmllist.Add(new VBXML { VB_COL = "XXQD_IS211", VB_VALUE = Req.IS211.IsEmpty() ? "" : Req.IS211 });//是否211高校
                    //2016-6-3 新增
                    xmllist.Add(new VBXML { VB_COL = "XXQD_Customer_Channel", VB_VALUE = Req.Customer_Channel == null ? "" : Req.Customer_Channel });//[星星]获客渠道
                    //2017-1-25 新增(JC[013])
                    xmllist.Add(new VBXML { VB_COL = "Front_Education", VB_VALUE = Req.Front_Education == null ? "" : Req.Front_Education });//[星星]获客渠道

                    xmllist.Add(new VBXML { VB_COL = "VBS_SocialScore", VB_VALUE = Req.SocialScore.ToString() });//[申请人]社保评分
                    //社保变量
                    if (Req.SocialType == "社保")
                    {
                        SocialSecurityEntity socialEntity = socialService.GetByIdentityCard(Req.Identitycard);

                        xmllist.Add(new VBXML { VB_COL = "SocialSecurity", VB_VALUE = socialEntity != null && socialEntity.SocialInsuranceBase != null ? socialEntity.SocialInsuranceBase.ToString() : "0" });//[申请人]社保缴费基数 
                        xmllist.Add(new VBXML { VB_COL = "SocialAllowanceBase ", VB_VALUE = "0" });//[申请人]公积金缴费基数
                    }
                    else if (Req.SocialType == "公积金")
                    {
                        ProvidentFundEntity providentEntity = providentService.GetByIdentityCard(Req.Identitycard);

                        xmllist.Add(new VBXML { VB_COL = "SocialAllowanceBase", VB_VALUE = providentEntity != null && providentEntity.SalaryBase != null ? providentEntity.SalaryBase.ToString() : "0" });//[申请人]公积金缴费基数 
                        xmllist.Add(new VBXML { VB_COL = "SocialSecurity ", VB_VALUE = "0" });//[申请人]社保缴费基数
                    }
                    else
                    {
                        xmllist.Add(new VBXML { VB_COL = "SocialAllowanceBase", VB_VALUE = "0" });//[申请人]公积金缴费基数 
                        xmllist.Add(new VBXML { VB_COL = "SocialSecurity ", VB_VALUE = "0" });//[申请人]社保缴费基数
                    }

                    //手机变量
                    xmllist.Add(new VBXML { VB_COL = "MOBILE_ISREALNAME", VB_VALUE = summaryEntity.IsRealNameAuth == 1 ? "是" : "否" });//[手机]手机是否实名
                    xmllist.Add(new VBXML { VB_COL = "MOBILE_USED_MONTHS", VB_VALUE = useMonths.ToString() });//[手机]手机使用年限
                    xmllist.Add(new VBXML { VB_COL = "MOBILE_CONSUM_3MTH", VB_VALUE = summaryEntity.ThreeMonthCallRecordAmount.ToString() });//[手机]最近3个月消费金额
                    xmllist.Add(new VBXML { VB_COL = "MOBILE_CALLCOUNT_3MTH", VB_VALUE = summaryEntity.ThreeMonthCallRecordCount.ToString() });//[手机]最近3个月通话次数
                    xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY", VB_VALUE = summaryEntity.CALLED_TIMES_IN30DAY.ToString() });//[手机]30天内危险号码被叫次数
                    xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY", VB_VALUE = summaryEntity.CALLED_TIMES_IN15DAY.ToString() });//[手机]15天内危险号码被叫次数
                    xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN30DAY_GRAY", VB_VALUE = summaryEntity.CALLED_TIMES_IN30DAY_Gray.ToString() });//[手机]30天内灰色号码被叫次数
                    xmllist.Add(new VBXML { VB_COL = "CALLED_TIMES_IN15DAY_GRAY", VB_VALUE = summaryEntity.CALLED_TIMES_IN15DAY_Gray.ToString() });//[手机]15天内灰色号码被叫次数

                    //征信变量
                    xmllist.Add(new VBXML { VB_COL = "Normal_Used_Rate", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALUSEDRATE != null ? reportEntity.CRD_STAT_LND.NORMALUSEDRATE.ToString() : "0" });//[网络版征信]正常信用卡使用率
                    xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_HOUSE_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT != null ? reportEntity.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT.ToString() : "0" });//[网络版征信]住房贷款发生过90天以上逾期的账户数(all)
                    xmllist.Add(new VBXML { VB_COL = "ALL_LOAN_OTHER_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT != null ? reportEntity.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT.ToString() : "0" });//[网络版征信]其他贷款发生过90天以上逾期的账户数(all)
                    xmllist.Add(new VBXML { VB_COL = "ALL_CREDIT_DELAY90_CNT", VB_VALUE = reportEntity.CRD_STAT_LND.ALLCREDITDELAY90CNT != null ? reportEntity.CRD_STAT_LND.ALLCREDITDELAY90CNT.ToString() : "0" });//[网络版征信]信用卡发生过90天以上逾期的账户数(all)

                    xmllist.Add(new VBXML { VB_COL = "CREDIT_IS_FORCED_RECORD ", VB_VALUE = reportEntity.PubInfoSummary.ForceexctnCount > 0 ? "是" : "否" });//[网络版征信]是否有强制执行记录
                    xmllist.Add(new VBXML { VB_COL = "COUNT_CARD_IN3M ", VB_VALUE = reportEntity.CRD_STAT_QR.M3CREDITCNT.ToString() });//3个月内信用卡审批查询次数
                    xmllist.Add(new VBXML { VB_COL = "COUNT_loan_IN3M ", VB_VALUE = reportEntity.CRD_STAT_QR.M3LOANCNT.ToString() });//3个月内贷款审批查询次数
                    xmllist.Add(new VBXML { VB_COL = "COUNT_Nonbank_IN3M", VB_VALUE = reportEntity.CRD_STAT_QR.COUNT_Nonbank_IN3M != null ? reportEntity.CRD_STAT_QR.COUNT_Nonbank_IN3M.ToString() : "0" });//[征信]3个月内非银机构查询次数

                    xmllist.Add(new VBXML { VB_COL = "lnd_max_normal_Credit_Limit_Amount", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX != null ? reportEntity.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX.ToString() : "0" });//正常状态贷记卡最大授信额度
                    xmllist.Add(new VBXML { VB_COL = "lnd_Curr_Overdue_Amount", VB_VALUE = reportEntity.CRD_STAT_LND.CREDITDLQAMOUNT != null ? reportEntity.CRD_STAT_LND.CREDITDLQAMOUNT.ToString() : "0" });//贷记卡当前逾期金额
                    xmllist.Add(new VBXML { VB_COL = "lnd_Used_Credit_Limit_Amount", VB_VALUE = reportEntity.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT != null ? reportEntity.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT.ToString() : "0" });//贷记卡已使用额度
                    xmllist.Add(new VBXML { VB_COL = "lnd_max_overdue_percent", VB_VALUE = reportEntity.CRD_STAT_LND.lnd_max_overdue_percent != null ? reportEntity.CRD_STAT_LND.lnd_max_overdue_percent.ToString() : "0" });//贷记卡最大逾期次数占比
                    xmllist.Add(new VBXML { VB_COL = "lnd_max_normal_Age", VB_VALUE = reportEntity.CRD_STAT_LND.lnd_max_normal_Age.ToString() });//正常状态最大授信额度卡的卡龄
                    xmllist.Add(new VBXML { VB_COL = "lnd_normal_count", VB_VALUE = reportEntity.CRD_STAT_LND.NORMALCARDNUM != null ? reportEntity.CRD_STAT_LND.NORMALCARDNUM.ToString() : "0" });//状态为正常的贷记卡张数
                    xmllist.Add(new VBXML { VB_COL = "loand_Badrecord", VB_VALUE = reportEntity.CRD_STAT_LND.loand_Badrecord != null ? reportEntity.CRD_STAT_LND.loand_Badrecord.ToString() : "0" });//贷记卡呆账数
                    xmllist.Add(new VBXML { VB_COL = "stncard_Badrecord", VB_VALUE = reportEntity.CRD_STAT_LND.stncard_Badrecord != null ? reportEntity.CRD_STAT_LND.stncard_Badrecord.ToString() : "0" });//准贷记卡呆账数

                    xmllist.Add(new VBXML { VB_COL = "ln_Curr_Overdue_Amount", VB_VALUE = reportEntity.CRD_STAT_LN.LOANDLQAMOUNT != null ? reportEntity.CRD_STAT_LN.LOANDLQAMOUNT.ToString() : "0" });//贷款当前逾期金额
                    xmllist.Add(new VBXML { VB_COL = "ln_housing_fund_amount", VB_VALUE = reportEntity.CRD_STAT_LN.ln_housing_fund_amount != null ? reportEntity.CRD_STAT_LN.ln_housing_fund_amount.ToString() : "0" });//个人住房公积金贷款额
                    xmllist.Add(new VBXML { VB_COL = "ln_shopfront_amount", VB_VALUE = reportEntity.CRD_STAT_LN.ln_shopfront_amount != null ? reportEntity.CRD_STAT_LN.ln_shopfront_amount.ToString() : "0" });//个人住房商铺贷款额
                    xmllist.Add(new VBXML { VB_COL = "ln_housing_mortgage_amount", VB_VALUE = reportEntity.CRD_STAT_LN.ln_housing_mortgage_amount != null ? reportEntity.CRD_STAT_LN.ln_housing_mortgage_amount.ToString() : "0" });//个人住房按揭贷款额

                    xmllist.Add(new VBXML { VB_COL = "Monthly_Other_Mortgage_Payment_Total", VB_VALUE = reportEntity.CRD_STAT_LN.Monthly_Other_Mortgage_Payment_Total != null ? reportEntity.CRD_STAT_LN.Monthly_Other_Mortgage_Payment_Total.ToString() : "0" });//其他贷款月按揭还款总额
                    xmllist.Add(new VBXML { VB_COL = "StnCard_UseCreditLimit", VB_VALUE = reportEntity.CRD_STAT_LND.StnCard_UseCreditLimit != null ? reportEntity.CRD_STAT_LND.StnCard_UseCreditLimit.ToString() : "0" });//准贷记卡透支余额
                    xmllist.Add(new VBXML { VB_COL = "Monthly_Mortgage_Payment_Max", VB_VALUE = reportEntity.CRD_STAT_LN.Monthly_Mortgage_Payment_Max != null ? reportEntity.CRD_STAT_LN.Monthly_Mortgage_Payment_Max.ToString() : "0" });//最大住房月按揭还款额
                    xmllist.Add(new VBXML { VB_COL = "Monthly_Mortgage_Payment_Total", VB_VALUE = reportEntity.CRD_STAT_LN.Monthly_Mortgage_Payment_Total != null ? reportEntity.CRD_STAT_LN.Monthly_Mortgage_Payment_Total.ToString() : "0" });//住房月按揭还款总额
                    xmllist.Add(new VBXML { VB_COL = "Monthly_Commercial_Mortgage_Payment_Total", VB_VALUE = reportEntity.CRD_STAT_LN.Monthly_Commercial_Mortgage_Payment_Total != null ? reportEntity.CRD_STAT_LN.Monthly_Commercial_Mortgage_Payment_Total.ToString() : "0" });//商用房月按揭还款总额 

                    rc = rcServcie.GetRuleResultByCustomWithIdentityNoCallFrom(ServiceConstants.RC_Model_XXQD, Req.Identitycard, Req.Name, ServiceConstants.RC_Model_XXQD_EduLoan, SerializationHelper.SerializeToXml(xmllist));
                    decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();

                    BaseMongo baseMongo = new BaseMongo();
                    RCStorage rcStoreage = new RCStorage();
                    rcStoreage.Params = xmllist;
                    rcStoreage.Result = decisionResult;
                    rcStoreage.BusId = Req.Identitycard;
                    rcStoreage.BusType = "XXQD_EduLoan";
                    baseMongo.Insert<RCStorage>(rcStoreage, "XXQD_RCStorage" + DateTime.Now.ToString(Consts.DateFormatString7));

                    decimal FormalitiesRate = 0;//手续费率
                    decimal MonthlyInterestRate = 0;//月利率
                    decimal MonthlyServiceRate = 0;//月服务费率
                    decimal CreditAmount = 0;//审批金额
                    string CustomerLevel = string.Empty;//客户评级
                    if (decisionResult != null)
                    {
                        if (decisionResult.Result == "通过")
                        {
                            //手续费率
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*手续费率"))
                            {
                                FormalitiesRate = decisionResult.RuleResultCanShowSets["*手续费率"].ToDecimal(0);
                            }
                            //月利率
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*月利率"))
                            {
                                MonthlyInterestRate = decisionResult.RuleResultCanShowSets["*月利率"].ToDecimal(0);
                            }
                            //月服务费率
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*月服务费率"))
                            {
                                MonthlyServiceRate = decisionResult.RuleResultCanShowSets["*月服务费率"].ToDecimal(0);
                            }
                            //审批金额
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*审批金额"))
                            {
                                CreditAmount = decisionResult.RuleResultCanShowSets["*审批金额"].ToDecimal(0);
                            }
                            //客户评级
                            if (decisionResult.RuleResultCanShowSets.ContainsKey("*客户评级"))
                            {
                                CustomerLevel = decisionResult.RuleResultCanShowSets["*客户评级"];
                            }

                            baseRes.StatusCode = ServiceConsts.StatusCode_success;
                            baseRes.StatusDescription = "授信成功";
                        }
                        else
                        {
                            RejectReasons = new List<RCRule>();
                            var RuleResultSets = decisionResult.RuleResultSets;

                            RCRule RCRuleModel = null;
                            foreach (var item in RuleResultSets)
                            {
                                //查询拒绝组拒绝的原因
                                if (item.Value.Result == "1" && item.Value.Rule.RU_GP_TP == 1)
                                {
                                    RCRuleModel = new RCRule();
                                    RCRuleModel.RuleName = item.Value.Rule.RU_NM;
                                    RCRuleModel.RuleDesc = item.Value.Rule.RU_DESC;
                                    RejectReasons.Add(RCRuleModel);
                                }
                            }

                            baseRes.StatusDescription = "授信失败";
                        }
                    }

                    var rcResult = new
                    {
                        FormalitiesRate = FormalitiesRate,//手续费率
                        MonthlyInterestRate = MonthlyInterestRate,//月利率
                        MonthlyServiceRate = MonthlyServiceRate,//月服务费率
                        CreditAmount = CreditAmount,//授信金额
                        ValidDate = DateTime.Now.AddMonths(12).ToString(Consts.DateFormatString2),//有效期
                        RejectReasons = RejectReasons,//拒绝原因
                        MobileScore = mobileScore,
                        CustomerLevel = CustomerLevel
                    };

                    #endregion

                    baseRes.Result = jsonService.SerializeObject(rcResult);
                }
                catch (Exception e)
                {
                    Log4netAdapter.WriteError(Req.Identitycard + "决策异常；", e);
                    baseRes.StatusDescription = e.Message;
                    baseRes.StatusCode = ServiceConsts.StatusCode_error;
                }
                baseRes.EndTime = DateTime.Now.ToString();
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }
        #endregion

        #region
        private SummaryEntity GetMobileSummary(string identitycard, string mobile)
        {
            SummaryEntity retSummary = null;
            try
            {
                IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
                Variable_mobile_summaryEntity variableEntity = variableService.GetByBusIdentityNoAndMobile(identitycard, mobile);
                //var sources = source.Split('_');
                //if (sources.Count() == 2)
                //    variableEntity = variableService.GetBySourceIdAndSourceType(sources[1], sources[0]);
                if (variableEntity != null)
                {
                    retSummary = new SummaryEntity();
                    retSummary.OneMonthCallRecordAmount = variableEntity.One_Month_Call_Record_Amount;
                    retSummary.OneMonthCallRecordCount = variableEntity.One_Month_Call_Record_Count;
                    retSummary.ThreeMonthCallRecordAmount = variableEntity.Three_Month_Call_Record_Amount;
                    retSummary.ThreeMonthCallRecordCount = variableEntity.Three_Month_Call_Record_Count;
                    retSummary.SixMonthCallRecordAmount = variableEntity.Six_Month_Call_Record_Amount;
                    retSummary.SixMonthCallRecordCount = variableEntity.Six_Month_Call_Record_Count;

                    retSummary.MAX_PLAN_AMT = variableEntity.MAX_PLAN_AMT;
                    retSummary.DAY90_CALLING_TIMES = variableEntity.DAY90_CALLING_TIMES;
                    retSummary.CALLED_PHONE_CNT = variableEntity.CALLED_PHONE_CNT;
                    retSummary.LOCAL_CALL_TIME = variableEntity.LOCAL_CALL_TIME;
                    retSummary.DAY180_CALLING_SUBTTL = variableEntity.DAY180_CALLING_SUBTTL;
                    retSummary.DAY90_CALL_TTL_TIME = variableEntity.DAY90_CALL_TTL_TIME;
                    retSummary.DAY90_CALL_TIMES = variableEntity.DAY90_CALL_TIMES;
                    retSummary.DAY90_CALLING_TTL_TIME = variableEntity.DAY90_CALLING_TTL_TIME;
                    retSummary.NET_LSTM6_ONL_FLOW = variableEntity.NET_LSTM6_ONL_FLOW;
                    retSummary.DAY_CALLING_TTL_TIME = variableEntity.DAY_CALLING_TTL_TIME;
                    retSummary.CALLED_TIMES = variableEntity.CALLED_TIMES;
                    retSummary.CALLED_TTL_TIME = variableEntity.CALLED_TTL_TIME;
                    retSummary.MRNG_CALLED_TIMES = variableEntity.MRNG_CALLED_TIMES;
                    retSummary.CALL_TTL_TIME = variableEntity.CALL_TTL_TIME;
                    retSummary.NIGHT_CALLED_TTL_TIME = variableEntity.NIGHT_CALLED_TTL_TIME;
                    retSummary.AFTN_CALL_TTL_TIME = variableEntity.AFTN_CALL_TTL_TIME;
                    retSummary.AFTN_CALLING_TTL_TIME = variableEntity.AFTN_CALLING_TTL_TIME;
                    retSummary.NIGHT_CALL_TTL_TIME = variableEntity.NIGHT_CALL_TTL_TIME;
                    retSummary.NIGHT_CALLING_TTL_TIME = variableEntity.NIGHT_CALLING_TTL_TIME;
                    retSummary.CALLING_TTL_TIME = variableEntity.CALLING_TTL_TIME;
                    retSummary.PH_USE_MONS = variableEntity.PH_USE_MONS;
                    retSummary.CALL_PHONE_CNT = variableEntity.CALL_PHONE_CNT;
                    retSummary.CTT_DAYS_CNT = variableEntity.CTT_DAYS_CNT;
                    retSummary.CALLED_CTT_DAYS_CNT = variableEntity.CALLED_CTT_DAYS_CNT;
                    retSummary.CALLING_CTT_DAYS_CNT = variableEntity.CALLING_CTT_DAYS_CNT;
                    retSummary.CALLED_TIMES_IN30DAY = (int)variableEntity.CALLED_TIMES_IN30DAY;
                    retSummary.CALLED_TIMES_IN15DAY = (int)variableEntity.CALLED_TIMES_IN15DAY;
                    retSummary.CALLED_TIMES_IN30DAY_Gray = (int)variableEntity.CALLED_TIMES_IN30DAY_Gray;
                    retSummary.CALLED_TIMES_IN15DAY_Gray = (int)variableEntity.CALLED_TIMES_IN15DAY_Gray;
                    retSummary.Regdate = variableEntity.Regdate;
                }

            }
            catch (Exception e)
            {

            }
            return retSummary;

        }
        private SummaryEntity GetMobileSummary(string source)
        {
            SummaryEntity retSummary = null;
            try
            {
                IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
                Variable_mobile_summaryEntity variableEntity = null;
                var sources = source.Split('_');
                if (sources.Count() == 2)
                    variableEntity = variableService.GetBySourceIdAndSourceType(sources[1], sources[0]);
                if (variableEntity != null)
                {
                    retSummary = new SummaryEntity();
                    retSummary.IsRealNameAuth = variableEntity.IsRealNameAuth;
                    retSummary.OneMonthCallRecordAmount = variableEntity.One_Month_Call_Record_Amount;
                    retSummary.OneMonthCallRecordCount = variableEntity.One_Month_Call_Record_Count;
                    retSummary.ThreeMonthCallRecordAmount = variableEntity.Three_Month_Call_Record_Amount;
                    retSummary.ThreeMonthCallRecordCount = variableEntity.Three_Month_Call_Record_Count;
                    retSummary.SixMonthCallRecordAmount = variableEntity.Six_Month_Call_Record_Amount;
                    retSummary.SixMonthCallRecordCount = variableEntity.Six_Month_Call_Record_Count;

                    retSummary.MAX_PLAN_AMT = variableEntity.MAX_PLAN_AMT;
                    retSummary.DAY90_CALLING_TIMES = variableEntity.DAY90_CALLING_TIMES;
                    retSummary.CALLED_PHONE_CNT = variableEntity.CALLED_PHONE_CNT;
                    retSummary.LOCAL_CALL_TIME = variableEntity.LOCAL_CALL_TIME;
                    retSummary.DAY180_CALLING_SUBTTL = variableEntity.DAY180_CALLING_SUBTTL;
                    retSummary.DAY90_CALL_TTL_TIME = variableEntity.DAY90_CALL_TTL_TIME;
                    retSummary.DAY90_CALL_TIMES = variableEntity.DAY90_CALL_TIMES;
                    retSummary.DAY90_CALLING_TTL_TIME = variableEntity.DAY90_CALLING_TTL_TIME;
                    retSummary.NET_LSTM6_ONL_FLOW = variableEntity.NET_LSTM6_ONL_FLOW;
                    retSummary.DAY_CALLING_TTL_TIME = variableEntity.DAY_CALLING_TTL_TIME;
                    retSummary.CALLED_TIMES = variableEntity.CALLED_TIMES;
                    retSummary.CALLED_TTL_TIME = variableEntity.CALLED_TTL_TIME;
                    retSummary.MRNG_CALLED_TIMES = variableEntity.MRNG_CALLED_TIMES;
                    retSummary.CALL_TTL_TIME = variableEntity.CALL_TTL_TIME;
                    retSummary.NIGHT_CALLED_TTL_TIME = variableEntity.NIGHT_CALLED_TTL_TIME;
                    retSummary.AFTN_CALL_TTL_TIME = variableEntity.AFTN_CALL_TTL_TIME;
                    retSummary.AFTN_CALLING_TTL_TIME = variableEntity.AFTN_CALLING_TTL_TIME;
                    retSummary.NIGHT_CALL_TTL_TIME = variableEntity.NIGHT_CALL_TTL_TIME;
                    retSummary.NIGHT_CALLING_TTL_TIME = variableEntity.NIGHT_CALLING_TTL_TIME;
                    retSummary.CALLING_TTL_TIME = variableEntity.CALLING_TTL_TIME;
                    retSummary.PH_USE_MONS = variableEntity.PH_USE_MONS;
                    retSummary.CALL_PHONE_CNT = variableEntity.CALL_PHONE_CNT;
                    retSummary.CTT_DAYS_CNT = variableEntity.CTT_DAYS_CNT;
                    retSummary.CALLED_CTT_DAYS_CNT = variableEntity.CALLED_CTT_DAYS_CNT;
                    retSummary.CALLING_CTT_DAYS_CNT = variableEntity.CALLING_CTT_DAYS_CNT;
                    retSummary.CALLED_TIMES_IN30DAY = (int)variableEntity.CALLED_TIMES_IN30DAY;
                    retSummary.CALLED_TIMES_IN15DAY = (int)variableEntity.CALLED_TIMES_IN15DAY;
                    retSummary.CALLED_TIMES_IN30DAY_Gray = (int)variableEntity.CALLED_TIMES_IN30DAY_Gray;
                    retSummary.CALLED_TIMES_IN15DAY_Gray = (int)variableEntity.CALLED_TIMES_IN15DAY_Gray;
                    retSummary.Regdate = variableEntity.Regdate;
                }

            }
            catch (Exception e)
            {

            }
            return retSummary;

        }

        #endregion
    }
}