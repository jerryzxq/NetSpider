using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.AssureApi.Tools;

namespace Vcredit.ExternalCredit.AssureApi.Filters
{
    /// <summary>
    /// 记录请求参数Filter
    /// </summary>
    public class LogParamFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                //var ip = new IPHelper(actionContext.Request.GetOwinContext()).IP;
                var ip = CommonFun.GetClientIP();

                string body = string.Empty;
                foreach (var argument in actionContext.ActionArguments.Values)
                    body += JsonConvert.SerializeObject(argument);

                Log4netAdapter.WriteInfo(string.Format("OnActionExecuting ==> IP：{0}，请求Url：{1}，Body参数为：{2}", ip, actionContext.Request.RequestUri.AbsoluteUri, body));
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("OnActionExecuting ==> 参数记录出现异常"), ex);
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                var ip = CommonFun.GetClientIP();

                string body = string.Empty;
                foreach (var argument in actionExecutedContext.ActionContext.ActionArguments.Values)
                    body += JsonConvert.SerializeObject(argument);

                var t = (actionExecutedContext.Response.Content).ReadAsStringAsync().ContinueWith(task =>
                {
                    var value = task.Result;
                    Log4netAdapter.WriteInfo(
                        string.Format("OnActionExecuted ==> IP：{0}，请求Url：{1}，Body参数为：{2}，请求返回结果：{3}",
                           ip,
                           actionExecutedContext.Request.RequestUri.AbsoluteUri,
                           body,
                           value));
                });
                t.Wait();
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("OnActionExecuted ==> 参数记录出现异常"), ex);
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}