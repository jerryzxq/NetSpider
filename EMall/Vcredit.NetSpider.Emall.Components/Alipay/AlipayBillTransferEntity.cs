using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillTransfer")]
    
	public class AlipayBillTransferEntity
	{
	
		public AlipayBillTransferEntity() { }


		#region Attributes
      
		private long iD;
       
        [AutoIncrement]
        /// <summary>
        /// 主键
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? billID;
       

        /// <summary>
        /// 账单编号
        /// </summary>
		public long? BillID
		{
			get { return billID; }
			set { billID = value; }
		}
      
		private string name;
       

        /// <summary>
        /// 消费名称
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private decimal? actualPaymentAmount;
       

        /// <summary>
        /// 对方实收
        /// </summary>
		public decimal? ActualPaymentAmount
		{
			get { return actualPaymentAmount; }
			set { actualPaymentAmount = value; }
		}
      
		private decimal? serviceCharge;
       

        /// <summary>
        /// 服务费
        /// </summary>
		public decimal? ServiceCharge
		{
			get { return serviceCharge; }
			set { serviceCharge = value; }
		}
      
		private decimal? amount;
       

        /// <summary>
        /// 金额
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}
      
		private string otherInfo;
       

        /// <summary>
        /// 对方信息
        /// </summary>
		public string OtherInfo
		{
			get { return otherInfo; }
			set { otherInfo = value; }
		}
      
		private DateTime? paymentTime;
       

        /// <summary>
        /// 转账创建时间
        /// </summary>
		public DateTime? PaymentTime
		{
			get { return paymentTime; }
			set { paymentTime = value; }
		}
      
		private string state;
       

        /// <summary>
        /// 状态
        /// </summary>
		public string State
		{
			get { return state; }
			set { state = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
            get
            {

                if (createTime == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return createTime;
                }
            }
			set { createTime = value; }
		}
		#endregion

	}
}
