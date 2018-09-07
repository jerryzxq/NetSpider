using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.VipShop
{
	[Alias("vipshop_receiveAddress")]
	public class VipshopReceiveAddressEntity
	{
		public VipshopReceiveAddressEntity() { }

		#region Attributes
      
		private long id  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public long Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private long userId  ;
        /// <summary>
        /// 
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string area  ;
        /// <summary>
        /// 所在地区
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string address  ;
        /// <summary>
        /// 详细地址
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string zip  ;
        /// <summary>
        /// 邮政编码
        /// </summary>
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
      
		private string receiver  ;
        /// <summary>
        /// 收货人姓名
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// 手机号码
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string telePhone  ;
        /// <summary>
        /// 电话号码
        /// </summary>
		public string TelePhone
		{
			get { return telePhone; }
			set { telePhone = value; }
		}
      
		private bool? isDefault  ;
        /// <summary>
        /// 是否默认
        /// </summary>
		public bool? IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
		}

        private int? addrType;
        /// <summary>
        /// 地址类型
        /// </summary>
		public int? AddrType
        {
            get { return addrType; }
            set { addrType = value; }
        }

        private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private DateTime? updateTime;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
        }
        #endregion
    }
}
