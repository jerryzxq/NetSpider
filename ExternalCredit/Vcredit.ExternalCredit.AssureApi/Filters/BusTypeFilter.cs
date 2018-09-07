using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CrawlerLayer;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExternalCredit.Dto.Common;
namespace Vcredit.ExternalCredit.AssureApi.Filters
{
    /// <summary>
    /// 白名单Filter
    /// </summary>
    public class BusTypeFilterAttribute : ActionFilterAttribute
    {

        public static readonly List<WhiteBusTypeDto> BUS_TYPE_LIST = JsonConvert.DeserializeObject<List<WhiteBusTypeDto>>(File.ReadAllText(UniversalFilePathResolver.ResolvePath("~\\Configs\\BusTypeList.json")));
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var errData = new ApiResultDto<AddQueryInfoResultDto>
            {
                StatusCode = StatusCode.Fail,
                StatusDescription = "BusType 尚未添加到白名单，请联系管理员",
                Result = new AddQueryInfoResultDto { FailReason = SysEnums.AssureFailReasonState.BusType没有权限 }
            };
            try
            {
                if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0)   // 允许匿名访问
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }
                string body = string.Empty;
                foreach (var argument in actionContext.ActionArguments.Values)
                    body += JsonConvert.SerializeObject(argument);

                var jobj = JObject.Parse(body);
                var busType = jobj.SelectToken("$..BusType").ToString();

                if (!BusTypeVerify.CheckBusType(CreditType.AssureCredit,busType))
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, errData);
                    return;

                }

                // TODO: 添加其它验证方法
                base.OnActionExecuting(actionContext);
            }
            catch
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, errData);
            }
        }
    }
   
}