using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.VipShop
{
	[Alias("vipshop_logistics")]
	public class VipshopLogisticsEntity
	{
		public VipshopLogisticsEntity() { }

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
      
		private string accountName  ;
        /// <summary>
        /// 用户淘宝账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string logisticsNo  ;
        /// <summary>
        /// 货运单号
        /// </summary>
		public string LogisticsNo
		{
			get { return logisticsNo; }
			set { logisticsNo = value; }
		}
      
		private string logisticsType  ;
        /// <summary>
        /// 发货方式
        /// </summary>
		public string LogisticsType
		{
			get { return logisticsType; }
			set { logisticsType = value; }
		}
      
		private string logisticsPhone  ;
        /// <summary>
        /// 承运人电话
        /// </summary>
		public string LogisticsPhone
		{
			get { return logisticsPhone; }
			set { logisticsPhone = value; }
		}
      
		private string logisticsCompany  ;
        /// <summary>
        /// 承运人(公司)
        /// </summary>
		public string LogisticsCompany
		{
			get { return logisticsCompany; }
			set { logisticsCompany = value; }
		}
      
		private string address  ;
        /// <summary>
        /// 
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string receiver  ;
        /// <summary>
        /// 
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string telephone  ;
        /// <summary>
        /// 
        /// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
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
