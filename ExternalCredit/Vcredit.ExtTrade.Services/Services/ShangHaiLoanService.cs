using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.Common.Ext;
using Vcredit.Common;
using Vcredit.ExtTrade.Services.Models;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.ExternalCredit.Services;
using System.ServiceModel.Activation;
using System.ServiceModel;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExternalCredit.CrawlerLayer;
namespace Vcredit.ExtTrade.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class ShangHaiLoanService : IShangHaiLoanService
    {
    
        readonly CRD_CD_CreditUserInfoBusiness creditbus = new CRD_CD_CreditUserInfoBusiness();
        readonly ComplianceService comService = new ComplianceServiceImpl();
        public BaseRes AddQueryInfo(Stream userInfo)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
       
                string reqText = userInfo.AsStringText();
                var ip= CommonFun.GetClientIP();
                Log4netAdapter.WriteInfo("接口名：GetShangHaiLoanCredit；客户端IP:" + ip);
                var creditInfo = JsonConvert.DeserializeObject<CRD_CD_CreditUserInfoEntity>(reqText);
                CheckAndReturnData(creditInfo,ref  baseRes);
                if (baseRes.StatusCode == ServiceConsts.StatusCode_success)
                {
                    DirectPostRequestMessage(creditInfo);
                    Log4netAdapter.WriteInfo(string.Format("GetShangHaiLoanCredit请求参数CertNO:{0},CertType:{1},Name:{2}", creditInfo.Cert_No, creditInfo.Cert_Type, creditInfo.Name));
                    baseRes.StatusDescription = "GetShangHaiLoanCredit调用完毕";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;

                }
                Log4netAdapter.WriteInfo(baseRes.StatusDescription);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetShangHaiLoanCredit请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        public void DirectPostRequestMessage(CRD_CD_CreditUserInfoEntity requestMessage)
        {
            requestMessage.UpdateTime = DateTime.Now;
            requestMessage.State = (byte)RequestState.Default;
            requestMessage.SourceType = (int)SysEnums.SourceType.ShangHai;
            requestMessage.Query_Org = ConfigData.orgCode;
            requestMessage.Authorization_Date = DateTime.Now.ToString("yyyyMMdd");
            requestMessage.FileState = (int)AuthorizationFileState.Default;
            creditbus.Save(requestMessage);
          

        }
        private void  CheckAndReturnData(CRD_CD_CreditUserInfoEntity credit,ref  BaseRes res)
        {
            res.StatusCode = ServiceConsts.StatusCode_success;
            res.StatusDescription = "调用成功";
            //var expDate = credit.Cert_ExpDate.ToDateTime();
            //if (expDate == null)
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_fail;
            //    res.StatusDescription = "身份证过期日期没有填写，或者不合法";
            //    res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
            //    return ;
            //}
            //if (expDate.Value.AddDays(-(ConfigData.CertReviseDays - 1)) < DateTime.Now)
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_fail;
            //    res.StatusDescription = "身份证已经过期";
            //    res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
            //    return ;
            //}
            if (string.IsNullOrEmpty(credit.Name) || string.IsNullOrEmpty(credit.BusType) ||
                  string.IsNullOrEmpty(credit.Cert_No) || string.IsNullOrEmpty(credit.Cert_Type)
                || string.IsNullOrEmpty(credit.QueryReason)
                || (credit.ApplyID == null && credit.QueryReason != CommonData.LoanAfterManager && credit.BusType != "VBS")
                || !credit.Cert_No.IsValidIdentity()
                || credit.Name.IsContianBlankSpace())
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "参数不合法";
                res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
                return ;
            }
            if(creditbus.isInLimitedDays_Assure(ConfigData.upLoadLimitedDays, credit.Cert_No, credit.Name))
            {
                ComplianceCall(credit);//回调合规接口
                res.StatusCode = ServiceConsts.StatusCode_HaveGet;
                res.StatusDescription = "身份证号是：" + credit.Cert_No + "的用户"+ConfigData.upLoadLimitedDays+"之内不能重复上传";
                res.Result = GetFailReasonStr(WMFailReasonState.不能重复查询);
                return ;
            }
            if (!BusTypeVerify.CheckBusType(CreditType.ShangHaiCredit,credit.BusType))
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "身份证号是：" + credit.Cert_No +"不支持业务"+credit.BusType;
                res.Result = GetFailReasonStr(WMFailReasonState.BusType没有权限);
                return;
            }
            if (!Commonfunction.AgeIsBetween18And60(credit.Cert_No))
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "年龄不在18到60周岁之间";
                res.Result = GetFailReasonStr(WMFailReasonState.超出时间限制);
                return ;
            }
            if (credit.QueryReason != CommonData.LoanAfterManager&&credit.BusType != "VBS")
            {
                if (!IsSignatured(credit))
                {
                    res.StatusCode = ServiceConsts.StatusCode_fail;
                    res.StatusDescription = credit.Cert_No + "还没有签名或者签名接口返回空值";
                    res.Result = GetFailReasonStr(WMFailReasonState.没有手写签名);
                    return ;
                }
            }
            if (CacheHelper.GetCache(credit.Cert_No) != null)
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "已经提交过证件号是" + credit.Cert_No+ "的数据";
                res.Result = GetFailReasonStr(WMFailReasonState.不能重复查询);
                return ;
            }
            else
            {
                CacheHelper.SetCache(credit.Cert_No, credit, new TimeSpan(0, 0, 60));
            }
        }
        private string GetFailReasonStr(WMFailReasonState state)
        {
            return JsonConvert.SerializeObject(new WMFailReason { FailReason = state });

        }
        private void ComplianceCall(CRD_CD_CreditUserInfoEntity sreq)
        {
            if (sreq.QueryReason == CommonData.LoanAfterManager)
                return;
            var respone = comService.CallBack(new CallBackRequest()
            {
                ApplyID = sreq.ApplyID??0
            });
            if (respone != null && !respone.IsSeccess)
            {
                Log4netAdapter.WriteInfo("身份证号是：" + sreq.Cert_No + "的用户回调合规接口失败");
            }
            if (respone == null)
            {
                Log4netAdapter.WriteInfo("身份证号是：" + sreq.Cert_No + "的用户回调合规接口响应为null值");
            }
        }
        private bool IsSignatured(CRD_CD_CreditUserInfoEntity req)
        {
            bool result = true;
            var respone = comService.IsSignatured(new IsSignaturedRequest()
            {
                ApplyID = req.ApplyID??0,
                IdentityNo = req.Cert_No,
                Name = req.Name

            });
            if (respone != null && respone.IsSeccess)
            {
                if (!respone.IsSignature)
                {
                    result = false;
                    Log4netAdapter.WriteInfo(req.Cert_No + "还未签名不能查征");
                }
            }
            else
            {
                Log4netAdapter.WriteInfo(req.Cert_No + "合规验证接口返回异常");
                result = false;

            }
            return result;
        }

   
    }
}