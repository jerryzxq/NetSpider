using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_ACCOUNTEntity

	/// <summary>
	/// CRD_ACCOUNTEntity object for NHibernate mapped table 'CRD_ACCOUNT'.
	/// </summary>
	public class CRD_ACCOUNTEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string UserName{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Password{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string QueryCode{get; set;}

        private DateTime _CreateTime = DateTime.Now;
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        public virtual string CertNo { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public virtual string CertType { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public virtual string Mobile { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public virtual string Email { get; set; }
	}
	#endregion
}