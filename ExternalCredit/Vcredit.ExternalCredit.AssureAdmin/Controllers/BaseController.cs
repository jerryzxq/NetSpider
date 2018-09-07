using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.AssureAdmin.Authorize;
using Vcredit.ExternalCredit.AssureAdmin.Tools;

namespace Vcredit.ExternalCredit.AssureAdmin.Controllers
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