using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vcredit.Common.Utility;

namespace Vcredit.ExternalCredit.AssureWeb.Tools
{
    public class WebUtils
    {
        public static void LogRequest(ActionExecutingContext actionContext)
        {
            try
            {
                string ip = CommonFun.GetClientIP();

                string body = string.Empty;
                foreach (var argument in actionContext.ActionParameters.Values)
                    body += JsonConvert.SerializeObject(argument);

                Log4netAdapter.WriteInfo(string.Format("IP：{0}，请求Url：{1}，Body参数为：{2}", ip, actionContext.RequestContext.HttpContext.Request.Path, body));
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("参数记录出现异常"), ex);
            }

        }
    }
}