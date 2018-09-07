using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Vcredit.ExtTrade.Services.Services;

namespace Vcredit.ExtTrade.Services
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new ServiceRoute("ExtTrade", new WebServiceHostFactory(), typeof(UpLoadCreditInfoService)));
            RouteTable.Routes.Add(new ServiceRoute("OrgCredit/Query", new WebServiceHostFactory(), typeof(QueryService)));
            RouteTable.Routes.Add(new ServiceRoute("OriginalExtTrade", new WebServiceHostFactory(), typeof(OriginalCreditServcie)));
            RouteTable.Routes.Add(new ServiceRoute("ShangHaiCredit", new WebServiceHostFactory(), typeof(ShangHaiLoanService)));

        }
    }
}