using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.Services.Impl
{
    /// <summary>
    /// 调用合规接口服务
    /// </summary>
    public class ComplianceServiceImpl : ComplianceService
    {
        private HttpItem _httpItem;

        private HttpHelper _httpHelper = new HttpHelper();

        private HttpResult _httpResult;

        private static readonly string  _complianceQuery = "/Compliance/Query";
        private static readonly string _complianceCallBack = "/Compliance/CallBack";
        private static readonly string _complianceApply = "/Compliance/Apply";
		private static readonly string _complianceApplyExternal = "/Compliance/WMCreditApply";
        private static readonly string _complianceApplyChengdu = "/Compliance/CDXDCreditApply";


        /// <summary>
        /// 合规查询接口，返回是否签名
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IsSignaturedResponse IsSignatured(IsSignaturedRequest request)
        {
            try
            {
                var url = ConfigData.ComplianceServiceUrl;

                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("ComplianceServiceUrl 手写签名地址不能为空");

                url += _complianceQuery;

                var postdata = JsonConvert.SerializeObject(request);
                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    PostEncoding = Encoding.UTF8, // 参数中有中文
                    ContentType = "application/json",
                };
                //_httpItem.ProxyIp = "127.0.0.1:8888";

                _httpResult = _httpHelper.GetHtml(_httpItem);
                var html = _httpResult.Html;

                Log4netAdapter.WriteInfo(string.Format("是否已手写签名接口 url: {0} 参数：{1} 接口返回：{2} ", url, postdata, html));

                var response = JsonConvert.DeserializeObject<IsSignaturedResponse>(html);

                return response;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("请求是否已手写签名接口出现异常！", ex);
                return null;
            }
        }

        /// <summary>
        /// 合规是否签名回掉接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CallBackResponse CallBack(CallBackRequest request)
        {
            try
            {
                var url = ConfigData.ComplianceServiceUrl;

                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("ComplianceServiceUrl 合规是否签名回掉接口地址不能为空");

                url += _complianceCallBack + "?ApplyID=" + request.ApplyID;

                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "get",
                };

                _httpResult = _httpHelper.GetHtml(_httpItem);
                var html = _httpResult.Html;

                Log4netAdapter.WriteInfo(string.Format("合规是否签名回掉接口 url: {0} 接口返回：{1} ", url, html));

                var response = JsonConvert.DeserializeObject<CallBackResponse>(html);

                return response;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("请求合规是否签名回掉接口出现异常！", ex);
                return null;
            }
        }

		/// <summary>
		/// 合规申请接口（担保）
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public ApplyResponse Apply(ApplyRequest request)
		{
			try
			{
				var url = ConfigData.ComplianceServiceUrl;

				if (string.IsNullOrEmpty(url))
					throw new ArgumentException("ComplianceServiceUrl 合规申请接口地址不能为空");

				url += _complianceApply;

				var postdata = JsonConvert.SerializeObject(request);
				_httpItem = new HttpItem
				{
					URL = url,
					Method = "POST",
					Postdata = postdata,
					PostEncoding = Encoding.UTF8, // 参数中有中文
					ContentType = "application/json",
				};

				_httpResult = _httpHelper.GetHtml(_httpItem);
				var html = _httpResult.Html;

                Log4netAdapter.WriteInfo(string.Format("合规申请接口 url: {0} 接口返回：{1} ", url, html));

				var response = JsonConvert.DeserializeObject<ApplyResponse>(html);

				return response;
			}
			catch (Exception ex)
			{
				Log4netAdapter.WriteError("合规申请接口出现异常！", ex);
				return null;
			}
		}

		/// <summary>
		/// 合规申请接口（外贸）
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public ApplyResponse ApplyExternal(ApplyRequest request)
		{
			try
			{
				var url = ConfigData.ComplianceServiceUrl;

				if (string.IsNullOrEmpty(url))
					throw new ArgumentException("ComplianceServiceUrl 合规申请接口地址不能为空");

				url += _complianceApplyExternal;

				var postdata = JsonConvert.SerializeObject(request);
				_httpItem = new HttpItem
				{
					URL = url,
					Method = "POST",
					Postdata = postdata,
					PostEncoding = Encoding.UTF8, // 参数中有中文
					ContentType = "application/json",
				};

				_httpResult = _httpHelper.GetHtml(_httpItem);
				var html = _httpResult.Html;

				//Log4netAdapter.WriteInfo(string.Format("合规申请接口 url: {0} 参数：{1} 接口返回：{2} ", url, postdata, html));
                Log4netAdapter.WriteInfo(string.Format("合规申请接口 url: {0} 接口返回：{1} ", url, html));

                var response = JsonConvert.DeserializeObject<ApplyResponse>(html);

				return response;
			}
			catch (Exception ex)
			{
				Log4netAdapter.WriteError("合规申请接口出现异常！", ex);
				return null;
			}
		}
        /// <summary>
        /// 合规申请接口（成都）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ApplyResponse ApplyChengdu(ApplyRequest request)
        {
            try
            {
                var url = ConfigData.ComplianceServiceUrl;

                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("ComplianceServiceUrl 合规申请接口地址不能为空");

                url += _complianceApplyChengdu;

                var postdata = JsonConvert.SerializeObject(request);
                _httpItem = new HttpItem
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    PostEncoding = Encoding.UTF8, // 参数中有中文
                    ContentType = "application/json",
                };

                _httpResult = _httpHelper.GetHtml(_httpItem);
                var html = _httpResult.Html;

                //Log4netAdapter.WriteInfo(string.Format("合规申请接口 url: {0} 参数：{1} 接口返回：{2} ", url, postdata, html));
                Log4netAdapter.WriteInfo(string.Format("合规申请接口 url: {0} 接口返回：{1} ", url, html));

                var response = JsonConvert.DeserializeObject<ApplyResponse>(html);

                return response;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("合规申请接口出现异常！", ex);
                return null;
            }
        }
    }
}
