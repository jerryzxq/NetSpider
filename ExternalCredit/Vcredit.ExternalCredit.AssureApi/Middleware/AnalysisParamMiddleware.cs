using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Vcredit.Common.Utility;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.AssureApi.Tools;

namespace Vcredit.ExternalCredit.AssureApi.Middleware
{
    /// <summary>
    /// 参数解析
    /// </summary>
    public class AnalysisParamMiddleware : OwinMiddleware
    {
        public AnalysisParamMiddleware(OwinMiddleware next) : base(next)
        { }

        public override Task Invoke(IOwinContext context)
        {
            return Task.Run(() =>
            {
                bool isBase64 = true;
                var ip = new IPHelper(context).IP;

                // post请求 Body base64 参数解码，所有访问api参数使用base64编码
                if (isBase64 &&
                    string.Equals(context.Request.Method, "Post", StringComparison.OrdinalIgnoreCase))
                {
                    if (context.Request.Body != null && context.Request.Body.Length > 0)
                    {
                        string body = new StreamReader(context.Request.Body).ReadToEnd();
                        Log4netAdapter.WriteInfo(string.Format("AnalysisParamMiddleware => ip: {0}, 请求接口: {1}， 请求Base64参数为: {2}"
                                                    , ip
                                                    , context.Request.Path.ToString()
                                                    , body));

                        byte[] bodydecoder = Encoding.Default.GetBytes(body);
                        try
                        {
                            bodydecoder = Convert.FromBase64String(body);

                            Log4netAdapter.WriteInfo(string.Format("AnalysisParamMiddleware => ip: {0}, 请求接口: {1}， 请求Base64解码后参数为: {2}"
                                                    , ip
                                                    , context.Request.Path.ToString()
                                                    , Encoding.UTF8.GetString(bodydecoder)));

                            // base64 解码重新写入 Request.Body
                            context.Request.Body = new MemoryStream(bodydecoder);
                        }
                        catch (Exception ex)
                        {
                            Log4netAdapter.WriteError("Base64 参数解析失败", ex);

                            var result = new ApiResultDto<string>
                            {
                                StatusCode = StatusCode.Fail,
                                StatusDescription = "请求失败，请求参数必须为Base64编码"
                            };
                            context.Response.Write(JsonConvert.SerializeObject(result));
                            return;
                        }
                    }
                }

                this.Next.Invoke(context);
            });
        }
    }
}