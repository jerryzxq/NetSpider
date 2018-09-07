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
using Vcredit.NetSpider.Parser;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.RestService.Contracts;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.RestService.Operation;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.RestService.Models.RC;
using Vcredit.NetSpider.RestService.Models.ProvidentFund;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class ProvidentFundService : IProvidentFundService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();
        IMobileExecutor mobileExecutor = ExecutorManager.GetMobileExecutor();
        ITaobaoExecutor taobaoExecutor = ExecutorManager.GetTaobaoExecutor();
        IExecutor Executor = ExecutorManager.GetExecutor();
        IVcreditCertifyExecutor vcertExecutor = ExecutorManager.GetVcreditCertifyExecutor();
        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();

        IVerCodeParserService vercodeService = ParserServiceManager.GetVerCodeParserService();
        CookieCollection cookies = new CookieCollection();
        bool isbase64 = true;
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public ProvidentFundService()
        {
            //string token =Chk.IsNull(HttpContext.Current.Request.QueryString["toke"]);
        }

        #region 公积金初始化
        public VerCodeRes ProvidentFundInitForXml(string city)
        {
            return ProvidentFundInit(city);
        }

        public VerCodeRes ProvidentFundInitForJson(string city)
        {
            return ProvidentFundInit(city);
        }
        public VerCodeRes ProvidentFundInit(string city)
        {
            Log4netAdapter.WriteInfo("接口：ProvidentFundInit；客户端IP:" + CommonFun.GetClientIP());
            SocialSecurityOpr Opr = new SocialSecurityOpr();
            VerCodeRes Res = profundExecutor.Init(city);
            return Res;
        }
        #endregion

        #region 公积金登录
        public ProvidentFundQueryRes ProvidentFundLoginForXml(Stream stream, string city)
        {
            ProvidentFundQueryRes baseRes = new ProvidentFundQueryRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                ProvidentFundReq Req = reqText.DeserializeXML<ProvidentFundReq>();
                baseRes = ProvidentFundLogin(city, Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public ProvidentFundQueryRes ProvidentFundLoginForJson(Stream stream, string city)
        {
            ProvidentFundQueryRes baseRes = new ProvidentFundQueryRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                ProvidentFundReq Req = jsonService.DeserializeObject<ProvidentFundReq>(reqText);
                baseRes = ProvidentFundLogin(city, Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;

        }
        public ProvidentFundQueryRes ProvidentFundLogin(string city, ProvidentFundReq funReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口名：ProvidentFundLogin；客户端IP:{0},城市:{1},参数：{2}", ClientIp, city, jsonService.SerializeObject(funReq, true)));
            ProvidentFundQueryRes queryRes = null;
            try
            {
                if (funReq.Identitycard.IsEmpty())
                {
                    queryRes = new ProvidentFundQueryRes();
                    queryRes.StatusCode = ServiceConstants.StatusCode_fail;
                    queryRes.StatusDescription = ServiceConsts.Required_IdentitycardEmpty;
                    return queryRes;
                }
                queryRes = profundExecutor.GetProvidentFund(city, funReq);

                #region 保存数据
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity applyEntity = new Spd_applyEntity();
                applyEntity.Identitycard = funReq.Identitycard;
                applyEntity.IPAddr = ClientIp;
                applyEntity.Mobile = funReq.Mobile;
                applyEntity.Name = funReq.Name;
                applyEntity.AppId = funReq.BusType;
                applyEntity.Website = city;
                applyEntity.Spider_type = "housingfund";
                applyEntity.Apply_status = queryRes.StatusCode;
                applyEntity.Description = queryRes.StatusDescription;
                applyEntity.Token = funReq.Token;

                Spd_applyformEntity formEntity = new Spd_applyformEntity();
                if (!funReq.Username.IsEmpty())
                {
                    formEntity.Form_name = "Username";
                    formEntity.Form_value = funReq.Username;
                    applyEntity.Spd_applyformList.Add(formEntity);
                }
                if (!funReq.Password.IsEmpty())
                {
                    formEntity.Form_name = "Password";
                    formEntity.Form_value = funReq.Password;
                    applyEntity.Spd_applyformList.Add(formEntity);
                }
                if (!funReq.FundAccount.IsEmpty())
                {
                    formEntity.Form_name = "FundAccount";
                    formEntity.Form_value = funReq.FundAccount;
                    applyEntity.Spd_applyformList.Add(formEntity);
                }
                applyService.Save(applyEntity);

                if (queryRes.StatusCode == ServiceConstants.StatusCode_success)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            IProvidentFund service = NetSpiderFactoryManager.GetProvidentFundService();
                            ProvidentFundEntity entity = jsonService.DeserializeObject<ProvidentFundEntity>(jsonService.SerializeObject(queryRes));
                            ProvidentFundReserveEntity entity_reserve = jsonService.DeserializeObject<ProvidentFundReserveEntity>(jsonService.SerializeObject(queryRes.ProvidentFundReserveRes));
                            ProvidentFundLoanEntity entity_loan = jsonService.DeserializeObject<ProvidentFundLoanEntity>(jsonService.SerializeObject(queryRes.ProvidentFundLoanRes));
                            entity.Loginname = funReq.Username;
                            entity.Password = funReq.Password;
                            entity.BusIdentityCard = funReq.Identitycard;
                            entity.BusName = funReq.Name;
                            entity.BusType = funReq.BusType;
                            entity.ProvidentFundReserveRes = entity_reserve;
                            entity.ProvidentFundLoanRes = entity_loan;
                            service.Save(entity);
                        }
                        catch (Exception e)
                        {
                            Log4netAdapter.WriteError("身份证号:" + funReq.Identitycard + ",保存公积金数据异常", e);
                        }
                    });
                }
                #endregion
            }
            catch (Exception e)
            {
                queryRes.StatusCode = ServiceConstants.StatusCode_error;
                queryRes.StatusDescription = e.Message;
            }
            return queryRes;
        }
        #endregion

        #region 公积金城市对应表单

        /// <summary>
        /// 获取全国城市
        /// </summary>
        /// <returns></returns>
        public BaseRes QueryProvinceForJson()
        {
            string guid = CommonFun.GetGuidID();

            BaseRes Res = new BaseRes();
            try
            {
                Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFormsetting；客户端IP:" + CommonFun.GetClientIP() + "," + guid);

                ProvinceMongo mongo = new ProvinceMongo();
                List<Entity.Mongo.ProvidentFund.Province> proList = mongo.LoadAll();
                List<Entity.Mongo.ProvidentFund.Province> proListTemp = new List<Entity.Mongo.ProvidentFund.Province>();

                for (int i = 0; i < proList.Count; i++)
                {
                    proList[i].CityLevel = proList[i].CityLevel.Where(o => o.WhetherOnline == "1").ToList();
                    if (proList[i].CityLevel.Count == 0)
                    {
                        proListTemp.Add(proList[i]);
                    }
                }
                foreach (var item in proListTemp)
                {
                    proList.Remove(item);
                }

                if (proList != null)
                {
                    Res.Result = jsonService.SerializeObject(proList,true);
                    Res.StatusCode = ServiceConstants.StatusCode_success;
                }
                else
                {
                    Res.StatusCode = ServiceConstants.StatusCode_fail;
                    Res.StatusDescription = "暂不支持此城市公积金查询";
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConstants.StatusCode_error;
                Res.StatusDescription = e.Message;
                Log4netAdapter.WriteError(guid + "获取城市列表异常", e);
            }
            return Res;
        }

        public BaseRes ProvidentFundFormsettingQueryForXml(string city)
        {
            return ProvidentFundFormsettingQuery(city);
        }

        public BaseRes ProvidentFundFormsettingQueryForJson(string city)
        {
            return ProvidentFundFormsettingQuery(city);
        }

        public BaseRes ProvidentFundFormsettingQuery(string city)
        {
            string guid = CommonFun.GetGuidID();

            BaseRes Res = new BaseRes();
            try
            {
                //Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFormsetting；客户端IP:" + CommonFun.GetClientIP() + "；城市:" + city + ";" + guid);

                ProvidentFundMongo mongo = new ProvidentFundMongo();
                Entity.Mongo.ProvidentFund.City cityEntity = mongo.GetCityFormSetting(city);

                if (cityEntity != null)
                {
                    Res.Result = jsonService.SerializeObject(cityEntity);
                    Res.StatusCode = ServiceConstants.StatusCode_success;
                }
                else
                {
                    Res.StatusCode = ServiceConstants.StatusCode_fail;
                    Res.StatusDescription = "暂不支持此城市公积金查询";
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConstants.StatusCode_error;
                Res.StatusDescription = e.Message;
                Log4netAdapter.WriteError(guid + "获取城市公积金表单设置异常", e);
            }
            return Res;
        }
        #endregion

        #region 表单设置
        public BaseRes ProvidentFundFormsettingSave(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：ProvidentFundFormsettingSave；客户端IP:" + ClientIp);
            BaseRes baseRes = new BaseRes();
            try
            {
                Entity.Mongo.ProvidentFund.City cityEntity = jsonService.DeserializeObject<Entity.Mongo.ProvidentFund.City>(reqText);

                ProvidentFundMongo mongo = new ProvidentFundMongo();
                mongo.SaveCityFormSetting(cityEntity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "公积金表单信息保存完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "公积金表单信息保存出错";
                Log4netAdapter.WriteError("公积金表单信息保存出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        public BaseRes ProvidentFundFormsettingUpdate(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：ProvidentFundFormsettingUpdate；客户端IP:" + ClientIp + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                Entity.Mongo.ProvidentFund.City cityEntity = jsonService.DeserializeObject<Entity.Mongo.ProvidentFund.City>(reqText);

                ProvidentFundMongo mongo = new ProvidentFundMongo();
                mongo.UpdateCityFormSetting(cityEntity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "公积金表单信息更新完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "公积金表单信息更新出错";
                Log4netAdapter.WriteError("公积金表单信息更新出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        #endregion

        #region 查询公积金数据
        public BaseRes GetProvidentFundInfo(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：GetProvidentFundInfo；客户端IP:" + ClientIp + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                query fundReq = jsonService.DeserializeObject<query>(reqText);
                if (fundReq.Identitycard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_error;
                    baseRes.StatusDescription = "公积金数据查询出错,请输入参数身份证";
                    return baseRes;
                }
                IProvidentFund fundService = NetSpiderFactoryManager.GetProvidentFundService();
                var fundEntity = fundService.GetByIdentityCard(fundReq.Identitycard.ToTrim(), fundReq.BusType, fundReq.citycode);
                if (fundEntity != null)
                {
                    fundEntity.Loginname = null;
                    fundEntity.Password = null;
                    //fundEntity.Payment_State = null;
                }
                baseRes.Result = jsonService.SerializeObject(fundEntity, true);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "公积金数据查询完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "公积金数据查询出错";
                Log4netAdapter.WriteError("公积金数据查询出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        private class query
        {
            public string Identitycard { get; set; }
            public string citycode { get; set; }
            public string BusType { get; set; }
        }

        public BaseRes GetProvidentFundAll(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：GetProvidentFundAll；客户端IP:" + ClientIp + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                query fundReq = jsonService.DeserializeObject<query>(reqText);
                if (fundReq.Identitycard.IsEmpty()){
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "公积金数据查询出错,请输入参数身份证";
                    return baseRes;
                }
                IProvidentFund fundService = NetSpiderFactoryManager.GetProvidentFundService();
                var fundEntity = fundService.GetAllDataByIdentityCard(fundReq.Identitycard, fundReq.BusType, fundReq.citycode);
                if (fundEntity != null)
                {
                    fundEntity.Loginname = null;
                    fundEntity.Password = null;
                    //fundEntity.Payment_State = null;

                    IProvidentFundLoan fundloanService = NetSpiderFactoryManager.GetProvidentFundLoanService();
                    var fundloanEntity = fundloanService.GetByProvidentFundId(fundEntity.Id);
                    if (fundloanEntity != null)
                    {
                        IProvidentFundLoanDetail fundloandetailService = NetSpiderFactoryManager.GetProvidentFundLoanDetailService();
                        var fundloandetailEntity = fundloandetailService.GetDetailListByProvidentFundId(fundEntity.Id);
                        fundloanEntity.ProvidentFundLoanDetailList = fundloandetailEntity;
                    }
                    fundEntity.ProvidentFundLoanRes = fundloanEntity;

                    IProvidentFundReserve fundreserveService = NetSpiderFactoryManager.GetProvidentFundReserveService();
                    var fundreserveEntity = fundreserveService.GetByProvidentFundId(fundEntity.Id);
                    if (fundreserveEntity != null)
                    {
                        IProvidentFundReserveDetail fundreservedetailService = NetSpiderFactoryManager.GetProvidentFundReserveDetailService();
                        var fundreservedetailEntity = fundreservedetailService.GetDetailListByProvidentFundId(fundEntity.Id);
                        fundreserveEntity.ProvidentReserveFundDetailList = fundreservedetailEntity;
                    }
                    fundEntity.ProvidentFundReserveRes = fundreserveEntity;
                }
                baseRes.Result = jsonService.SerializeObject(fundEntity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "公积金数据查询完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "公积金数据查询出错";
                Log4netAdapter.WriteError("公积金数据查询出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        #endregion

        #region 私有方法

        #endregion

        #region 评分计算
        public BaseRes ScoreCalculateForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            ScoreCalculateReq queryReq = reqText.DeserializeXML<ScoreCalculateReq>();
            return ScoreCalculate(queryReq);
        }

        public BaseRes ScoreCalculateForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            ScoreCalculateReq queryReq = jsonService.DeserializeObject<ScoreCalculateReq>(reqText);
            return ScoreCalculate(queryReq);
        }
        public BaseRes ScoreCalculate(ScoreCalculateReq queryReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口：ScoreCalculate；客户端IP:{0},参数：{1}", ClientIp, jsonService.SerializeObject(queryReq.IdentityCard, true)));

            BaseRes baseRes = new BaseRes();
            DecisionResult decisionResult = new DecisionResult();
            RCWebService.RCWebService rcServcie = new RCWebService.RCWebService();
            List<VBXML> xmllist = new List<VBXML>();
            int score = 0;
            try
            {
                IProvidentFund service = NetSpiderFactoryManager.GetProvidentFundService();
                ProvidentFundEntity entity = service.GetByIdentityCard(queryReq.IdentityCard);

                #region 计算征信评分
                if (entity != null)
                {
                    xmllist.Add(new VBXML { VB_COL = "Customer_GENDER", VB_VALUE = ChinaIDCard.GetSex(queryReq.IdentityCard) });//[网络版征信]性别
                    xmllist.Add(new VBXML { VB_COL = "M24_ContinuePay_SumMonth", VB_VALUE = entity.PaymentMonths_Continuous.ToString() });//[公积金社保]24个月内最近连续缴费月数
                    xmllist.Add(new VBXML { VB_COL = "ACTUAL_PAY_SUMMONTH", VB_VALUE = entity.PaymentMonths.ToString() });//[公积金社保]实际缴保月数
                    xmllist.Add(new VBXML { VB_COL = "LASTYEAR_AVG_SOCIALSECURITYBASE", VB_VALUE = entity.SalaryBase.ToString() });//[公积金社保]个人上一年平均缴费基数
                    xmllist.Add(new VBXML { VB_COL = "SOCIALSECURITY_AGE", VB_VALUE = ChinaIDCard.GetAge(queryReq.IdentityCard).ToString() });//[卡卡贷]年龄
                    xmllist.Add(new VBXML { VB_COL = "Customer_HOUSEHOLD", VB_VALUE = queryReq.IsLocal });//[卡卡贷]是否本地籍

                    Log4netAdapter.WriteInfo(jsonService.SerializeObject(xmllist));
                    string rc = rcServcie.GetRuleResultByCustom(ServiceConstants.RC_Model_Shebao, SerializationHelper.SerializeToXml(xmllist));
                    decisionResult = rc.DeserializeXML<List<DecisionResult>>().FirstOrDefault();


                    if (decisionResult != null)
                    {
                        Log4netAdapter.WriteInfo(jsonService.SerializeObject(decisionResult));
                        if (decisionResult.RuleResultCanShowSets.ContainsKey("*网络版社保评分"))
                        {
                            score = decisionResult.RuleResultCanShowSets["*网络版社保评分"].ToInt(0);

                            //更新公积金表
                            entity.Score = score;
                            service.Update(entity);
                        }
                    }

                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "决策计算完毕";
                    baseRes.Result = score.ToString();
                }
                else
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "无征信";
                }
                #endregion

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(queryReq.IdentityCard + ",决策异常；", e);
                baseRes.StatusDescription = e.Message;
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
            }
            baseRes.EndTime = DateTime.Now.ToString();

            Log4netAdapter.WriteInfo(queryReq.IdentityCard + "接口：ScoreCalculate,调用结束");
            return baseRes;
        }
        #endregion


    }
}