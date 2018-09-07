using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Gome
{
	[Alias("gome_order")]
	public class GomeOrderEntity
	{
		public GomeOrderEntity() { }

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
        /// 
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// 
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string orderType  ;
        /// <summary>
        /// 
        /// </summary>
		public string OrderType
		{
			get { return orderType; }
			set { orderType = value; }
		}
      
		private DateTime? orderTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}
      
		private string orderStatus  ;
        /// <summary>
        /// 
        /// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}
      
		private string payType  ;
        /// <summary>
        /// 
        /// </summary>
		public string PayType
		{
			get { return payType; }
			set { payType = value; }
		}
      
		private decimal? totalAmount  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private decimal? payAmount  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}
      
		private int? goodsCount  ;
        /// <summary>
        /// 
        /// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
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
      
		private DateTime? updateTime  ;
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
