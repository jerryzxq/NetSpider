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
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.NetSpider.Entity;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class EduService : IEduService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口
        IChsiExecutor chsiExecutor = ExecutorManager.GetChsiExecutor();

        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ClientIp = CommonFun.GetClientIP();
        #endregion

        public EduService()
        {
        }
        #region 学信网接口

        #region 学信网注册
        public VerCodeRes ChsiRegisterStep1ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiRegisterReq Req = reqText.DeserializeXML<ChsiRegisterReq>();
            return ChsiRegisterStep1(Req);
        }

        public VerCodeRes ChsiRegisterStep1ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiRegisterReq Req = jsonService.DeserializeObject<ChsiRegisterReq>(reqText);
            return ChsiRegisterStep1(Req);
        }

        public VerCodeRes ChsiRegisterStep1(ChsiRegisterReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiRegisterStep1；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            VerCodeRes Res = new VerCodeRes();
            try
            {
                Res = chsiExecutor.Register_Init(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }

            return Res;
        }

        public BaseRes ChsiRegisterStep2ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiRegisterReq Req = reqText.DeserializeXML<ChsiRegisterReq>();
            return ChsiRegisterStep2(Req);
        }

        public BaseRes ChsiRegisterStep2ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiRegisterReq Req = jsonService.DeserializeObject<ChsiRegisterReq>(reqText);
            return ChsiRegisterStep2(Req);
        }
        public BaseRes ChsiRegisterStep2(ChsiRegisterReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiRegisterStep2；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = chsiExecutor.Register_Step1(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }

            return Res;
        }


        public BaseRes ChsiRegisterStep3ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiRegisterReq Req = reqText.DeserializeXML<ChsiRegisterReq>();
            return ChsiRegisterStep3(Req);
        }

        public BaseRes ChsiRegisterStep3ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiRegisterReq Req = jsonService.DeserializeObject<ChsiRegisterReq>(reqText);
            return ChsiRegisterStep3(Req);
        }
        public BaseRes ChsiRegisterStep3(ChsiRegisterReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiRegisterStep3；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = chsiExecutor.Register_Step2(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }

            return Res;
        }
        #endregion

        #region 学信网查询
        public VerCodeRes ChsiQueryInitForXml()
        {
            return ChsiQueryInit();
        }
        public VerCodeRes ChsiQueryInitForJson()
        {
            return ChsiQueryInit();
        }
        public VerCodeRes ChsiQueryInit()
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiQueryInit；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            VerCodeRes Res = new VerCodeRes();
            try
            {
                Res = chsiExecutor.Query_Init();
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }

            return Res;
        }

        public BaseRes ChsiQueryForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            LoginReq Req = reqText.DeserializeXML<LoginReq>();
            return ChsiQuery(Req);
        }

        public BaseRes ChsiQueryForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            LoginReq Req = jsonService.DeserializeObject<LoginReq>(reqText);
            return ChsiQuery(Req);
        }
        public BaseRes ChsiQuery(LoginReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiQuery；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = chsiExecutor.Query_GetInfo(req);

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //ISpd_Login spdService = NetSpiderFactoryManager.GetSpd_LoginService();
                        //Spd_LoginEntity loginEntity = new Spd_LoginEntity();
                        //loginEntity.Id = req.Token;
                        //loginEntity.IdentityCard = req.Identitycard;
                        //loginEntity.IPAddr = ClientIp;
                        //loginEntity.LoginParam1 = req.Username;
                        //loginEntity.LoginParam2 = req.Password;
                        //loginEntity.Mobile = req.Mobile;
                        //loginEntity.Name = req.Name;
                        //loginEntity.SpiderType = "chsiquery";
                        //loginEntity.StatusCode = Res.StatusCode;
                        //loginEntity.StatusDesc = Res.StatusDescription;
                        //spdService.Save(loginEntity);

                        ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                        Spd_applyEntity applyEntity = new Spd_applyEntity();
                        applyEntity.Identitycard = req.Identitycard;
                        applyEntity.IPAddr = ClientIp;
                        applyEntity.Mobile = req.Mobile;
                        applyEntity.Name = req.Name;
                        applyEntity.Website = ServiceConsts.WebSite_XueXin;
                        applyEntity.Spider_type = ServiceConsts.SpiderType_Chsiquery;
                        applyEntity.Apply_status = Res.StatusCode;
                        applyEntity.Description = Res.StatusDescription;
                        applyEntity.Token = req.Token;

                        Spd_applyformEntity formEntity = new Spd_applyformEntity();
                        formEntity.Form_name = "Username";
                        formEntity.Form_value = req.Username;
                        applyEntity.Spd_applyformList.Add(formEntity);
                        formEntity = new Spd_applyformEntity();
                        formEntity.Form_name = "Password";
                        formEntity.Form_value = req.Password;
                        applyEntity.Spd_applyformList.Add(formEntity);
                        applyService.Save(applyEntity);

                        if (Res.StatusCode == ServiceConsts.StatusCode_success)
                        {
                            IChsi_Info service = NetSpiderFactoryManager.GetChsi_InfoService();

                            List<Chsi_InfoEntity> entitys = jsonService.DeserializeObject<List<Chsi_InfoEntity>>(Res.Result);
                            foreach (var item in entitys)
                            {
                                item.Token = req.Token;
                            }
                            service.SaveAll(entitys);
                        }
                    }
                    catch (Exception e)
                    {
                        Log4netAdapter.WriteError(guid + ",保存学信网信息出错",e);
                    }
                });
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo(guid + ",接口调用结束");
            return Res;
        }
        #endregion

        #region 学信网忘记用户名
        public BaseRes ChsiForgetUsernameForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = reqText.DeserializeXML<ChsiForgetReq>();
            return ChsiForgetUsername(Req);
        }

        public BaseRes ChsiForgetUsernameForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = jsonService.DeserializeObject<ChsiForgetReq>(reqText);
            return ChsiForgetUsername(Req);
        }
        public BaseRes ChsiForgetUsername(ChsiForgetReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiForgetUsername；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = chsiExecutor.ForgetUsername(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo(guid + ",接口调用结束");
            return Res;
        }
        #endregion

        #region 学信网忘记密码
        public VerCodeRes ChsiForgetPasswordStep1ForXml()
        {
            return ChsiForgetPasswordStep1();
        }
        public VerCodeRes ChsiForgetPasswordStep1ForJson()
        {
            return ChsiForgetPasswordStep1();
        }
        public VerCodeRes ChsiForgetPasswordStep1()
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiForgetPasswordStep1；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            VerCodeRes Res = new VerCodeRes();
            try
            {
                Res = chsiExecutor.ForgetPwd_Step1();
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo(guid + ",接口调用结束");
            return Res;
        }
        public VerCodeRes ChsiForgetPasswordStep2ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = reqText.DeserializeXML<ChsiForgetReq>();
            return ChsiForgetPasswordStep2(Req);
        }
        public VerCodeRes ChsiForgetPasswordStep2ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = jsonService.DeserializeObject<ChsiForgetReq>(reqText);
            return ChsiForgetPasswordStep2(Req);
        }
        public VerCodeRes ChsiForgetPasswordStep2(ChsiForgetReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiForgetPasswordStep2；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            VerCodeRes Res = new VerCodeRes();
            try
            {
                Res = chsiExecutor.ForgetPwd_Step2(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo(guid + ",接口调用结束");
            return Res;
        }
        public BaseRes ChsiForgetPasswordStep3ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = reqText.DeserializeXML<ChsiForgetReq>();
            return ChsiForgetPasswordStep3(Req);
        }
        public BaseRes ChsiForgetPasswordStep3ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = jsonService.DeserializeObject<ChsiForgetReq>(reqText);
            return ChsiForgetPasswordStep3(Req);
        }
        public BaseRes ChsiForgetPasswordStep3(ChsiForgetReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiForgetPasswordStep3；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = chsiExecutor.ForgetPwd_Step3(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo(guid + ",接口调用结束");
            return Res;
        }
        public BaseRes ChsiForgetPasswordStep4ForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = reqText.DeserializeXML<ChsiForgetReq>();
            return ChsiForgetPasswordStep4(Req);
        }
        public BaseRes ChsiForgetPasswordStep4ForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            ChsiForgetReq Req = jsonService.DeserializeObject<ChsiForgetReq>(reqText);
            return ChsiForgetPasswordStep4(Req);
        }
        public BaseRes ChsiForgetPasswordStep4(ChsiForgetReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：ChsiForgetPasswordStep4；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            Log4netAdapter.WriteInfo(guid + ",参数：" + jsonService.SerializeObject(req, true));
            BaseRes Res = new BaseRes();
            try
            {
                Res = chsiExecutor.ForgetPwd_Step4(req);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }
            Log4netAdapter.WriteInfo(guid + ",接口调用结束");
            return Res;
        }
        #endregion

        #endregion
    }
}