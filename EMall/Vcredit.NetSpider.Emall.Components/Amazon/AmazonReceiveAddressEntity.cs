using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_receiveaddress")]
	public class AmazonReceiveAddressEntity
	{
		public AmazonReceiveAddressEntity() { }

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
      
		private string name  ;
        /// <summary>
        /// 收货人姓名
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// 电话
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string tel  ;
        /// <summary>
        /// 固话
        /// </summary>
		public string Tel
		{
			get { return tel; }
			set { tel = value; }
		}
      
		private string province  ;
        /// <summary>
        /// 省
        /// </summary>
		public string Province
		{
			get { return province; }
			set { province = value; }
		}
      
		private string city  ;
        /// <summary>
        /// 市
        /// </summary>
		public string City
		{
			get { return city; }
			set { city = value; }
		}
      
		private string area  ;
        /// <summary>
        /// 区
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string address  ;
        /// <summary>
        /// 地址
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string zip  ;
        /// <summary>
        /// 邮编
        /// </summary>
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
      
		private string deliverMethods  ;
        /// <summary>
        /// 配送方式
        /// </summary>
		public string DeliverMethods
		{
			get { return deliverMethods; }
			set { deliverMethods = value; }
		}
      
		private int? defaultAdressFlag  ;
        /// <summary>
        /// 默认地址标志
        /// </summary>
		public int? DefaultAdressFlag
		{
			get { return defaultAdressFlag; }
			set { defaultAdressFlag = value; }
		}
      
		private string aKeyOrder  ;
        /// <summary>
        /// 一键下单
        /// </summary>
		public string AKeyOrder
		{
			get { return aKeyOrder; }
			set { aKeyOrder = value; }
		}
      
		private string adressOtherName  ;
        /// <summary>
        /// 地址别名
        /// </summary>
		public string AdressOtherName
		{
			get { return adressOtherName; }
			set { adressOtherName = value; }
		}
        private int userId;
        /// <summary>
        /// 客户编号
        /// </summary>
        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        private string accountName;
         
        /// <summary>
        /// 客户账号
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建日期
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
