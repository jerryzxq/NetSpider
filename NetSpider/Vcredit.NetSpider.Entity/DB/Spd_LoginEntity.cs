using System;
using System.Collections.Generic;
using Vcredit.Framework.Server.Entity;

namespace Vcredit.NetSpider.Entity.DB
{
    #region Spd_LoginEntity

    /// <summary>
    /// Spd_LoginEntity object for NHibernate mapped table 'Spd_Login'.
    /// </summary>
    public class Spd_LoginEntity
    {
        public virtual string Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public virtual string Mobile { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public virtual string IdentityCard { get; set; }
        /// <summary>
        /// 登录参数1
        /// </summary>
        public virtual string LoginParam1 { get; set; }
        /// <summary>
        /// 登录参数2
        /// </summary>
        public virtual string LoginParam2 { get; set; }
        /// <summary>
        /// 登录参数3
        /// </summary>
        public virtual string LoginParam3 { get; set; }
        /// <summary>
        /// 客户端IP
        /// </summary>
        public virtual string IPAddr { get; set; }
        /// <summary>
        /// spider类型
        /// </summary>
        public virtual string SpiderType { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public virtual int? StatusCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string StatusDesc { get; set; }
        
        private DateTime _CreateTime = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
    }
    #endregion
}