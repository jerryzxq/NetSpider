using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Alipay_BillFlowerRePayment")]
    
	public class AlipayBillFlowerRePaymentEntity
	{
	
		public AlipayBillFlowerRePaymentEntity() { }


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
      
		private string rePaymentName;
       

        /// <summary>
        /// 
        /// </summary>
		public string RePaymentName
		{
			get { return rePaymentName; }
			set { rePaymentName = value; }
		}
      
		private decimal? rePaymentAmount;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? RePaymentAmount
		{
			get { return rePaymentAmount; }
			set { rePaymentAmount = value; }
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
      
		private DateTime? rePaymentCreateTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? RePaymentCreateTime
		{
			get { return rePaymentCreateTime; }
			set { rePaymentCreateTime = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion

	}
}
