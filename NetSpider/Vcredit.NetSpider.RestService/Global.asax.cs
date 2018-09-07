using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using Quartz;
using Quartz.Impl;
using Vcredit.NetSpider.RestService;
using Vcredit.NetSpider.RestService.Services;

namespace Vcredit.NetSpider.RestService
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.Start();
            RouteTable.Routes.Add(new ServiceRoute("SpiderService", new WebServiceHostFactory(), typeof(SpiderService)));
            RouteTable.Routes.Add(new ServiceRoute("credit", new WebServiceHostFactory(), typeof(CreditService)));
            RouteTable.Routes.Add(new ServiceRoute("edu", new WebServiceHostFactory(), typeof(EduService)));
            RouteTable.Routes.Add(new ServiceRoute("mobile", new WebServiceHostFactory(), typeof(MobileService)));
            RouteTable.Routes.Add(new ServiceRoute("gjj", new WebServiceHostFactory(), typeof(ProvidentFundService)));
            RouteTable.Routes.Add(new ServiceRoute("shebao", new WebServiceHostFactory(), typeof(SocialSecurityService)));
            RouteTable.Routes.Add(new ServiceRoute("faceverify", new WebServiceHostFactory(), typeof(FaceverifyService)));
            RouteTable.Routes.Add(new ServiceRoute("job", new WebServiceHostFactory(), typeof(JobService)));
            RouteTable.Routes.Add(new ServiceRoute("config", new WebServiceHostFactory(), typeof(ConfigService)));
            RouteTable.Routes.Add(new ServiceRoute("common", new WebServiceHostFactory(), typeof(CommonService)));
            RouteTable.Routes.Add(new ServiceRoute("dangernum", new WebServiceHostFactory(), typeof(DangerNumberService)));
            RouteTable.Routes.Add(new ServiceRoute("graynumber", new WebServiceHostFactory(), typeof(GrayNumberService)));
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
