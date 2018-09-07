using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Serialization;

namespace Vcredit.ExternalCredit.AssureAdmin.Authorize
{
    //存放数据的用户实体
    public class MyUserDataPrincipal : IPrincipal
    {
        //数据源
        //private readonly MingshiEntities mingshiDb = new MingshiEntities();

        #region 自定义属性

        /// <summary>
        /// 用户ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户登录名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户名字
        /// </summary>
        public string Name { get; set; }

        ////这里可以定义其他一些属性        
        #endregion

        #region 验证用户角色（该方法可根据自己的系统角色验证用户角色）
        /// <summary>
        /// 当使用Authorize特性时，会调用改方法验证角色 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return true;

            //// 此处考虑多种角色，目前本系统只有一种角色
            //var userRoles = ((int)Role).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //var roles = role.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //return (from s in roles
            //        from urole in userRoles
            //        where s.Equals(urole)
            //        select s).Any();

            return true;
        }

        /// <summary>
        /// 当使用Authorize特性时，会调用改方法验证用户信息
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public bool IsInUser(string users)
        {
            //找出用户所有所属角色
            //var users = user.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //return mingshiDb.User.Any(u => users.Contains(u.UserName));

            if (string.IsNullOrEmpty(users))
                return true;

            var userArray = users.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return userArray.Contains(LoginName);
        }
        #endregion

        [ScriptIgnore]    //在序列化的时候忽略该属性
        public IIdentity Identity { get { throw new NotImplementedException(); } }
    }
}