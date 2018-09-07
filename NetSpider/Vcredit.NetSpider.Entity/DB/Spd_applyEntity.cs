using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region Spd_applyEntity

    /// <summary>
    /// Spd_applyEntity object for NHibernate mapped table 'spd_apply'.
    /// </summary>
    public class Spd_applyEntity
    {
        public virtual int ApplyId { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public virtual string Identitycard { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public virtual string Mobile { get; set; }
        /// <summary>
        /// 采集网站
        /// </summary>
        public virtual string Website { get; set; }
        /// <summary>
        /// 采集类别
        /// </summary>
        public virtual string Spider_type { get; set; }
        /// <summary>
        /// 应用ID
        /// </summary>
        public virtual string AppId { get; set; }
        /// <summary>
        /// 客户端会话令牌
        /// </summary>
        public virtual string Token { get; set; }

        private DateTime _CreateTime = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        /// <summary>
        /// 申请状态
        /// </summary>
        public virtual int? Apply_status { get; set; }
        /// <summary>
        /// 采集状态
        /// </summary>
        public virtual int Crawl_status { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string Description { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public virtual string IPAddr { get; set; }

        private List<Spd_applyformEntity> _Spd_applyformList = new List<Spd_applyformEntity>();
        public virtual List<Spd_applyformEntity> Spd_applyformList
        {
            get { return _Spd_applyformList; }
            set { this._Spd_applyformList = value; }
        }
    }
    #endregion
}