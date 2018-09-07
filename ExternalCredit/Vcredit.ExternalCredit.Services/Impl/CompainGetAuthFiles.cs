using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.Services.Impl
{
     public class CompainGetAuthFiles : IComplianAdditionalService
    {
        readonly HttpHelper _httpHelper = new HttpHelper();
        private static readonly string _complianceQuery = "/Compliance/GetUserFileImages";
        public UserFileRespne GetAuthenticationFile(UserFilesRequest request)
        {
            UserFileRespne result = null;
            try
            {
                var url = ConfigData.ComplianceServiceUrl;
                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("GetAuthenticationFile 合规获取授权文件地址不能为空");
                url += _complianceQuery;

                var postdata = JsonConvert.SerializeObject(request);
                var _httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    PostEncoding = Encoding.UTF8, // 参数中有中文
                    ContentType = "application/json",
                };
                var _httpResult = _httpHelper.GetHtml(_httpItem);
                if (_httpResult.StatusCode == HttpStatusCode.OK)
                {
                    result = JsonConvert.DeserializeObject<UserFileRespne>(_httpResult.Html);
                }
                else
                {
                    Log4netAdapter.WriteInfo("合规获取授权文件可能是网络问题，没有获取成功！");
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("合规获取授权文件出现异常！", ex);
            }
            return result;
        }
    }
}
