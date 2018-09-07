using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.IO;
using System.Web;
using System.Web.Mvc;

[assembly: OwinStartup(typeof(Vcredit.ExternalCredit.AssureAdmin.Startup))]

namespace Vcredit.ExternalCredit.AssureAdmin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
