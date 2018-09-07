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
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Mongo.ProvidentFund;
using Vcredit.NetSpider.Entity.Mongo.SocialSecurity;
using Vcredit.NetSpider.RestService.Models.ProvidentFund;
using Vcredit.NetSpider.RestService.Models.RC;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class SocialSecurityService : ISocialSecurityService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();
        IMobileExecutor mobileExecutor = ExecutorManager.GetMobileExecutor();
        ITaobaoExecutor taobaoExecutor = ExecutorManager.GetTaobaoExecutor();
        IExecutor Executor = ExecutorManager.GetExecutor();
        IVcreditCertifyExecutor vcertExecutor = ExecutorManager.GetVcreditCertifyExecutor();
        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();

        IRuntimeService runService = ProcessEngine.GetRuntimeService();
        IVerCodeParserService vercodeService = ParserServiceManager.GetVerCodeParserService();
        CookieCollection cookies = new CookieCollection();
        bool isbase64 = true;
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public SocialSecurityService()
        {
            //string token =Chk.IsNull(HttpContext.Current.Request.QueryString["toke"]);
        }

        #region 社保查询初始化
        /// <summary>
        /// 社保登录初始化（XML）
        /// </summary>
        /// <param name="city">社保城市</param>
        /// <returns></returns>
        public VerCodeRes SocialSecurityInitForXml(string city)
        {
            return SocialSecurityInit(city);
        }
        /// <summary>
        /// 社保登录初始化（JSON）
        /// </summary>
        /// <param name="city">社保城市</param>
        /// <returns></returns>
        public VerCodeRes SocialSecurityInitForJson(string city)
        {
            return SocialSecurityInit(city);
        }
        /// <summary>
        /// 社保登录初始化
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public VerCodeRes SocialSecurityInit(string city)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：SocialSecurityInit；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + "城市:" + city);
            VerCodeRes Res = new VerCodeRes();
            try
            {
                Res = socialsecExecutor.Init(city);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConstants.StatusCode_error;
                Res.StatusDescription = e.Message;
                Log4netAdapter.WriteError(guid + "社保初始化接口异常", e);
            }
            return Res;
        }
        #endregion

        #region  社保登录
        public SocialSecurityQueryRes SocialSecurityLoginForXml(Stream stream, string city)
        {
            SocialSecurityQueryRes baseRes = new SocialSecurityQueryRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                SocialSecurityReq Req = reqText.DeserializeXML<SocialSecurityReq>();
                baseRes = SocialSecurityLogin(city, Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public SocialSecurityQueryRes SocialSecurityLoginForJson(Stream stream, string city)
        {
            SocialSecurityQueryRes baseRes = new SocialSecurityQueryRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                SocialSecurityReq Req = jsonService.DeserializeObject<SocialSecurityReq>(reqText);
                baseRes = SocialSecurityLogin(city, Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;

        }
        public SocialSecurityQueryRes SocialSecurityLogin(string city, SocialSecurityReq socialReq)
        {
            Log4netAdapter.WriteInfo(string.Format("接口名：SocialSecurityLogin；客户端IP:{0},城市:{1},参数：{2}", ClientIp, city, jsonService.SerializeObject(socialReq, true)));

            SocialSecurityQueryRes queryRes = null;
            try
            {
                if (socialReq.Identitycard.IsEmpty())
                {
                    queryRes = new SocialSecurityQueryRes();
                    queryRes.StatusCode = ServiceConstants.StatusCode_fail;
                    queryRes.StatusDescription = ServiceConsts.Required_IdentitycardEmpty;
                    return queryRes;
                }

                queryRes = socialsecExecutor.GetSocialSecurity(city, socialReq);

                #region 保存数据
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity applyEntity = new Spd_applyEntity();
                applyEntity.Identitycard = socialReq.Identitycard;
                applyEntity.IPAddr = ClientIp;
                applyEntity.Mobile = socialReq.Mobile;
                applyEntity.Name = socialReq.Name;
                applyEntity.AppId = socialReq.BusType;
                applyEntity.Website = city;
                applyEntity.Spider_type = "shebao";
                applyEntity.Apply_status = queryRes.StatusCode;
                applyEntity.Description = queryRes.StatusDescription;
                applyEntity.Token = socialReq.Token;

                Spd_applyformEntity formEntity = new Spd_applyformEntity();
                if (!socialReq.Username.IsEmpty())
                {
                    formEntity.Form_name = "Username";
                    formEntity.Form_value = socialReq.Username;
                    applyEntity.Spd_applyformList.Add(formEntity);
                }
                if (!socialReq.Password.IsEmpty())
                {
                    formEntity.Form_name = "Password";
                    formEntity.Form_value = socialReq.Password;
                    applyEntity.Spd_applyformList.Add(formEntity);
                }
                if (!socialReq.Citizencard.IsEmpty())
                {
                    formEntity.Form_name = "Citizencard";
                    formEntity.Form_value = socialReq.Citizencard;
                    applyEntity.Spd_applyformList.Add(formEntity);
                }
                applyService.Save(applyEntity);

                if (queryRes.StatusCode == ServiceConstants.StatusCode_success)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ISocialSecurity service = NetSpiderFactoryManager.GetSocialSecurityService();
                            SocialSecurityEntity entity = jsonService.DeserializeObject<SocialSecurityEntity>(jsonService.SerializeObject(queryRes));
                            entity.Loginname = socialReq.Username;
                            entity.Password = socialReq.Password;
                            entity.BusIdentityCard = socialReq.Identitycard;
                            entity.BusName = socialReq.Name;
                            entity.BusType = socialReq.BusType;
                            service.Save(entity);
                        }
                        catch (Exception e)
                        {
                            Log4netAdapter.WriteError("身份证号:" + socialReq.Identitycard + ",保存社保数据异常", e);
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

        #region 城市对应表单
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
                Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFormsetting；客户端IP:" + CommonFun.GetClientIP() +","+ guid);

                SheBaoProvinceMongo mongo = new SheBaoProvinceMongo();
                List<Entity.Mongo.SocialSecurity.SheBaoProvince> proList = mongo.LoadAll();
                List<Entity.Mongo.SocialSecurity.SheBaoProvince> proListTemp = new List<Entity.Mongo.SocialSecurity.SheBaoProvince>();

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
                    Res.StatusDescription = "暂不支持此城市社保查询";
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

        public BaseRes FormsettingQueryForXml(string city)
        {
            return FormsettingQuery(city);
        }

        public BaseRes FormsettingQueryForJson(string city)
        {
            return FormsettingQuery(city);
        }

        public BaseRes FormsettingQuery(string city)
        {
            string guid = CommonFun.GetGuidID();

            BaseRes Res = new BaseRes();
            try
            {
                //Log4netAdapter.WriteInfo("接口：ProvidentFundQueryFormsetting；客户端IP:" + CommonFun.GetClientIP() + "；城市:" + city + ";" + guid);

                SheBaoProvidentFundMongo mongo = new SheBaoProvidentFundMongo();
                Entity.Mongo.ProvidentFund.City cityEntity= mongo.GetCityFormSetting(city);

                if (cityEntity != null)
                {
                    Res.Result = jsonService.SerializeObject(cityEntity);
                    Res.StatusCode = ServiceConstants.StatusCode_success;
                }
                else
                {
                    Res.StatusCode = ServiceConstants.StatusCode_fail;
                    Res.StatusDescription = "暂不支持此城市社保查询";
                }
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConstants.StatusCode_error;
                Res.StatusDescription = e.Message;
                Log4netAdapter.WriteError(guid + "获取城市社保表单设置异常", e);
            }
            return Res;
        }
        #endregion

        #region 表单设置
        public BaseRes FormsettingSave(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：SocialSecurityFormsettingSave；客户端IP:" + ClientIp);
            BaseRes baseRes = new BaseRes();
            try
            {
                Entity.Mongo.ProvidentFund.City cityEntity = jsonService.DeserializeObject<Entity.Mongo.ProvidentFund.City>(reqText);

                SheBaoProvidentFundMongo mongo = new SheBaoProvidentFundMongo();
                mongo.SaveCityFormSetting(cityEntity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "社保表单信息保存完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "社保表单信息保存出错";
                Log4netAdapter.WriteError("社保表单信息保存出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        public BaseRes FormsettingUpdate(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：SocialSecurityFormsettingUpdate；客户端IP:" + ClientIp + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                Entity.Mongo.ProvidentFund.City cityEntity = jsonService.DeserializeObject<Entity.Mongo.ProvidentFund.City>(reqText);

                SheBaoProvidentFundMongo mongo = new SheBaoProvidentFundMongo();
                mongo.UpdateCityFormSetting(cityEntity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "社保表单信息更新完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "社保表单信息更新出错";
                Log4netAdapter.WriteError("社保表单信息更新出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
        #endregion

        #region 查询公积金数据
        public BaseRes GetSocialSecurityInfo(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：GetSocialSecurityInfo；客户端IP:" + ClientIp + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                var socailReq = jsonService.DeserializeObject<query>(reqText);
                if (socailReq.Identitycard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_error;
                    baseRes.StatusDescription = "社保数据查询出错,请输入参数身份证";
                    return baseRes;
                }
                ISocialSecurity socialService = NetSpiderFactoryManager.GetSocialSecurityService();
                var socialEntity = socialService.GetByIdentityCard(socailReq.Identitycard, socailReq.BusType, socailReq.citycode);
                if (socialEntity != null)
                {
                    socialEntity.Loginname = null;
                    socialEntity.Password = null;
                    //socialEntity.Payment_State = null;
                }
          
                baseRes.Result = jsonService.SerializeObject(socialEntity,true);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "社保数据查询完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "社保数据查询出错";
                Log4netAdapter.WriteError("社保数据查询出错", e);
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


        public BaseRes GetSocialSecurityAll(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(isbase64);
            Log4netAdapter.WriteInfo("接口：GetSocialSecurityAll；客户端IP:" + ClientIp + ";参数：" + reqText);
            BaseRes baseRes = new BaseRes();
            try
            {
                query socailReq = jsonService.DeserializeObject<query>(reqText);
                if (socailReq.Identitycard.IsEmpty())
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "社保数据查询出错,请输入参数身份证";
                    return baseRes;
                }
                ISocialSecurity socialService = NetSpiderFactoryManager.GetSocialSecurityService();
                var socialEntity = socialService.GetAllDataByIdentityCard(socailReq.Identitycard, socailReq.BusType, socailReq.citycode);
                if (socialEntity != null)
                {
                    socialEntity.Loginname = null;
                    socialEntity.Password = null;
                    //socialEntity.Payment_State = null;
                }

                baseRes.Result = jsonService.SerializeObject(socialEntity);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "社保数据查询完毕";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "社保数据查询出错";
                Log4netAdapter.WriteError("社保数据查询出错", e);
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }
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
                ISocialSecurity service = NetSpiderFactoryManager.GetSocialSecurityService();
                SocialSecurityEntity entity = service.GetByIdentityCard(queryReq.IdentityCard);

                #region 计算征信评分
                if (entity != null)
                {
                    xmllist.Add(new VBXML { VB_COL = "Customer_GENDER", VB_VALUE = ChinaIDCard.GetSex(queryReq.IdentityCard) });//[网络版征信]性别
                    xmllist.Add(new VBXML { VB_COL = "M24_ContinuePay_SumMonth", VB_VALUE = entity.PaymentMonths_Continuous.ToString() });//[公积金社保]24个月内最近连续缴费月数
                    xmllist.Add(new VBXML { VB_COL = "ACTUAL_PAY_SUMMONTH", VB_VALUE = entity.PaymentMonths.ToString() });//[公积金社保]实际缴保月数
                    xmllist.Add(new VBXML { VB_COL = "LASTYEAR_AVG_SOCIALSECURITYBASE", VB_VALUE = entity.SocialInsuranceBase.ToString() });//[公积金社保]个人上一年平均缴费基数
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

                            //更新社保表
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