using Vcredit.ExternalCredit.AssureWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using Vcredit.ExternalCredit.AssureWeb.Tools;

namespace Vcredit.ExternalCredit.AssureWeb.Authorize
{
    /// <summary>
    /// 身份验证类
    /// </summary>
    /// <typeparam name="TUserData">用户数据</typeparam>
    public class MyFormsAuthentication<TUserData> where TUserData : class, new()
    {
        /// <summary>
        /// 点击记住我 Cookie保存是时间（天）
        /// </summary>
        private static int CookieSaveDays = 1;

        /// <summary>
        /// cookie 默认有效时间（小时）
        /// </summary>
        private static int CookieDefaultHours = SysConst.CookieDefaultHours;

        /// <summary>
        /// 用户登录成功时设置Cookie
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="userData">用户数据</param>
        /// <param name="rememberMe">是否记住</param>
        public static void SetAuthCookie(string loginName, TUserData userData, bool rememberMe)
        {
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }

            var data = (new JavaScriptSerializer()).Serialize(userData);

            //创建ticket
            var ticket = new FormsAuthenticationTicket(
                2,
                loginName,
                DateTime.Now,
                DateTime.Now.AddDays(CookieSaveDays),
                rememberMe,
                data);

            //加密ticket
            var cookieValue = FormsAuthentication.Encrypt(ticket);

            //创建Cookie
            var cookie = new HttpCookie(SysConst.CookieName, cookieValue)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                Path = FormsAuthentication.FormsCookiePath,
            };
            if (rememberMe)
            {
                cookie.Expires = DateTime.Now.AddDays(CookieSaveDays);
            }
            else
            {
                cookie.Expires = DateTime.Now.AddHours(CookieDefaultHours);
            }

            //写入Cookie
            HttpContext.Current.Response.Cookies.Remove(cookie.Name);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 从Request中解析出Ticket,UserData
        /// </summary>
        /// <param name="request">http请求</param>
        /// <returns>用户对象</returns>
        public static MyFormsPrincipal<TUserData> TryParsePrincipal(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            // 1. 读登录Cookie
            var cookie = request.Cookies[SysConst.CookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                return null;
            }

            try
            {
                // 2. 解密Cookie值，获取FormsAuthenticationTicket对象
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null && !string.IsNullOrEmpty(ticket.UserData))
                {
                    var userData = (new JavaScriptSerializer()).Deserialize<TUserData>(ticket.UserData);
                    if (userData != null)
                    {
                        return new MyFormsPrincipal<TUserData>(ticket, userData);
                    }
                }

                return null;
            }
            catch
            {
                /* 有异常也不要抛出，防止攻击者试探。 */
                return null;
            }
        }
    }
}