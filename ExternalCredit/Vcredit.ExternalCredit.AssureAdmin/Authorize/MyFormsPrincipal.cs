using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace Vcredit.ExternalCredit.AssureAdmin.Authorize
{
    /// <summary>
    /// 通用的用户实体
    /// </summary>
    /// <typeparam name="TUserData"></typeparam>
    public class MyFormsPrincipal<TUserData> : IPrincipal where TUserData : class, new()
    {
        /// <summary>
        /// 当前用户实例
        /// </summary>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public TUserData UserData { get; private set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="userData"></param>
        public MyFormsPrincipal(FormsAuthenticationTicket ticket, TUserData userData)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");
            if (userData == null)
                throw new ArgumentNullException("userData");

            Identity = new FormsIdentity(ticket);
            UserData = userData;
        }

        /// <summary>
        /// 角色验证
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsInRole(string role)
        {
            var userData = UserData as MyUserDataPrincipal;
            if (userData == null)
                throw new NotImplementedException();

            return userData.IsInRole(role);
        }

        /// <summary>
        /// 用户登录名验证
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsInUser(string user)
        {
            var userData = UserData as MyUserDataPrincipal;
            if (userData == null)
                throw new NotImplementedException();

            return userData.IsInUser(user);
        }
    }
}