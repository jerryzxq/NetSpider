using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.AssureWeb.Authorize;
using Vcredit.ExternalCredit.AssureWeb.Tools;

namespace Vcredit.ExternalCredit.AssureWeb.Controllers
{
    public class AccountController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            WebUtils.LogRequest(actionContext);
            base.OnActionExecuting(actionContext);
        }

        public ActionResult Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var isSuccess = model.LoginName.Equals(ConfigurationManager.AppSettings["LoginName"]) &&
                                    model.Password.Equals(ConfigurationManager.AppSettings["Password"]);
                    if (isSuccess)
                    {
                        //验证成功，用户名密码正确，构造用户数据（可以添加更多数据，这里只保存用户Id）
                        var userData = new MyUserDataPrincipal
                        {
                            LoginName = model.LoginName,
                        };
                        //保存Cookie
                        MyFormsAuthentication<MyUserDataPrincipal>.SetAuthCookie(model.LoginName, userData, true);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                        ModelState.AddModelError("loginfail", "用户名或密码错误！");
                }
            }

            return View(model);

        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            this.DelLoginCookie();
            return RedirectToAction("Login", "Account");
        }

        #region 删除登录cookie信息
        ///<summary>
        /// 删除登录cookie信息
        ///</summary>
        private void DelLoginCookie()
        {
            HttpCookie cookies = Request.Cookies[SysConst.CookieName];
            if (cookies != null)
            {
                cookies.Expires = DateTime.Today.AddDays(-1);
                Response.Cookies.Add(cookies);
                Request.Cookies.Remove(SysConst.CookieName);
            }
        }
        #endregion

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
				//return RedirectToAction("Index", "Home");
				return RedirectToAction("ApplyLimitList", "ApplyLimitMaintain");
            }
        }
    }
}