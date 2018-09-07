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
using Vcredit.NetSpider.DataAccess.Ftp;

namespace Vcredit.WeiXin.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class VbsService : IVbsService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口
        IChsiExecutor chsiExecutor = ExecutorManager.GetChsiExecutor();

        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ftpDirectory = "/jxlreport";
        bool isbase64 = true;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public VbsService()
        {
        }
        #endregion

        #region 学信网
        public BaseRes GetChsiInfoForXml(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                VbsQueryReq Req = reqText.DeserializeXML<VbsQueryReq>();
                baseRes = GetChsiInfo(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetChsiInfoJson(Stream stream)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                VbsQueryReq Req = jsonService.DeserializeObject<VbsQueryReq>(reqText);
                baseRes = GetChsiInfo(Req);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public BaseRes GetChsiInfo(VbsQueryReq Req)
        {
            Log4netAdapter.WriteInfo("接口：GetChsiInfo；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                //ISpd_Login loginService = NetSpiderFactoryManager.GetSpd_LoginService();
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                IChsi_Info chsiService = NetSpiderFactoryManager.GetChsi_InfoService();

                //var login = loginService.GetByIdentityCard(Req.Identitycard, "chsiquery");
                //var appyly = applyService.GetByIdentityCardAndSpiderType(Req.Identitycard, "chsiquery");
                //IList<Chsi_InfoEntity> entitys = chsiService.GetListByToken(appyly.Token);
                IList<Chsi_InfoEntity> entitys = chsiService.GetListByIdentityCard(Req.Identitycard);

                baseRes.Result = jsonService.SerializeObject(entitys);

                baseRes.StatusDescription = "信息已获取";
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = e.Message;
            }
            return baseRes;
        }

        public Stream GetChsiPhotoForXml(Stream stream)
        {
            Stream retStream = null;
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                VbsQueryReq Req = reqText.DeserializeXML<VbsQueryReq>();
                retStream = GetChsiPhoto(Req);
            }
            catch (Exception e)
            {

            }
            return retStream;
        }

        public Stream GetChsiPhotoForJson(Stream stream)
        {
            Stream retStream = null;
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(isbase64);
                Log4netAdapter.WriteInfo("参数:" + reqText);
                VbsQueryReq Req = jsonService.DeserializeObject<VbsQueryReq>(reqText);
                retStream = GetChsiPhoto(Req);
            }
            catch (Exception e)
            {
               
            }
            return retStream;
        }

        public Stream GetChsiPhoto(VbsQueryReq Req)
        {
            Log4netAdapter.WriteInfo("接口：GetChsiPhoto；客户端IP:" + CommonFun.GetClientIP());
            Stream retStream = null;
            try
            {
                EduChsiFTP ftp = new EduChsiFTP();
                byte[] buffer = ftp.DownloadChsiPhotoToByte(Req.FileName.ToUrlDecode());
                retStream = FileOperateHelper.BytesToStream(buffer);

                return retStream;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("获取学信网图片出错", e);
            }
            return retStream;
        }
        #endregion
    }
}