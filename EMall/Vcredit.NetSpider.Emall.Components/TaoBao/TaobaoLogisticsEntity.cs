using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_logistics")]
	public class TaobaoLogisticsEntity
	{
		public TaobaoLogisticsEntity() { }

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
        /// 基础信息编号
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName = string.Empty;
        /// <summary>
        /// 用户淘宝账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private string orderNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string logisticsNo = string.Empty;
        /// <summary>
        /// 货运单号
        /// </summary>
		public string LogisticsNo
		{
			get { return logisticsNo; }
			set { logisticsNo = value; }
		}
      
		private string logisticsType = string.Empty;
        /// <summary>
        /// 发货方式
        /// </summary>
		public string LogisticsType
		{
			get { return logisticsType; }
			set { logisticsType = value; }
		}
      
		private string logisticsPhone = string.Empty;
        /// <summary>
        /// 承运人电话
        /// </summary>
		public string LogisticsPhone
		{
			get { return logisticsPhone; }
			set { logisticsPhone = value; }
		}
      
		private string logisticsCompany = string.Empty;
        /// <summary>
        /// 承运人(公司)
        /// </summary>
		public string LogisticsCompany
		{
			get { return logisticsCompany; }
			set { logisticsCompany = value; }
		}
      
		private string address = string.Empty;
        /// <summary>
        /// 
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string receiver = string.Empty;
        /// <summary>
        /// 
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string telephone = string.Empty;
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
      
		private string createUser  ;
        /// <summary>
        /// 
        /// </summary>
		public string CreateUser
		{
			get { return createUser; }
			set { createUser = value; }
		}
      
		private DateTime? updateTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
      
		private string updateUser  ;
        /// <summary>
        /// 
        /// </summary>
		public string UpdateUser
		{
			get { return updateUser; }
			set { updateUser = value; }
		}
		#endregion
	}
}
