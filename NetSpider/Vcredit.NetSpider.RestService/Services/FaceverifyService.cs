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
using Vcredit.NetSpider.Entity.Service.Faceverify;
using Vcredit.NetSpider.ThirdParty.Baidu;
using Vcredit.NetSpider.ThirdParty.Minshi;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity.Mongo.Faceverify;

namespace Vcredit.NetSpider.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class FaceverifyService : IFaceverifyService
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

        public FaceverifyService()
        {
        }
        #region 人脸识别判断是否一个人


        public BaseRes FaceCompareByIdcardForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            FaceCompareReq Req = reqText.DeserializeXML<FaceCompareReq>();
            return FaceCompareByIdcard(Req);
        }

        public BaseRes FaceCompareByIdcardForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            FaceCompareReq Req = jsonService.DeserializeObject<FaceCompareReq>(reqText);
            return FaceCompareByIdcard(Req);
        }
        public BaseRes FaceCompareByIdcard(FaceCompareReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：FaceCompareByIdcard；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);
            
            BaseRes Res = new BaseRes();
            decimal compareResult=0;
            try
            {
                //数据校验
                if (req.IdentityCard.IsEmpty())
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = ServiceConsts.Required_IdentitycardEmpty;
                    return Res;
                }
                if (req.IdentityPhotoBase64.IsEmpty() || req.PersonPhotoBase64.IsEmpty())
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = ServiceConsts.Required_PhotoEmpty;
                    return Res;
                }

                FaceverifyMongo faceMongo=new FaceverifyMongo();
                FaceCompare faceMongoEntity = new FaceCompare();
                faceMongoEntity.IdentityCard = req.IdentityCard;
                faceMongoEntity.Name = req.Name;

                BaiduFaceverify baiduVerify = new BaiduFaceverify();
                compareResult=baiduVerify.FaceCompare(req.PersonPhotoBase64, req.IdentityPhotoBase64);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                faceMongoEntity.BaiduSimilarity = compareResult;
                if (compareResult >= 40)
                {
                    Res.Result = ServiceConsts.BaseRusult_True;
                }
                else
                {
                    Res.Result = ServiceConsts.BaseRusult_False;
                }

                //MinshiFaceverify minshiVerify = new MinshiFaceverify();
                //compareResult = minshiVerify.FaceCompare(req.PersonPhotoBase64, req.IdentityPhotoBase64);
                //faceMongoEntity.MinshiSimilarity = compareResult;
                //if (Res.Result == ServiceConsts.BaseRusult_False&&compareResult >= 70)
                //{
                //    Res.Result = ServiceConsts.BaseRusult_True;
                //}

                faceMongo.SaveFaceCompare(faceMongoEntity);
                Log4netAdapter.WriteInfo("FaceCompare=>" + req.IdentityCard + ",结果：" + compareResult);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }

            return Res;
        }
        #endregion

        #region 人脸识别得到识别度
        public BaseRes FaceCompareGetSimilarityForXml(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            FaceCompareReq Req = reqText.DeserializeXML<FaceCompareReq>();
            return FaceCompareGetSimilarity(Req);
        }

        public BaseRes FaceCompareGetSimilarityForJson(Stream stream)
        {
            //获得请求内容
            string reqText = stream.AsStringText(true);
            FaceCompareReq Req = jsonService.DeserializeObject<FaceCompareReq>(reqText);
            return FaceCompareGetSimilarity(Req);
        }
        public BaseRes FaceCompareGetSimilarity(FaceCompareReq req)
        {
            string guid = CommonFun.GetGuidID();
            Log4netAdapter.WriteInfo("接口：FaceCompareGetSimilarity；客户端IP:" + CommonFun.GetClientIP() + "；" + guid);

            BaseRes Res = new BaseRes();
            decimal compareResult = 0;
            try
            {
                FaceverifyMongo faceMongo = new FaceverifyMongo();
                FaceCompare faceMongoEntity = new FaceCompare();
                faceMongoEntity.IdentityCard = req.IdentityCard;
                faceMongoEntity.Name = req.Name;

                BaiduFaceverify baiduVerify = new BaiduFaceverify();
                compareResult = baiduVerify.FaceCompare(req.PersonPhotoBase64, req.IdentityPhotoBase64);
               
                faceMongoEntity.BaiduSimilarity = compareResult;

                //MinshiFaceverify minshiVerify = new MinshiFaceverify();
                //compareResult = minshiVerify.FaceCompare(req.PersonPhotoBase64, req.IdentityPhotoBase64);
                faceMongoEntity.MinshiSimilarity = compareResult;

                //faceMongo.SaveFaceCompare(faceMongoEntity);

                var result = new {
                    BaiduSimilarity = faceMongoEntity.BaiduSimilarity,
                    MinshiSimilarity = faceMongoEntity.MinshiSimilarity
                };
                Res.StatusCode = ServiceConstants.StatusCode_success;
                Res.Result = jsonService.SerializeObject(result);

                Log4netAdapter.WriteInfo("FaceCompare=>" + req.IdentityCard + ",结果：" + compareResult);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = e.Message;
            }

            return Res;
        }
        #endregion
    }
}