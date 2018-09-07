using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_CloudVipOrder")] 
	public class SuningCloudVipOrderEntity
	{
		public SuningCloudVipOrderEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string businessName  ;
        /// <summary>
        /// 商户号
        /// </summary>
		public string BusinessName
		{
			get { return businessName; }
			set { businessName = value; }
		}
      
		private string source  ;
        /// <summary>
        /// 来源
        /// </summary>
		public string Source
		{
			get { return source; }
			set { source = value; }
		}
      
		private int? cloudVip  ;
        /// <summary>
        /// 云钻
        /// </summary>
		public int? CloudVip
		{
			get { return cloudVip; }
			set { cloudVip = value; }
		}
      
		private int? cloudVipType  ;
        /// <summary>
        /// 云钻类型
        /// </summary>
		public int? CloudVipType
		{
			get { return cloudVipType; }
			set { cloudVipType = value; }
		}
      
		private DateTime? cloudVipCreateTime  ;
        /// <summary>
        /// 云钻产生时间
        /// </summary>
		public DateTime? CloudVipCreateTime
		{
			get { return cloudVipCreateTime; }
			set { cloudVipCreateTime = value; }
		}
      
		private DateTime? cloudVipEndTime  ;
        /// <summary>
        /// 云钻过期时间
        /// </summary>
		public DateTime? CloudVipEndTime
		{
			get { return cloudVipEndTime; }
			set { cloudVipEndTime = value; }
		}
        private string remark;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get { return remark; }
            set { remark = value; }
        }
		private long? userId  ;
        /// <summary>
        /// 客户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// 客户名称
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
