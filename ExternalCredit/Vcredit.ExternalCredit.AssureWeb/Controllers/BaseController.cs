using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.AssureWeb.Authorize;
using Vcredit.ExternalCredit.AssureWeb.Tools;

namespace Vcredit.ExternalCredit.AssureWeb.Controllers
{
    [MyAuthorize]
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            WebUtils.LogRequest(actionContext);
            base.OnActionExecuting(actionContext);
        }
    }
}