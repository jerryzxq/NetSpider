using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.IO;
using Vcredit.Common.Utility;
using System.Threading;
using Vcredit.ExternalCredit.AssureAdmin.Tools;
using Vcredit.ExternalCredit.AssureAdmin.Authorize;

namespace Vcredit.ExternalCredit.AssureAdmin
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 在应用程序启动时运行的代码
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            log4net.Config.XmlConfigurator.Configure(File.OpenRead(Server.MapPath("~/Configs/Log4net.config")));
        }

        /// <summary>
        /// 全局异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex is ThreadAbortException)
                return;

            Log4netAdapter.WriteError("系统异常", ex);
        }

        void Application_End(object sender, EventArgs e)
        {
            Log4netAdapter.WriteInfo("【Application_End】");
        }

        /// <summary>
        /// 登录身份认证
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_PostAuthenticateRequest(object sender, System.EventArgs e)
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[SysConst.CookieName];
            if (authCookie != null)
            {
                HttpContext.Current.User =
                    MyFormsAuthentication<MyUserDataPrincipal>.TryParsePrincipal(HttpContext.Current.Request);

            }
        }
    }
}