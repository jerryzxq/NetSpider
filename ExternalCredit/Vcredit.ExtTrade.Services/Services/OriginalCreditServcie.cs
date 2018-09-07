using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.Services.Contracts;
using Vcredit.Common.Ext;

using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using Vcredit.Common;
using Vcredit.ExtTrade.Services.Models;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
using System.ServiceModel.Activation;
using System.ServiceModel;
namespace Vcredit.ExtTrade.Services.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class OriginalCreditServcie:IOriginalCreditService
    {


        readonly CRD_CD_CreditUserInfoBusiness creditbus = new CRD_CD_CreditUserInfoBusiness();
        public BaseRes AddCreditUserInfo(System.IO.Stream userInfo)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = userInfo.AsStringText();
                Log4netAdapter.WriteInfo("请求参数：" + reqText);
                Log4netAdapter.WriteInfo("接口名：AddCreditUserInfo；客户端IP:" + CommonFun.GetClientIP());
                SingleCreditReq sreq = JsonConvert.DeserializeObject<SingleCreditReq>(reqText);
                var req = CheckAndReturnData(sreq, ref baseRes);
                if (req != null)
                {
                    if (ConfigData.SwitchInterfaceNum == "0")
                        new SingleRequest().SubmitReqest(req);
                    else
                        SingleRequest.ReceiveCreditInfo(req);
                    baseRes.Result = req.czPactNo;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口AddCreditUserInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        private SingleCreditReq CheckAndReturnData(SingleCreditReq sreq, ref  BaseRes res)
        {
            res.StatusCode = ServiceConsts.StatusCode_success;
            res.StatusDescription = "调用成功";
            if (string.IsNullOrEmpty(sreq.cifName) || string.IsNullOrEmpty(sreq.BusType) ||
                  string.IsNullOrEmpty(sreq.idNo) || string.IsNullOrEmpty(sreq.idType)
                  || string.IsNullOrEmpty(sreq.czAuth) || string.IsNullOrEmpty(sreq.czId)
                || string.IsNullOrEmpty(sreq.crpReason)  || !sreq.idNo.IsValidIdentity()
                || sreq.cifName.IsContianBlankSpace())
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "参数不合法";
                res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
                return null;
            }
            if (creditbus.isInLimitedDays(ConfigData.upLoadLimitedDays, sreq.idNo, sreq.cifName))
            {
                res.StatusCode = ServiceConsts.StatusCode_HaveGet;
                res.StatusDescription = "身份证号是：" + sreq.idNo + "的用户三十天之内不能重复上传";
                res.Result = GetFailReasonStr(WMFailReasonState.不能重复查询);
                return null;
            }
            if (CacheHelper.GetCache(sreq.idNo) != null)
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "已经提交过证件号是" + sreq.idNo + "的数据";
                res.Result = GetFailReasonStr(WMFailReasonState.不能重复查询);
                return null;
            }
            else
            {
                CacheHelper.SetCache(sreq.idNo, sreq, new TimeSpan(0, 0, 60));
            }
            return sreq;
        }
        private string GetFailReasonStr(WMFailReasonState state)
        {
            return JsonConvert.SerializeObject(new WMFailReason { FailReason = state });

        }
        public BaseRes UpLoadAuthFile(System.IO.Stream fileInfo)
        {
            BaseRes baseRes = new BaseRes();

            try
            {
                string reqText = fileInfo.AsStringText();
                CRD_CD_CreditUserInfoEntity credit=new CRD_CD_CreditUserInfoEntity ();
                Log4netAdapter.WriteInfo("接口名：UpLoadAuthFile；客户端IP:" + CommonFun.GetClientIP());
                var file = JsonConvert.DeserializeObject<ReceiveFile>(reqText);
                Log4netAdapter.WriteInfo("请求身份证" + file.idNo);
                if (string.IsNullOrEmpty(file.idNo) || string.IsNullOrEmpty(file.cifName)
                    || string.IsNullOrEmpty(file.AuthorizationBase64Str) || string.IsNullOrEmpty(file.CertBase64Str)
                    || string.IsNullOrEmpty(file.idType))
                {
                    baseRes.StatusDescription = "参数都是必须填写，不能为空";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
                else if (!creditbus.HaveNewRequest(file.idNo,ConfigData.upLoadLimitedDays,ref  credit))
                 {
                     baseRes.StatusDescription = "没有查询到授权查询征信的记录";
                     baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                 }
                else
                {
                    file.Receive((int)credit.CreditUserInfo_Id,credit.PactNo);
                    baseRes.StatusDescription = "UpLoadAuthFile调用完毕";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口UpLoadAuthFile请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
    }
}