using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using Quartz;
using Quartz.Impl;
using Vcredit.WeiXin.RestService;
using Vcredit.WeiXin.RestService.Job;
using Vcredit.WeiXin.RestService.Services;

namespace Vcredit.WeiXin.RestService
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.Start();

            RouteTable.Routes.Add(new ServiceRoute("kkd", new WebServiceHostFactory(), typeof(KKDService)));
            RouteTable.Routes.Add(new ServiceRoute("vbs", new WebServiceHostFactory(), typeof(VbsService)));
            RouteTable.Routes.Add(new ServiceRoute("jxl", new WebServiceHostFactory(), typeof(JxlService)));
            RouteTable.Routes.Add(new ServiceRoute("rc", new WebServiceHostFactory(), typeof(RCService)));
            RouteTable.Routes.Add(new ServiceRoute("bank", new WebServiceHostFactory(), typeof(BankService)));
        }

        void Application_End(object sender, EventArgs e)
        {
            //  在应用程序关闭时运行的代码
        }

        void Application_Error(object sender, EventArgs e)
        {
            // 在出现未处理的错误时运行的代码

        }
    }
}
