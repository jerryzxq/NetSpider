using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Alipay_BillFlowerOrderDetail")]
    
	public class AlipayBillFlowerOrderDetailEntity
	{
	
		public AlipayBillFlowerOrderDetailEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long billFlowerID;
       

        /// <summary>
        /// 
        /// </summary>
		public long BillFlowerID
		{
			get { return billFlowerID; }
			set { billFlowerID = value; }
		}
      
		private string productName;
       

        /// <summary>
        /// 
        /// </summary>
		public string ProductName
		{
			get { return productName; }
			set { productName = value; }
		}
      
		private string orderState;
       

        /// <summary>
        /// 
        /// </summary>
		public string OrderState
		{
			get { return orderState; }
			set { orderState = value; }
		}
      
		private decimal? productAmount;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? ProductAmount
		{
			get { return productAmount; }
			set { productAmount = value; }
		}
      
		private decimal? serviceCharge;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? ServiceCharge
		{
			get { return serviceCharge; }
			set { serviceCharge = value; }
		}
      
		private DateTime? orderCreateTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? OrderCreateTime
		{
			get { return orderCreateTime; }
			set { orderCreateTime = value; }
		}
      
		private DateTime createTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion

	}
}
