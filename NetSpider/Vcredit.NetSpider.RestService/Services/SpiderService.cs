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
using Vcredit.NetSpider.Parser;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.RestService.Contracts;
using Vcredit.NetSpider.WorkFlow;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.RestService.Operation;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class SpiderService : ISpiderService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();
        //ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();
        IMobileExecutor mobileExecutor = ExecutorManager.GetMobileExecutor();
        ITaobaoExecutor taobaoExecutor = ExecutorManager.GetTaobaoExecutor();
        IExecutor Executor = ExecutorManager.GetExecutor();
        IVcreditCertifyExecutor vcertExecutor = ExecutorManager.GetVcreditCertifyExecutor();
        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();

        IRuntimeService runService = ProcessEngine.GetRuntimeService();
        IVerCodeParserService vercodeService = ParserServiceManager.GetVerCodeParserService();
        CookieCollection cookies = new CookieCollection();
        #endregion

        public SpiderService()
        {
            //string token =Chk.IsNull(HttpContext.Current.Request.QueryString["toke"]);
        }

        #region 数据采集服务初始化
        public BaseRes SpiderServiceInit()
        {
            BaseRes Res = new BaseRes();
            try
            {

                string token = CommonFun.GetGuidID();
                Res.Token = token;
                Res.StatusDescription = "数据采集服务已初始化,token有效期为5分钟";
                Res.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConstants.StatusCode_error;
                Res.StatusDescription = "数据采集服务初始化异常," + e.Message;
            }
            return Res;
        }
        #endregion

        #region 查询淘宝店铺销售总额
        public TaobaoSellerRes GetTaobaoSellerTotalAmountForJson(string sellerUrl)
        {
            return GetTaobaoSellerTotalAmount(sellerUrl);
        }
        public TaobaoSellerRes GetTaobaoSellerTotalAmountForXml(string sellerUrl)
        {
            return GetTaobaoSellerTotalAmount(sellerUrl);
        }
        public TaobaoSellerRes GetTaobaoSellerTotalAmount(string sellerUrl)
        {
            TaobaoSellerRes result = new TaobaoSellerRes();
            try
            {
                if (!sellerUrl.StartsWith("http"))
                {
                    sellerUrl = "http://" + sellerUrl;
                }
                if (ServiceConstants.TaobaoSellerRess.Keys.Contains(sellerUrl))
                {
                    TaobaoSellerRes tempRes = ServiceConstants.TaobaoSellerRess[sellerUrl];
                    if (tempRes.TotalAmount == -1)
                    {
                        result.StatusDescription = "报告未生成，请耐心等候……";
                    }
                    else
                    {
                        result.Result = tempRes.TotalAmount.ToString();
                        result.StatusDescription = "报告已生成";
                        result.TotalAmount = tempRes.TotalAmount;
                        result.UseMinute = tempRes.UseMinute;
                        result.CompanyName = tempRes.CompanyName;
                        result.OpenTime = tempRes.OpenTime;
                        result.MonthStatistics = tempRes.MonthStatistics;
                        result.ItemCount = tempRes.ItemCount;
                        ServiceConstants.TaobaoSellerRess.Remove(sellerUrl);
                    }
                }
                else
                {
                    TaobaoSellerRes tempRes = new TaobaoSellerRes();
                    tempRes.TotalAmount = -1;
                    //GetTaobaoSellerTotalAmountAsynQueryCaller caller = new GetTaobaoSellerTotalAmountAsynQueryCaller(GetTaobaoSellerTotalAmountAsynQuery);
                    //ServiceConstants.TaobaoSellerRess.Add(sellerUrl, tempRes);
                    //IAsyncResult async = caller.BeginInvoke(sellerUrl, null, null);

                    ServiceConstants.TaobaoSellerRess.Add(sellerUrl, tempRes);
                    new Thread((ThreadStart)delegate()
                    {
                        if (sellerUrl.Contains("tmall"))
                        {
                            tempRes = taobaoExecutor.GetTmallSellerTotalAmount(sellerUrl);
                        }
                        else
                        {
                            tempRes = taobaoExecutor.GetTaobaoSellerTotalAmount(sellerUrl);
                        }
                        ServiceConstants.TaobaoSellerRess[sellerUrl] = tempRes;
                    }).Start();
                    result.StatusDescription = "请4个小时后再获取此销售总额查询报告";
                }
                result.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                result.StatusCode = ServiceConstants.StatusCode_error;
                result.StatusDescription = e.Message;
            }
            return result;
        }

        #endregion

        #region 开始执行抓取淘宝商家店铺
        public BaseRes ExecutorSpiderTaobaoSellerForJson(string sellerUrl, string bid)
        {
            return ExecutorSpiderTaobaoSeller(sellerUrl, bid);
        }
        public BaseRes ExecutorSpiderTaobaoSellerForXml(string sellerUrl, string bid)
        {
            return ExecutorSpiderTaobaoSeller(sellerUrl, bid);
        }
        public BaseRes ExecutorSpiderTaobaoSeller(string sellerUrl, string bid)
        {
            BaseRes result = new BaseRes();
            try
            {
                if (!sellerUrl.StartsWith("http"))
                {
                    sellerUrl = "http://" + sellerUrl;
                }
                TaobaoSellerRes tempRes = new TaobaoSellerRes();

                new Thread((ThreadStart)delegate()
                {
                    if (sellerUrl.Contains("tmall"))
                    {
                        tempRes = taobaoExecutor.GetTmallSellerTotalAmount(sellerUrl);
                    }
                    else
                    {
                        tempRes = taobaoExecutor.GetTaobaoSellerTotalAmount(sellerUrl);
                    }
                    //回调接口，更新店铺成交总额到VBS
                    VBSSpiderServiceCallback.SpiderServiceCallbackSoapClient ServiceCallback = new VBSSpiderServiceCallback.SpiderServiceCallbackSoapClient();
                    bool isExc = ServiceCallback.ExecutorSpiderTaobaoSellerCallback(int.Parse(bid), Newtonsoft.Json.JsonConvert.SerializeObject(tempRes));
                    //int num = ServiceCallback.ExecutorSpiderTaobaoSellerCallback(int.Parse(bid), tempRes.TotalAmount);
                    Log4netAdapter.WriteInfo(String.Format("VBS开始执行抓取淘宝商家店铺回调函数执行完毕，返回值：{0}", isExc));
                }).Start();
                result.StatusCode = ServiceConstants.StatusCode_success;
            }
            catch (Exception e)
            {
                result.StatusCode = ServiceConstants.StatusCode_error;
                result.StatusDescription = e.Message;
            }

            return result;
        }

        #endregion

        #region 社保数据查询
        public BaseRes SocialSecurityInitForXml(string city)
        {
            return SocialSecurityInit(city);
        }

        public BaseRes SocialSecurityInitForJson(string city)
        {
            return SocialSecurityInit(city);
        }
        public BaseRes SocialSecurityInit(string city)
        {
            Log4netAdapter.WriteInfo("接口：SocialInsuranceQuery；客户端IP:" + CommonFun.GetClientIP());
            SocialSecurityOpr Opr = new SocialSecurityOpr();
            BaseRes Res = new BaseRes();
            return Res;
        }
        public SocialSecurityQueryRes SocialSecurityQueryForXml(string city)
        {
            return SocialSecurityQuery(city);
        }

        public SocialSecurityQueryRes SocialSecurityQueryForJson(string city)
        {
            return SocialSecurityQuery(city);
        }

        public SocialSecurityQueryRes SocialSecurityQuery(string city)
        {
            SocialSecurityQueryRes QueryRes = new SocialSecurityQueryRes();
            SocialSecurityOpr Opr = new SocialSecurityOpr();
            Log4netAdapter.WriteInfo("接口：SocialInsuranceQuery；客户端IP:" + CommonFun.GetClientIP());
            string NativeCity = string.Empty;//户籍市
            string CityName = string.Empty;//采集城市名称

            string username = Chk.IsNull(HttpContext.Current.Request.QueryString["username"]);
            string password = Chk.IsNull(HttpContext.Current.Request.QueryString["password"]);
            string vercode = Chk.IsNull(HttpContext.Current.Request.QueryString["vercode"]);
            try
            {
                city = city.ToUpper();
                if (city == ServiceConstants.SocialSecurityCity_Qingdao)
                {
                    CityName = "青岛";
                    Log4netAdapter.WriteInfo("青岛社保信息开始采集");
                    QueryRes = Opr.QueryQingDao(username, password);
                }
                else if (city == ServiceConstants.SocialSecurityCity_Shanghai)
                {
                    CityName = "上海";
                    Log4netAdapter.WriteInfo("上海社保信息开始采集");
                    QueryRes = Opr.QueryShangHai(username, password);

                }
                //社保采集城市
                QueryRes.SocialSecurityCity = CityName;
                NativeCity = ChinaIDCard.GetAddress(QueryRes.IdentityCard.Substring(0, 4) + "00");
                if (NativeCity.Contains(CityName))
                {
                    QueryRes.IsLocal = true;
                }
                ////计算最近24个月的缴费情况
                //string PayTime = string.Empty;
                //string Payment_State = string.Empty;
                //for (int i = 0; i <= 23; i++)
                //{
                //    PayTime = DateTime.Now.AddMonths(-i).ToString("yyyyMM");

                //    var query = QueryRes.Details.Where(o => o.PayTime == PayTime).FirstOrDefault();
                //    if (query != null)
                //    {
                //        Payment_State += "/";//缴费
                //    }
                //    else
                //    {
                //        Payment_State += "N";//未缴费
                //    }
                //}
                //QueryRes.Payment_State = Payment_State;

                ////性别
                //if (QueryRes.Sex.IsEmpty())
                //{
                //    QueryRes.Sex = ChinaIDCard.GetSex(QueryRes.IdentityCard);
                //}
                ////出生日期
                //if (QueryRes.BirthDate.IsEmpty())
                //{
                //    QueryRes.BirthDate = ChinaIDCard.GetBirthDate(QueryRes.IdentityCard);
                //}


            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("社保信息采集异常", e);
                QueryRes.StatusCode = ServiceConstants.StatusCode_error;
                QueryRes.StatusDescription = e.Message;
            }
            return QueryRes;
        }
        #endregion

        #region 移动账单查询
        public BaseRes PhoneLoginForXml()
        {
            return PhoneLogin();
        }

        public BaseRes PhoneLoginForJson()
        {
            return PhoneLogin();
        }
        public BaseRes PhoneLogin()
        {
            Log4netAdapter.WriteInfo("接口名：PhoneLogin；客户端IP:" + CommonFun.GetClientIP());

            string phoneNo = HttpContext.Current.Request.QueryString["phoneno"];
            string password = HttpContext.Current.Request.QueryString["password"];
            string vercode = HttpContext.Current.Request.QueryString["vercode"];
            string smscode = HttpContext.Current.Request.QueryString["smscode"];

            //return phoneExecutor.PhoneLogin(phoneNo, password, vercode, smscode);
            return null;
        }
        //public PhoneRes GetPhoneInfoForXml()
        //{
        //    return GetPhoneInfo();
        //}

        //public PhoneRes GetPhoneInfoForJson()
        //{
        //    return GetPhoneInfo();
        //}
        //public PhoneRes GetPhoneInfo()
        //{
        //    Log4netAdapter.WriteInfo("接口名：GetPhoneInfo；客户端IP:" + CommonFun.GetClientIP());

        //    string phoneNo = HttpContext.Current.Request.QueryString["phoneno"];

        //    return phoneExecutor.GetPhoneInfo(phoneNo);
        //}
        #endregion

        #region 公积金查询
        public BaseRes ProvidentFundInitForXml(string city)
        {
            return ProvidentFundInit(city);
        }

        public BaseRes ProvidentFundInitForJson(string city)
        {
            return ProvidentFundInit(city);
        }
        public BaseRes ProvidentFundInit(string city)
        {
            Log4netAdapter.WriteInfo("接口：ProvidentFundInit；客户端IP:" + CommonFun.GetClientIP());
            SocialSecurityOpr Opr = new SocialSecurityOpr();
            BaseRes Res = profundExecutor.Init(city);
            return Res;
        }

        public ProvidentFundQueryRes ProvidentFundQueryForXml(string city)
        {
            return ProvidentFundQuery(city);
        }

        public ProvidentFundQueryRes ProvidentFundQueryForJson(string city)
        {
            return ProvidentFundQuery(city);
        }
        public ProvidentFundQueryRes ProvidentFundQuery(string city)
        {
            Log4netAdapter.WriteInfo("接口名：ProvidentFundQuery；客户端IP:" + CommonFun.GetClientIP());
            ProvidentFundQueryRes QueryRes = null;
            try
            {
               
                string username = Chk.IsNull(HttpContext.Current.Request.QueryString["username"]);
                string password = Chk.IsNull(HttpContext.Current.Request.QueryString["password"]);
                string vercode = Chk.IsNull(HttpContext.Current.Request.QueryString["vercode"]);
                string token = Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);

                ProvidentFundReq funReq = new ProvidentFundReq();
                funReq.Username = username;
                funReq.Password = password;
                funReq.Vercode = vercode;
                funReq.Token = token;
               
                QueryRes = profundExecutor.GetProvidentFund(city, funReq);

                //计算最近24个月的缴费情况
                string PayTime = string.Empty;
                int PaymentMonths_Continuous = 0;
                for (int i = 0; i <= 23; i++)
                {
                    PayTime = DateTime.Now.AddMonths(-i).ToString("yyyyMM");

                    var query = QueryRes.ProvidentFundDetailList.Where(o => o.ProvidentFundTime == PayTime).FirstOrDefault();
                    if (query != null)
                    {
                        PaymentMonths_Continuous++;
                    }
                }
                QueryRes.PaymentMonths_Continuous = PaymentMonths_Continuous;
            }
            catch (Exception e)
            {
                QueryRes.StatusCode = ServiceConstants.StatusCode_error;
                QueryRes.StatusDescription = e.Message;
            }
            return QueryRes;
        }
        #endregion

        #region 央行互联网征信查询
        #region 初始化
        public BaseRes PbccrcReportInitForXml()
        {
            return PbccrcReportInit();
        }

        public BaseRes PbccrcReportInitForJson()
        {
            return PbccrcReportInit();
        }
        public BaseRes PbccrcReportInit()
        {

            return pbccrcExecutor.Init(); 
        }
        #endregion

        #region 登录
        public BaseRes PbccrcReportLoginForXml(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(false);
            LoginReq Req = reqText.DeserializeXML<LoginReq>();

            return PbccrcReportLogin(Req);
        }

        public BaseRes PbccrcReportLoginForJson(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(false);
            LoginReq Req = jsonService.DeserializeObject<LoginReq>(reqText);

            return PbccrcReportLogin(Req);
        }
        public BaseRes PbccrcReportLogin(LoginReq login)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportLogin；客户端IP:" + CommonFun.GetClientIP());
            string token = Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);
            return pbccrcExecutor.Login(token, login.Username, login.Password, login.Vercode);
        }
        #endregion
        public CRD_HD_REPORTRes GetPbccrcReportInfoForXml()
        {
            return GetPbccrcReportInfo();
        }

        public CRD_HD_REPORTRes GetPbccrcReportForJson()
        {
            return GetPbccrcReportInfo();
        }
        public CRD_HD_REPORTRes GetPbccrcReportInfo()
        {
            Log4netAdapter.WriteInfo("接口名：GetPbccrcReportInfo；客户端IP:" + CommonFun.GetClientIP());

            string username = HttpContext.Current.Request.QueryString["username"];
            string querycode = HttpContext.Current.Request.QueryString["querycode"];

            PbccrcReportQueryReq spiderReq = new PbccrcReportQueryReq();
            //spiderReq.Token = queryReq.Token;
            spiderReq.querycode = querycode;
            return pbccrcExecutor.GetReport(spiderReq);
        }

        #region 注册
        /// <summary>
        /// 第一步
        /// </summary>
        /// <param name="stream">参数</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep1ForXml(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
            return PbccrcReportRegisterStep1(RegisterReq);
        }
        /// <summary>
        /// 第一步
        /// </summary>
        /// <param name="stream">参数，例如：{'name':'{0}','certNo':'{1}','certType':'0','VerCode':'{2}'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep1ForJson(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
            return PbccrcReportRegisterStep1(RegisterReq);
        }
        public BaseRes PbccrcReportRegisterStep1(PbccrcReportRegisterReq RegisterReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep1；客户端IP:" + CommonFun.GetClientIP());
            string token =Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);
            return pbccrcExecutor.Register_Step1(token, RegisterReq.name, RegisterReq.certNo,RegisterReq.certType,RegisterReq.Vercode);
        }

        public BaseRes PbccrcReportRegisterStep2ForXml(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
            return PbccrcReportRegisterStep2(RegisterReq);
        }
        /// <summary>
        /// 第二步
        /// </summary>
        /// <param name="stream">参数，例如：{'mobileTel':'13524909205'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep2ForJson(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
            return PbccrcReportRegisterStep2(RegisterReq);
        }
        public BaseRes PbccrcReportRegisterStep2(PbccrcReportRegisterReq RegisterReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep2；客户端IP:" + CommonFun.GetClientIP());
            string token = Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);
            return pbccrcExecutor.Register_Step2(token, RegisterReq.mobileTel);
        }

        public BaseRes PbccrcReportRegisterStep3ForXml(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = reqText.DeserializeXML<PbccrcReportRegisterReq>();
            return PbccrcReportRegisterStep3(RegisterReq);
        }
        /// <summary>
        /// 第三步
        /// </summary>
        /// <param name="stream">参数，例如：{'loginname':'{0}','password':'{1}','confirmpassword':'{2}','mobileTel':'{3}','smscode':'{4}'}</param>
        /// <returns></returns>
        public BaseRes PbccrcReportRegisterStep3ForJson(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            PbccrcReportRegisterReq RegisterReq = jsonService.DeserializeObject<PbccrcReportRegisterReq>(reqText);
            return PbccrcReportRegisterStep3(RegisterReq);
        }
        public BaseRes PbccrcReportRegisterStep3(PbccrcReportRegisterReq RegisterReq)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportRegisterStep2；客户端IP:" + CommonFun.GetClientIP());
            string token = Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);
            return pbccrcExecutor.Register_Step3(token, RegisterReq.Username, RegisterReq.Password, RegisterReq.confirmpassword, RegisterReq.email, RegisterReq.mobileTel, RegisterReq.Smscode);
        }
        #endregion

        #region 查询申请
        public BaseRes PbccrcReportQueryApplicationStep1ForXml()
        {
            return PbccrcReportQueryApplicationStep1();
        }

        public BaseRes PbccrcReportQueryApplicationStep1ForJson()
        {
            return PbccrcReportQueryApplicationStep1();
        }
        public BaseRes PbccrcReportQueryApplicationStep1()
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep1；客户端IP:" + CommonFun.GetClientIP());
            string token = Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);
            return pbccrcExecutor.QueryApplication_Step1(token,false,null);
        }

        public BaseRes PbccrcReportQueryApplicationStep2ForXml(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            List<CRD_KbaQuestion> kbaList = reqText.DeserializeXML<List<CRD_KbaQuestion>>();
            return PbccrcReportQueryApplicationStep2(kbaList);
        }

        public BaseRes PbccrcReportQueryApplicationStep2ForJson(Stream stream)
        {
            //获得请求内容(请求内容为空)
            string reqText = stream.AsStringText(true);
            List<CRD_KbaQuestion> kbaList = jsonService.DeserializeObject<List<CRD_KbaQuestion>>(reqText);
            return PbccrcReportQueryApplicationStep2(kbaList);
        }
        public BaseRes PbccrcReportQueryApplicationStep2(List<CRD_KbaQuestion> kbaList)
        {
            Log4netAdapter.WriteInfo("接口名：PbccrcReportQueryApplicationStep2；客户端IP:" + CommonFun.GetClientIP());
            string token = Chk.IsNull(HttpContext.Current.Request.QueryString["token"]);
            return pbccrcExecutor.QueryApplication_Step2(token, kbaList);
        }
        #endregion
        #endregion

        #region 移动互联网认证
        public BaseRes VcreditCertifyForXml(string sort, string username, string password)
        {
            return VcreditCertify(sort, username, password);
        }

        public BaseRes VcreditCertifyForJson(string sort, string username, string password)
        {
            return VcreditCertify(sort, username, password);
        }
        public BaseRes VcreditCertify(string sort, string username, string password)
        {
            Log4netAdapter.WriteInfo("接口名：VcreditCertify；客户端IP:" + CommonFun.GetClientIP());

            string vercode = Chk.IsNull(HttpContext.Current.Request.QueryString["vercode"]);
            return vcertExecutor.GetVcreditCertify(sort, username, password, vercode);
        }
        #endregion

        #region 解析验证码

        public string GetVercodeByTesseract(Stream stream)
        {
            try
            {
                var bitmap = System.Drawing.Bitmap.FromStream(stream);
                string sort =Chk.IsNull(HttpContext.Current.Request.QueryString["sort"]);
                CharSort charSort = CharSort.All;
                if (!sort.IsEmpty())
                {
                    charSort = (CharSort)sort.ToInt(1001);
                }
                string vercode = secService.GetVerCodeByCharSort(Vcredit.Common.Helper.FileOperateHelper.ImageToBytes(bitmap), charSort);
                BaseRes Res = new BaseRes();
                return vercode;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        #endregion

        #region 移动账单查询
        //public VerCodeRes MobilInitForXml(string mobileNo)
        //{
        //    return MobilInit(mobileNo);
        //}

        //public VerCodeRes MobilInitForJson(string mobileNo)
        //{
        //    return MobilInit(mobileNo);
        //}
        //public VerCodeRes MobilInit(string mobileNo)
        //{
        //   return mobileExecutor.MobileInit(mobileNo);
        //}

        //public BaseRes MobileLoginForXml(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = reqText.DeserializeXML<MobileReq>();

        //    return MobileLogin(Req);
        //}

        //public BaseRes MobileLoginForJson(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

        //    return MobileLogin(Req);
        //}

        //public BaseRes MobileLogin(MobileReq mobileReq)
        //{
        //    Log4netAdapter.WriteInfo("接口名：PbccrcReportLogin；客户端IP:" + CommonFun.GetClientIP());
        //    Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
        //    BaseRes baseRes = new BaseRes();
        //    try
        //    {
        //        string token = mobileReq.Token;
        //        baseRes = mobileExecutor.MobileLogin(mobileReq);
        //    }
        //    catch (Exception e)
        //    {
        //        Log4netAdapter.WriteError("", e);
        //        baseRes.StatusDescription = e.Message;
        //    }
        //    return baseRes;
        //}

        //public BaseRes MobileSendSmsForXml(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = reqText.DeserializeXML<MobileReq>();

        //    return MobileSendSms(Req);
        //}

        //public BaseRes MobileSendSmsForJson(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

        //    return MobileSendSms(Req);
        //}
        //public VerCodeRes MobileSendSms(MobileReq mobileReq)
        //{
        //    Log4netAdapter.WriteInfo("接口名：MobileSendSms；客户端IP:" + CommonFun.GetClientIP());
        //    Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
        //    VerCodeRes baseRes = new VerCodeRes();
        //    try
        //    {
        //        string token = mobileReq.Token;
        //        baseRes = mobileExecutor.MobileSendSms(mobileReq);
        //    }
        //    catch (Exception e)
        //    {
        //        Log4netAdapter.WriteError("", e);
        //        baseRes.StatusDescription = e.Message;
        //    }
        //    return baseRes;
        //}

        //public BaseRes MobileCheckSmsForXml(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = reqText.DeserializeXML<MobileReq>();

        //    return MobileCheckSms(Req);
        //}

        //public BaseRes MobileCheckSmsForJson(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

        //    return MobileCheckSms(Req);
        //}
        //public BaseRes MobileCheckSms(MobileReq mobileReq)
        //{
        //    Log4netAdapter.WriteInfo("接口名：PbccrcReportLogin；客户端IP:" + CommonFun.GetClientIP());
        //    Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
        //    BaseRes baseRes = new BaseRes();
        //    try
        //    {
        //        string token = mobileReq.Token;
        //        baseRes = mobileExecutor.MobileCheckSms(mobileReq);
        //    }
        //    catch (Exception e)
        //    {
        //        Log4netAdapter.WriteError("", e);
        //        baseRes.StatusDescription = e.Message;
        //    }
        //    return baseRes;
        //}

        //public BaseRes MobileQueryForXml(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = reqText.DeserializeXML<MobileReq>();

        //    return MobileQuery(Req);
        //}

        //public BaseRes MobileQueryForJson(Stream stream)
        //{
        //    //获得请求内容
        //    string reqText = stream.AsStringText(true);
        //    MobileReq Req = jsonService.DeserializeObject<MobileReq>(reqText);

        //    return MobileQuery(Req);
        //}
        //public BaseRes MobileQuery(MobileReq mobileReq)
        //{
        //    Log4netAdapter.WriteInfo("接口名：MobileQuery；客户端IP:" + CommonFun.GetClientIP());
        //    Log4netAdapter.WriteInfo("参数：" + jsonService.SerializeObject(mobileReq, true));
        //    BaseRes Res = new BaseRes();
        //    try
        //    {
        //        string token = mobileReq.Token;
        //        Res.Result = mobileExecutor.MobileQuery(mobileReq).ToXml();
        //    }
        //    catch (Exception e)
        //    {
        //        Log4netAdapter.WriteError("", e);
        //        Res.StatusDescription = e.Message;
        //    }
        //    return Res;
        //}
        #endregion


       
    }
}