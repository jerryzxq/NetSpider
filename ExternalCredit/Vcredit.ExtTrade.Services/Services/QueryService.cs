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
using Vcredit.ExtTrade.Services.Models;
using Vcredit.ExternalCredit.CrawlerLayer.CreditVariable;
using Newtonsoft.Json.Converters;
using Vcredit.ExternalCredit.Dto.Common;
using Vcredit.ExternalCredit.CrawlerLayer;

namespace Vcredit.ExtTrade.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class QueryService : IQueryService
    {
        string clientIp = CommonFun.GetClientIP();
        CRD_CD_CreditUserInfoBusiness creditbus = new CRD_CD_CreditUserInfoBusiness();
        readonly BridgingBusiness bridg = new BridgingBusiness();
        bool isBase64 = false;
     

		/// <summary>
		/// 根据身份证号获取机构版征信报告查询状态
		/// </summary>
		/// <param name="identityno">身份证号</param>
		/// <returns></returns>
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
		/// 根据身份证号获取报告ID和报告编号(report_sn)
		/// </summary>
		/// <param name="identityno">身份证号</param>
		/// <returns></returns>
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

        /// <summary>
        /// 机构版征信统计信息（变量）
        /// </summary>
        /// <param name="creditInfo">提交参数对象</param>
        /// <returns></returns>
        public BaseRes GetExtCreditSummary(Stream creditInfo)
        {
            BaseRes baseRes = new BaseRes();
            ReportCaculation caculation = new ReportCaculation();
            try
            {
                string reqText = creditInfo.AsStringText(isBase64);
                Log4netAdapter.WriteInfo(string.Format("接口名：GetExtCreditSummary；客户端IP:{0},请求参数：{1}", clientIp,reqText));
                var req = JsonConvert.DeserializeObject<CreditQueryReq>(reqText);

                if (req==null||string.IsNullOrEmpty(req.ReportSn))
                {
                    baseRes.StatusDescription = "报告编号不能为空";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                }
                else
                {
                    VariableInfo summary = new VariableInfo();
                    summary = caculation.GetByReport_sn(req.ReportSn);
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    if (summary == null)
                    {
                        baseRes.StatusDescription = "报告编号不存在";
                    }
                    else
                    {
                        baseRes.Result = JsonConvert.SerializeObject(summary);
                        baseRes.StatusDescription = "GetExtCreditSummary调用完毕";
                    }
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetExtCreditSummary请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 机构版征信申请记录查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseRes QueryExtCreditReqest(Stream request)
        {
         
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = request.AsStringText(true);
                Log4netAdapter.WriteInfo(string.Format("接口名：QueryExtCreditReqest；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<Vcredit.ExtTrade.ModelLayer.Nolmal.QueryRequestEntity>(reqText);
                if(req==null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "提供的数据字段有问题！";
                    return baseRes;
                }
                if(req.PageIndex==0||req.PageSize==0)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "分页参数不能为0";
                    return baseRes;
                }
                if (!string.IsNullOrEmpty(req.StartDate) || !string.IsNullOrEmpty(req.StartDate))
                {
                    if (req.StartDate.ToDateTime() == null)
                        baseRes.StatusDescription = "开始时间格式不正确,";
                    if (req.EndDate.ToDateTime() == null)
                        baseRes.StatusDescription += "截止时间格式不正确";
                    if (!string.IsNullOrEmpty(baseRes.StatusDescription))
                    {
                        baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                        return baseRes;
                    }
                }
                var list= creditbus.GetByRequest(req);
                baseRes.Result = JsonConvert.SerializeObject(list, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "成功";
                    
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口QueryExtCreditReqest请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 开放查询产品类型
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseRes GetBusTypeEnum(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = request.AsStringText(true);
                Log4netAdapter.WriteInfo(string.Format("接口名：GetBusTypeEnum；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var list =BusTypeVerify.BusTypeInfoList.AllKeys;
                baseRes.Result = JsonConvert.SerializeObject(list);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "成功";

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口QueryExtCreditReqest请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 获取机构版征信报告查询状态（新）
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        public BaseRes GetStateInfo(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = request.AsStringText(isBase64);
                Log4netAdapter.WriteInfo(string.Format("接口名：GetStateInfo；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<Vcredit.ExtTrade.ModelLayer.Nolmal.QueryRequestEntity>(reqText);
                if (req == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "提供的数据字段有问题！";
                    return baseRes;
                }
                var credit = creditbus.GetStateInfoBycert_no(req.Cert_No, ConfigData.upLoadLimitedDays,req.Name);
                if(credit==null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "状态不存在或者已过期";
                }
                else
                {
                    baseRes.Result = JsonConvert.SerializeObject(new { State = credit.State, ErrorReason = credit.Error_Reason, ReportSn = credit.Report_sn, SourceType = credit.SourceType });
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetStateInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 获取机构版征信报告基本信息
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        public BaseRes GetReportInfo(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = request.AsStringText(isBase64);
                Log4netAdapter.WriteInfo(string.Format("接口名：GetReportInfo；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<Vcredit.ExtTrade.ModelLayer.Nolmal.QueryRequestEntity>(reqText);
                if (req == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "提供的数据字段有问题！";
                    return baseRes;
                }
                var report =new BridgingBusiness().GetReportWithinLimiteDays(req.Cert_No, ConfigData.upLoadLimitedDays, req.Name);
                if (report == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "征信报告信息不存在或者已经过期";
                }
                else
                {
                    baseRes.Result = JsonConvert.SerializeObject(new { ReportID = report.Report_Id, ReportSn = report.Report_Sn,ReportCreateTime= report.Report_Create_Time,SourceType=report.SourceType });
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetReportInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }

        /// <summary>
        /// 获取机构版征信报告基本信息,无时间限制
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseRes GetReportInfoNoLimit(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = request.AsStringText(isBase64);
                Log4netAdapter.WriteInfo(string.Format("接口名：GetReportInfoNoLimit；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<Vcredit.ExtTrade.ModelLayer.Nolmal.QueryRequestEntity>(reqText);
                if (req == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "提供的数据字段有问题！";
                    return baseRes;
                }
                var report = new BridgingBusiness().GetReportID(req.Cert_No);
                if (report == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "征信报告信息不存在";
                }
                else
                {
                    baseRes.Result = JsonConvert.SerializeObject(new { ReportID = report.Report_Id, ReportSn = report.Report_Sn, ReportCreateTime = report.Report_Create_Time, SourceType = report.SourceType });
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetReportInfoNoLimit请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 获取身份证信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseRes GetIdentityInfo(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            try
            {
                string reqText = request.AsStringText();
                Log4netAdapter.WriteInfo(string.Format("接口名：GetIdentityInfo；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<IdentityRequest>(reqText);
                if (req == null || (req.ReportId == 0 && req.ReportSn == null))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "提供的数据字段有问题！";
                    return baseRes;
                }
                Vcredit.ExtTrade.ModelLayer.Nolmal.CRD_PI_IDENTITYEntity identity = null;
                if (req.ReportId != 0)
                {
                    identity = creditbus.QueryIdentityInfo(req.ReportId);
                }
                else
                {
                    identity = creditbus.QueryIdentityInfo(req.ReportSn);
                }
                if (identity == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "没有征信报告，或者没有用户身份信息！";
                }
                else
                {
                    var identityInfo = MapperHelper.Map<Vcredit.ExtTrade.ModelLayer.Nolmal.CRD_PI_IDENTITYEntity, IdentityInfo>(identity);
                    baseRes.Result = JsonConvert.SerializeObject(identityInfo);
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetIdentityInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 根据征信编号、征信ID，查询征信信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseRes QueryCreditInfo(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            QueryCredit qcredit = new QueryCredit();
            try
            {
                string reqText = request.AsStringText();
                Log4netAdapter.WriteInfo(string.Format("接口名：QueryCreditInfo；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<QueryCreditInfoReq>(reqText);
                if (req == null || (req.ReportID == null&& req.ReportSn == null))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_ReqError;
                    baseRes.StatusDescription = "提供的数据字段有问题！";
                    return baseRes;
                }
                string json =null;
                if(req.ReportID!=null)
                {
                    json = qcredit.GetCreditInfo(req.ReportID.Value);
                }
                else
                {
                    json  = qcredit.GetCreditInfo(req.ReportSn);
                    
                }
                if(json ==null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_NotExist;
                    baseRes.StatusDescription = "信息不存在";
                }
                else
                {

                    baseRes.Result =json;
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口GetIdentityInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
        /// <summary>
        /// 查询征信地址信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseRes QueryCreditAddressInfo(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            QueryCredit qcredit = new QueryCredit();
            try
            {
                string reqText = request.AsStringText();
                Log4netAdapter.WriteInfo(string.Format("接口名：QueryCreditAddressInfo；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<QueryCreditInfoReq>(reqText);
                if (req == null || req.ReportID == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_ReqError;
                    baseRes.StatusDescription = "提供的数据参数有问题！";
                    return baseRes;
                }
                var addressList = qcredit.GetCreditAddressList(req.ReportID.Value);
                if (addressList.Count == 0)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_NotExist;
                    baseRes.StatusDescription = "没有居住信息";
                }
                else
                {

                    baseRes.Result = JsonConvert.SerializeObject(addressList);
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口QueryCreditAddressInfo请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }




        public BaseRes QueryCreditAddressByCert_NO(Stream request)
        {

            BaseRes baseRes = new BaseRes();
            QueryCredit qcredit = new QueryCredit();
            try
            {
                string reqText = request.AsStringText();
                Log4netAdapter.WriteInfo(string.Format("接口名：QueryCreditAddressByCert_NO；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<QueryCreditInfoReq>(reqText);
                if (req == null ||req.Cert_No == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_ReqError;
                    baseRes.StatusDescription = "提供的数据参数有问题！";
                    return baseRes;
                }
                var contact = qcredit.GetCreditAddressListByCert_No(req.Cert_No);
                if (contact == null)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_NotExist;
                    baseRes.StatusDescription = "没有居住信息";
                }
                else
                {

                    baseRes.Result = JsonConvert.SerializeObject(contact);
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口QueryCreditAddressByCert_NO请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }


        public BaseRes QueryForceExctnNum(Stream request)
        {
            BaseRes baseRes = new BaseRes();
            QueryCredit qcredit = new QueryCredit();
            try
            {
                string reqText = request.AsStringText();
                Log4netAdapter.WriteInfo(string.Format("接口名：QueryForceExctnNum；客户端IP:{0},请求参数：{1}", clientIp, reqText));
                var req = JsonConvert.DeserializeObject<QueryCreditInfoReq>(reqText);
                if (req == null || (req.Cert_No == null && req.ReportID == null))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_ReqError;
                    baseRes.StatusDescription = "提供的数据参数有问题！";
                    return baseRes;
                }
                var count = qcredit.QueryForceExctnNum(req.Cert_No,req.ReportID);
                if (count == -1)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_NotExist;
                    baseRes.StatusDescription = "没有征信信息";
                }
                else
                {

                    baseRes.Result =count.ToString();
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "成功";
                }

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("接口QueryForceExctnNum请求异常", ex);
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = ex.Message;
            }
            return baseRes;
        }
    }

}