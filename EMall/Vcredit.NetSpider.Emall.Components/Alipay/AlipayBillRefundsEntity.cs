using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillRefunds")]
    
	public class AlipayBillRefundsEntity
	{
	
		public AlipayBillRefundsEntity() { }


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
      
		private long? billID;
       

        /// <summary>
        /// 账单编号
        /// </summary>
		public long? BillID
		{
			get { return billID; }
			set { billID = value; }
		}
      
		private string numberNo;
       

        /// <summary>
        /// 退款申请号
        /// </summary>
		public string NumberNo
		{
			get { return numberNo; }
			set { numberNo = value; }
		}
      
		private DateTime? refundTime;
       

        /// <summary>
        /// 申请退款时间
        /// </summary>
		public DateTime? RefundTime
		{
			get { return refundTime; }
			set { refundTime = value; }
		}
      
		private string reason;
       

        /// <summary>
        /// 退款理由
        /// </summary>
		public string Reason
		{
			get { return reason; }
			set { reason = value; }
		}
      
		private decimal? amount;
       

        /// <summary>
        /// 退款金额(元)
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}
      
		private string state;
       

        /// <summary>
        /// 
        /// </summary>
		public string State
		{
			get { return state; }
			set { state = value; }
		}
      
		private string otherInfo;
       

        /// <summary>
        /// 其他信息
        /// </summary>
		public string OtherInfo
		{
			get { return otherInfo; }
			set { otherInfo = value; }
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
