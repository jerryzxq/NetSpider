using Vcredit.ExternalCredit.AssureWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vcredit.ExternalCredit.AssureWeb.Authorize
{
    /// <summary>
    /// 验证角色和用户名的类
    /// </summary>
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 是否有模块权限
        /// </summary>
        private bool HasAuthority = true;

        /// <summary>
        /// 验证信息
        /// </summary>
        /// <param name="httpContext">http上下文</param>
        /// <returns>是否授权</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.User as MyFormsPrincipal<MyUserDataPrincipal>;
            if (user != null)
            {
                HasAuthority = (user.IsInRole(Roles) && user.IsInUser(Users));
                return HasAuthority;
            }

            return false;
        }

        /// <summary>
        /// 验证未通过处理
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //验证不通过,直接跳转到相应页面，注意：如果不使用以下跳转，则会继续执行Action方法
            //filterContext.Result = new RedirectResult("http://www.baidu.com");

            filterContext.Result = new RedirectResult("~/Account/Login?returnUrl=" + filterContext.HttpContext.Request.RawUrl);
        }
    }
}