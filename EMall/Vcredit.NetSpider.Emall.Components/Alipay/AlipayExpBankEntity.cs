using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_ExpBank")]
    
	public class AlipayExpBankEntity
	{
	
		public AlipayExpBankEntity() { }


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
      
		private long? electronicBillID;
       

        /// <summary>
        /// 
        /// </summary>
		public long? ElectronicBillID
		{
			get { return electronicBillID; }
			set { electronicBillID = value; }
		}
      
		private string name;
       

        /// <summary>
        /// 
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private decimal? inflow;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Inflow
		{
			get { return inflow; }
			set { inflow = value; }
		}
      
		private decimal? flowOut;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? FlowOut
		{
			get { return flowOut; }
			set { flowOut = value; }
		}
      
		private decimal? repayment;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Repayment
		{
			get { return repayment; }
			set { repayment = value; }
		}
      
		private decimal? expenditure;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Expenditure
		{
			get { return expenditure; }
			set { expenditure = value; }
		}
      
		private decimal? withdrawals;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Withdrawals
		{
			get { return withdrawals; }
			set { withdrawals = value; }
		}
      
		private decimal? recharge;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Recharge
		{
			get { return recharge; }
			set { recharge = value; }
		}
      
		private decimal? refund;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Refund
		{
			get { return refund; }
			set { refund = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 
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
