using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_Consumption")]
    
	public class AlipayConsumptionEntity
	{
	
		public AlipayConsumptionEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 编号
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? electronicBillID;
       

        /// <summary>
        /// 电子账单编号
        /// </summary>
		public long? ElectronicBillID
		{
			get { return electronicBillID; }
			set { electronicBillID = value; }
		}
      
		private DateTime? startTime;
       

        /// <summary>
        /// 月份
        /// </summary>
		public DateTime? StartTime
		{
			get { return startTime; }
			set { startTime = value; }
		}
      
		private DateTime? endTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? EndTime
		{
			get { return endTime; }
			set { endTime = value; }
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
      
		private string currencyCode;
       

        /// <summary>
        /// 
        /// </summary>
		public string CurrencyCode
		{
			get { return currencyCode; }
			set { currencyCode = value; }
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
