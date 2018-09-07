using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_ReceiveAddress")]
    //[Schema("dbo")]
	public class SuningReceiveAddressEntity
	{
	
		public SuningReceiveAddressEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 编号
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}

     private long userId;
       

        /// <summary>
        /// 基本信息编号
        /// </summary>
     public long UserId
		{
            get { return userId; }
            set { userId = value; }
		}

        private string receiver;
       

        /// <summary>
        /// 姓名
        /// </summary>
        public string Receiver
		{
            get { return receiver; }
            set { receiver = value; }
		}
      
		private string mobile;
       

        /// <summary>
        /// 手机  
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

        private string telephone;
       

        /// <summary>
        /// 固定电话
        /// </summary>
        public string Telephone
		{
            get { return telephone; }
            set { telephone = value; }
		}
      
		private string province;
       

        /// <summary>
        /// 省
        /// </summary>
		public string Province
		{
			get { return province; }
			set { province = value; }
		}
      
		private string city;
       

        /// <summary>
        /// 市
        /// </summary>
		public string City
		{
			get { return city; }
			set { city = value; }
		}
      
		private string area;
       

        /// <summary>
        /// 区
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string address;
       

        /// <summary>
        /// 地址
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
        private int? defaultAdressFlag;

        /// <summary>
        /// 默认收货地址
        /// </summary>
        public int? DefaultAdressFlag
        {
            get { return defaultAdressFlag; }
            set { defaultAdressFlag = value; }
        }

        private string acountName;

        /// <summary>
        /// 账户名
        /// </summary>
        public string AcountName
        {
            get { return acountName; }
            set { acountName = value; }
        }

        private DateTime? createTime;
         
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
