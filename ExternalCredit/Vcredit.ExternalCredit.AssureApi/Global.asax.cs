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

namespace Vcredit.ExternalCredit.AssureApi
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 在应用程序启动时运行的代码
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
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
    }
}