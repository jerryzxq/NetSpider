using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillCreditPayment")]
    
	public class AlipayBillCreditPaymentEntity
	{
	
		public AlipayBillCreditPaymentEntity() { }


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
      
		private string billNO;
       

        /// <summary>
        /// 流水号 
        /// </summary>
		public string BillNO
		{
			get { return billNO; }
			set { billNO = value; }
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
      
		private string accounts;
       

        /// <summary>
        /// 帐号
        /// </summary>
		public string Accounts
		{
			get { return accounts; }
			set { accounts = value; }
		}
      
		private decimal? repaymentAmount;
       

        /// <summary>
        /// 还款金额
        /// </summary>
		public decimal? RepaymentAmount
		{
			get { return repaymentAmount; }
			set { repaymentAmount = value; }
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
      
		private decimal? actualPaymentAmount;
       

        /// <summary>
        /// 实付金额
        /// </summary>
		public decimal? ActualPaymentAmount
		{
			get { return actualPaymentAmount; }
			set { actualPaymentAmount = value; }
		}
      
		private DateTime? repaymentTime;
       

        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? RepaymentTime
		{
			get { return repaymentTime; }
			set { repaymentTime = value; }
		}
      
		private DateTime? successTime;
       

        /// <summary>
        /// 还款成功时间
        /// </summary>
		public DateTime? SuccessTime
		{
			get { return successTime; }
			set { successTime = value; }
		}
      
		private DateTime? filingTime;
       

        /// <summary>
        /// 还款申请提交时间
        /// </summary>
		public DateTime? FilingTime
		{
			get { return filingTime; }
			set { filingTime = value; }
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
