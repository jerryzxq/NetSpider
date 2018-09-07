using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Vcredit.ExternalCredit.AssureApi.Middleware;
using Microsoft.Owin.Extensions;
using System.Web.SessionState;
using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using Vcredit.ExternalCredit.CrawlerLayer.Assure;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.ExternalCredit.Services;

[assembly: OwinStartup(typeof(Vcredit.ExternalCredit.AssureApi.Startup))]

namespace Vcredit.ExternalCredit.AssureApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888

            // 配置详见：http://autofac.readthedocs.io/en/latest/integration/webapi.html
            // STANDARD WEB API SETUP:
            // Get your HttpConfiguration. In OWIN, you'll create one
            // rather than using GlobalConfiguration.
            var config = new HttpConfiguration();

            var builder = new ContainerBuilder();
            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // 接口注册
            builder.Register(c => new CreditQueryImpl()).As<CreditQuery>().InstancePerRequest();
            builder.Register(c => new ComplianceServiceImpl()).As<ComplianceService>().InstancePerRequest();

            // Middleware 注册
            //builder.RegisterType<AnalysisParamMiddleware>().InstancePerRequest();

            // Run other optional steps, like registering filters,
            // per-controller-type services, etc., then set the dependency resolver
            // to be Autofac.
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            WebApiConfig.Register(config);
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }
    }
}
