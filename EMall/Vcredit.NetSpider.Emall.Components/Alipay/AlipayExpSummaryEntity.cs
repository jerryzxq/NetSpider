using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_ExpSummary")]
    
	public class AlipayExpSummaryEntity
	{
	
		public AlipayExpSummaryEntity() { }


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
      
		private decimal? weight;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Weight
		{
			get { return weight; }
			set { weight = value; }
		}
      
		private decimal? amount;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
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
