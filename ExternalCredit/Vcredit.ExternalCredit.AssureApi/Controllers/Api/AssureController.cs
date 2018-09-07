using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Vcredit.ExternalCredit.AssureApi.Filters;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExternalCredit.CrawlerLayer;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExternalCredit.Services;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.AssureApi.Controllers
{
    /// <summary>
    /// 担保征信
    /// </summary>
	//[BusTypeFilter]
    [RoutePrefix("api/Assure")]
    public class AssureController : ApiController
    {
        /// <summary>
        /// 初始化并登陆
        /// </summary>
        /// <returns></returns>
        [Route("Login")]
        public ApiResultDto<AssureLoginResultDto> Login()
        {
            var result = _creditQuery.Login();

            return result;
        }

        private CreditQuery _creditQuery;

        private ComplianceService _signaturedService;

        public AssureController(CreditQuery creditQuery, ComplianceService complianceService)
        {
            _creditQuery = creditQuery;
            _signaturedService = complianceService;
        }

        /// <summary>
        /// 征信查询数据提交
        /// </summary>
        /// <param name="param">请求参数</param>
        /// <returns></returns>
        [Route("AddQueryInfo")]
        [HttpPost]
        [ResponseType(typeof(ApiResultDto<AddQueryInfoResultDto>))]
        [LogParamFilter]
        [BusTypeFilter]
        public IHttpActionResult AddQueryInfo(AssureQueryUserInfoParamDto param)
        {
            var result = new ApiResultDto<AddQueryInfoResultDto>
            {
                StatusCode = Dto.StatusCode.Fail,
                Result = new AddQueryInfoResultDto(),
            };

            if (!ModelState.IsValid)
            {
                result.Result.FailReason = SysEnums.AssureFailReasonState.实体校验失败;
                result.StatusDescription = "实体校验失败，" + this.GetModoleStateString();
                return base.Ok(result);
            }
         
            // a. 校验身份证、姓名
            bool inLimitDays;
            if (ConfigData.TwoQuerySwitch == "OPEN")
            {
                inLimitDays = new CRD_CD_CreditUserInfoBusiness().isInLimitedDays_Assure(ConfigData.upLoadLimitedDays, param.CertNo, param.Name);
            }
            else
            {
                inLimitDays = new CRD_CD_CreditUserInfoBusiness().isInLimitedDays(ConfigData.upLoadLimitedDays, param.CertNo, param.Name);
            }
            if (inLimitDays)
            {
                // a.1 保存成功调用回调接口
                this.Compliance_CallBack(param);

                result.Result.FailReason = SysEnums.AssureFailReasonState.不能重复查询;
                result.StatusDescription = string.Format("{0} 天内不能重复查询", ConfigData.upLoadLimitedDays);
                return base.Ok(result);
            }
			if (!Commonfunction.AgeIsBetween18And60(param.CertNo))
			{
				result.Result.FailReason = SysEnums.AssureFailReasonState.超出时间限制;
				result.StatusDescription = "年龄不在18到60周岁之间";
				return base.Ok(result);
			}
            // 若是贷后管理则不检验合规资料申请
            if (param.QueryReason != CommonData.LoanAfterManager)
            {
                // b. 校验是否手写签名
                var isSignatured = this.Compliance_IsSignatured(param);
                if (!isSignatured)
                {
                    result.Result.FailReason = SysEnums.AssureFailReasonState.没有手写签名;
                    result.StatusDescription = string.Format("用户没有手写签名，无法提交查询");
                    return base.Ok(result);
                }
            }

			// c.时间限制，时间限制外无法操作
			bool canOpt = new TimeLimitHandler().HandleRequest(new HandleRequest()).DoNextHandler;
			if (!canOpt)
			{
				result.Result.FailReason = SysEnums.AssureFailReasonState.超出时间限制;
				result.StatusDescription = "时间限制外无法操作";
				return base.Ok(result);
			}

			// d. 校验请求次数是否超出限制
			var applyLimitBusiness = new CRD_CD_CreditApplyLimitBusiness();
			var bAllowApply = applyLimitBusiness.IsApply((int)SysEnums.SourceType.Assure);
			if (!bAllowApply)
			{
				result.Result.FailReason = SysEnums.AssureFailReasonState.申请次数超出;
				result.StatusDescription = string.Format("日查询超限，无法提交查询");
				return base.Ok(result);
			}

            result = _creditQuery.AddQueryInfo(param);
            // d. 保存成功调用回调接口
            if (result.StatusCode == Dto.StatusCode.Success)
                this.Compliance_CallBack(param);

            return Ok(result);
        }

        /// <summary>
        /// 获取用户是否已经手写签名
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private bool Compliance_IsSignatured(AssureQueryUserInfoParamDto param)
        {
            var response = _signaturedService.IsSignatured(new IsSignaturedRequest
            {
                IdentityNo = param.CertNo,
                Name = param.Name,
                ApplyID = param.ApplyID,
            });
            return response.IsSignature;
        }
        /// <summary>
        /// 保存成功回调
        /// </summary>
        /// <param name="param"></param>
        private void Compliance_CallBack(AssureQueryUserInfoParamDto param)
        {
            if (param.QueryReason != CommonData.LoanAfterManager)
            {
                var response = _signaturedService.CallBack(new CallBackRequest
                {
                    ApplyID = param.ApplyID,
                });
            }
        }

        /// <summary>
        /// 获取白名单业务线
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("BusTypes")]
        [HttpGet]
        [ResponseType(typeof(ApiResultDto<List<string>>))]
        public IHttpActionResult BusTypes()
        {
            var types = BusTypeFilterAttribute.BUS_TYPE_LIST.Select(x => x.BusType).ToList();

            return Ok(new ApiResultDto<List<string>>
            {
                StatusCode = Dto.StatusCode.Success,
                StatusDescription = "数据获取成功",
                Result = types,
            });
        }

        /// <summary>
        /// 合规申请接口（担保）
        /// </summary>
        /// <param name="param">请求参数</param>
        /// <returns></returns>
        [Route("Apply")]
        [HttpPost]
        [ResponseType(typeof(ApplyResponse))]
        [AllowAnonymous]
        [LogComplianceParamFilter]
        public HttpResponseMessage Apply(AssureApplyParamDto param)
        {
            // 说明：由于webapiConfig统一配置json序列化----CamelCase
            // 合规接口返回key大写开头，因此这里返回HttpResponseMessage 返回保持返回值与合规接口一致
            var resmsg = new HttpResponseMessage();
            var response = new ApplyResponse() { IsSeccess = false };

            if (!ModelState.IsValid)
            {
                response.Message = "实体校验失败：" + this.GetModoleStateString();

                resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
                return (resmsg);
            }

            // a. 查询是否有有效征信
            var inLimitDays = new CRD_CD_CreditUserInfoBusiness().isInLimitedDays_Assure(ConfigData.upLoadLimitedDays, param.IdentityNo, param.Name);
            if (inLimitDays)
            {
                response.Message = string.Format("{0} 天内不能重复查询", ConfigData.upLoadLimitedDays);

                resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
                return (resmsg);
            }

            var applyRequest = new ApplyRequest();
            applyRequest = MapperHelper.Map<AssureApplyParamDto, ApplyRequest>(param);
            applyRequest.ApplyInfo = MapperHelper.Map<ApplyInfoEntityDto, ApplyInfoEntity>(param.ApplyInfo);

            response = _signaturedService.Apply(applyRequest);
            resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
            return (resmsg);
        }

        /// <summary>
        /// 合规申请接口（外贸）
        /// </summary>
        /// <param name="param">请求参数</param>
        /// <returns></returns>
        [Route("WMCreditApply")]
        [HttpPost]
        [ResponseType(typeof(ApplyResponse))]
        [AllowAnonymous]
        [LogComplianceParamFilter]
        public HttpResponseMessage ApplyExternal(AssureApplyParamDto param)
        {
            // 说明：由于webapiConfig统一配置json序列化----CamelCase
            // 合规接口返回key大写开头，因此这里返回HttpResponseMessage 返回保持返回值与合规接口一致
            var resmsg = new HttpResponseMessage();
            var response = new ApplyResponse() { IsSeccess = false };

            if (!ModelState.IsValid)
            {
                response.Message = "实体校验失败：" + this.GetModoleStateString();

                resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
                return (resmsg);
            }

            // a. 查询是否有有效征信
            var inLimitDays = new CRD_CD_CreditUserInfoBusiness().isInLimitedDays_Assure(ConfigData.upLoadLimitedDays, param.IdentityNo, param.Name);
            if (inLimitDays)
            {
                response.Message = string.Format("{0} 天内不能重复查询", ConfigData.upLoadLimitedDays);

                resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
                return (resmsg);
            }

            var applyRequest = new ApplyRequest();
            applyRequest = MapperHelper.Map<AssureApplyParamDto, ApplyRequest>(param);
            applyRequest.ApplyInfo = MapperHelper.Map<ApplyInfoEntityDto, ApplyInfoEntity>(param.ApplyInfo);

            response = _signaturedService.ApplyExternal(applyRequest);
            resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
            return (resmsg);
        }
        /// <summary>
        /// 合规申请接口（成都）
        /// </summary>
        /// <param name="param">请求参数</param>
        /// <returns></returns>
        [Route("CDXDCreditApply")]
        [HttpPost]
        [ResponseType(typeof(ApplyResponse))]
        [AllowAnonymous]
        [LogComplianceParamFilter]
        public HttpResponseMessage ApplyChengdu(AssureApplyParamDto param)
        {
            // 说明：由于webapiConfig统一配置json序列化----CamelCase
            // 合规接口返回key大写开头，因此这里返回HttpResponseMessage 返回保持返回值与合规接口一致
            var resmsg = new HttpResponseMessage();
            var response = new ApplyResponse() { IsSeccess = false };

            if (!ModelState.IsValid)
            {
                response.Message = "实体校验失败：" + this.GetModoleStateString();

                resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
                return (resmsg);
            }

            // a. 查询是否有有效征信
            var inLimitDays = new CRD_CD_CreditUserInfoBusiness().isInLimitedDays_Assure(ConfigData.upLoadLimitedDays, param.IdentityNo, param.Name);
            if (inLimitDays)
            {
                response.Message = string.Format("{0} 天内不能重复查询", ConfigData.upLoadLimitedDays);

                resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
                return (resmsg);
            }

            var applyRequest = new ApplyRequest();
            applyRequest = MapperHelper.Map<AssureApplyParamDto, ApplyRequest>(param);
            applyRequest.ApplyInfo = MapperHelper.Map<ApplyInfoEntityDto, ApplyInfoEntity>(param.ApplyInfo);

            response = _signaturedService.ApplyChengdu(applyRequest);
            resmsg.Content = new StringContent(JsonConvert.SerializeObject(response));
            return (resmsg);
        }
        private string GetModoleStateString()
        {
            var message = string.Join(" | ", ModelState.Values
                           .SelectMany(v => v.Errors)
                           .Select(e => e.ErrorMessage));

            return message;
        }
    }
}
