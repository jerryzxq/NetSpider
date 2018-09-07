using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Newtonsoft.Json;
using Vcredit.ExtTrade.ModelLayer;
using System.IO;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExtTrade.Services;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.Common.Utility;
using Vcredit.Common.Ext;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan;
using Vcredit.Common;
using Vcredit.ExternalCredit.Services;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExtTrade.Services.Models;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.CrawlerLayer;
namespace Vcredit.ExtTrade.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class UpLoadCreditInfoService : ICreditInfoService
    {
        readonly CreditUserInfo cuBus = new CreditUserInfo();
        readonly BridgingBusiness bridg = new BridgingBusiness();
        readonly CRD_CD_CreditUserInfoBusiness creditbus = new CRD_CD_CreditUserInfoBusiness();
        readonly ComplianceService comService = new ComplianceServiceImpl();
        #region 外贸征信
        /// <summary>
        /// 上传查询信息
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public BaseRes AddCreditUserInfo(Stream userInfo)
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
                   SingleRequest.ReceiveCreditInfo(req);
                    baseRes.Result = req.czPactNo;
                    ComplianceCall(sreq);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口AddCreditUserInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
                baseRes.Result = GetFailReasonStr(WMFailReasonState.数据保存失败);
            }
            return baseRes;
        }
        private bool IsSignatured(SingleCreditReq req)
        {
            bool result = true;
            var respone = comService.IsSignatured(new IsSignaturedRequest()
            {
                ApplyID = req.ApplyID,
                IdentityNo = req.idNo,
                Name = req.cifName

            });
            if (respone != null && respone.IsSeccess)
            {
                if (!respone.IsSignature)
                {
                    result = false;
                    Log4netAdapter.WriteInfo(req.idNo + "还未签名不能查征");
                }
            }
            else
            {
                Log4netAdapter.WriteInfo(req.idNo + "合规验证接口返回异常");
                result = false;

            }
            return result;
        }
        private string GetFailReasonStr(WMFailReasonState state)
        {
            return JsonConvert.SerializeObject(new WMFailReason { FailReason = state });

        }
        private SingleCreditReq CheckAndReturnData(SingleCreditReq sreq, ref  BaseRes res)
        {
            res.StatusCode = ServiceConsts.StatusCode_success;
            res.StatusDescription = "调用成功";
            if (sreq.grantType != null && sreq.grantType != CommonData.OffLineAuthorization && sreq.grantType != CommonData.LineAuthorization)
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "授权方式不对，必须是01（线上授权）或者02（线下授权）";
                res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
                return null;
            }         
            if (sreq.grantType == null)
                sreq.grantType = CommonData.LineAuthorization;
            //var expDate = sreq.Cert_ExpDate.ToDateTime();
            //if (expDate == null)
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_fail;
            //    res.StatusDescription = "身份证过期日期没有填写，或者不合法";
            //    res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
            //    return null;
            //}
            //if (expDate.Value.AddDays(-(ConfigData.CertReviseDays - 1)) < DateTime.Now)
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_fail;
            //    res.StatusDescription = "身份证已经过期";
            //    res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);

            //    return null;
            //}
            if (string.IsNullOrEmpty(sreq.cifName) || string.IsNullOrEmpty(sreq.BusType) ||
                  string.IsNullOrEmpty(sreq.idNo) || string.IsNullOrEmpty(sreq.idType)
                  || string.IsNullOrEmpty(sreq.czAuth) || string.IsNullOrEmpty(sreq.czId)
                || string.IsNullOrEmpty(sreq.crpReason) || (sreq.ApplyID == 0 && sreq.crpReason == CommonData.LoanApproval) || !sreq.idNo.IsValidIdentity()
                || sreq.cifName.IsContianBlankSpace())
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "参数不合法";
                res.Result = GetFailReasonStr(WMFailReasonState.实体校验失败);
                return null;
            }
            if (!BusTypeVerify.CheckBusType(CreditType.ExtTrade, sreq.BusType))
            {
                res.StatusCode = ServiceConsts.StatusCode_fail;
                res.StatusDescription = "BusType没有权限";
                res.Result = GetFailReasonStr(WMFailReasonState.BusType没有权限);
                return null;
            }
            if (creditbus.isInLimitedDays_Assure(ConfigData.upLoadLimitedDays, sreq.idNo, sreq.cifName))
            {
                ComplianceCall(sreq);//回调合规接口
                res.StatusCode = ServiceConsts.StatusCode_HaveGet;
                res.StatusDescription = "身份证号是：" + sreq.idNo + "的用户" + ConfigData.upLoadLimitedDays.ToString() + "天之内不能重复上传";
                res.Result = GetFailReasonStr(WMFailReasonState.不能重复查询);
                return null;
            }
            //if (sreq.BusType != CommonData.XINLOUDAI && !Commonfunction.AgeIsBetween18And60(sreq.idNo))
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_fail;
            //    res.StatusDescription = "年龄不在18到60周岁之间";
            //    res.Result = GetFailReasonStr(WMFailReasonState.超出时间限制);
            //    return null;
            //}
            //if (sreq.BusType == CommonData.XINLOUDAI && !Commonfunction.AgeIsBetween18And60(sreq.idNo, 18, 65))
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_fail;
            //    res.StatusDescription = "年龄不在18到65周岁之间";
            //    res.Result = GetFailReasonStr(WMFailReasonState.超出时间限制);
            //    return null;
            //}
            //if (sreq.crpReason != CommonData.LoanAfterManager)
            //{
            //    if (!IsSignatured(sreq))
            //    {
            //        res.StatusCode = ServiceConsts.StatusCode_fail;
            //        res.StatusDescription = sreq.idNo + "还没有签名或者签名接口返回空值";
            //        res.Result = GetFailReasonStr(WMFailReasonState.没有手写签名);
            //        return null;
            //    }
            //}
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

        private void ComplianceCall(SingleCreditReq sreq)
        {
            if (sreq.crpReason == CommonData.LoanAfterManager)
                return;
            var respone = comService.CallBack(new CallBackRequest()
            {
                ApplyID = sreq.ApplyID
            });
            if (respone != null && !respone.IsSeccess)
            {
                Log4netAdapter.WriteInfo("身份证号是：" + sreq.idNo + "的用户回调合规接口失败");
            }
            if (respone == null)
            {
                Log4netAdapter.WriteInfo("身份证号是：" + sreq.idNo + "的用户回调合规接口响应为null值");
            }
        }
        private void GetOtherInfo(CRD_CD_CreditUserInfoEntity creditentity)
        {
            creditentity.Name = System.Text.UTF8Encoding.UTF8.GetString(Convert.FromBase64String(creditentity.Name));
            creditentity.User_Code = ConfigData.userCode;
            creditentity.Query_Org = ConfigData.orgCode;
            creditentity.SourceType = (byte)SysEnums.SourceType.Trade;
        }
        #endregion
        public BaseRes GetReportIdAndReportSnByIdentityNo(string identityno)
        {
            Log4netAdapter.WriteInfo("请求参数：" + identityno);
            Log4netAdapter.WriteInfo("接口名：GetReportIdAndReportSnByIdentityNo,客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                var result = bridg.GetReportIDAndStateByIdentityNo(identityno, ConfigData.upLoadLimitedDays);
                if (result.Item1 == 0)
                {
                    baseRes.StatusDescription = "该用户没有数据或者已经过期";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
                else
                {
                    baseRes.Result = result.Item1.ToString() + "," + result.Item2 + ",";
                    baseRes.StatusDescription = "GetReportIdReportSnAndStateByIdentityNo调用完毕";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetReportIdReportSnAndStateByIdentityNo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        public BaseRes GetReportStateByIdentityNo(string identityno)
        {
            Log4netAdapter.WriteInfo("请求参数：" + identityno);
            Log4netAdapter.WriteInfo("接口名：GetReportStateByIdentityNo,客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                int state = creditbus.GetStateBycert_no(identityno, ConfigData.upLoadLimitedDays);
                if (state != -1)
                {
                    baseRes.Result = state.ToString();
                    baseRes.StatusDescription = "GetReportStateByIdentityNo调用完毕";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    baseRes.StatusDescription = "状态不存在或者已经过期";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetReportStateByIdentityNo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }

        /// <summary>
        /// 上传授权文件
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public BaseRes UpLoadAuthFile(Stream fileInfo)
        {
            BaseRes baseRes = new BaseRes();

            try
            {
                string reqText = fileInfo.AsStringText();
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
                else
                {
                    file.Receive();
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
        /// <summary>
        /// 更新历史数据的ApplyId
        /// </summary>
        /// <param name="applyInfo"></param>
        /// <returns></returns>
        public BaseRes UpdateApplyID(Stream applyInfo)
        {
            BaseRes baseRes = new BaseRes();
            Action<int, string, string> action = (status, des, res) =>
                {
                    baseRes.StatusCode = status;
                    baseRes.StatusDescription = des;
                    baseRes.Result = res;
                };
            try
            {
                string reqText = applyInfo.AsStringText();
                Log4netAdapter.WriteInfo("请求参数：" + reqText);
                Log4netAdapter.WriteInfo("接口名：AddCreditUserInfo；客户端IP:" + CommonFun.GetClientIP());
                var req = JsonConvert.DeserializeObject<UpdateApplyIDReq>(reqText);
                if (req != null)
                {
                    if (string.IsNullOrEmpty(req.Date) || string.IsNullOrEmpty(req.Cert_No) || string.IsNullOrEmpty(req.Name)
                        || string.IsNullOrEmpty(req.BusType) || req.ApplyID == null || !req.Cert_No.IsValidIdentity())
                    {
                        action(ServiceConsts.StatusCode_fail, "参数不合法", GetFailReasonStr(WMFailReasonState.实体校验失败));
                        return baseRes;
                    }
                    var credit = creditbus.Select(req.Cert_No, req.Name, req.BusType, req.Date);
                    if (credit == null)
                    {
                        action(ServiceConsts.StatusCode_fail, "没有查询到更新数据", GetFailReasonStr(WMFailReasonState.没有查询到数据));
                    }
                    else if (credit.ApplyID != null)
                    {
                        action(ServiceConsts.StatusCode_success, "已经存在ApplyID", GetFailReasonStr(WMFailReasonState.已经存在数据));
                    }
                    else
                    {
                        credit.ApplyID = req.ApplyID;
                        if (creditbus.UpdateOnlyApplyId(credit))
                            action(ServiceConsts.StatusCode_success, "成功更新", null);
                        else
                            action(ServiceConsts.StatusCode_fail, "数据更新失败", GetFailReasonStr(WMFailReasonState.更新数据失败));
                    }
                }
                else
                {
                    action(ServiceConsts.StatusCode_fail, "序列化出现问题", GetFailReasonStr(WMFailReasonState.序列化出现问题));
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口UpdateApplyID请求异常", ex);
                action(ServiceConsts.StatusCode_error, "请求出现异常", null);
            }
            return baseRes;
        }

    }

}