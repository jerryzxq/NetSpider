using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_Bank")]
    
	public class AlipayBankEntity
	{
	
		public AlipayBankEntity() { }


		#region Attributes
      
		private long? iD;
       
        [AutoIncrement]
        /// <summary>
        /// 主键
        /// </summary>
		public long? ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? baicID;
       

        /// <summary>
        /// 基本信息编号
        /// </summary>
		public long? BaicID
		{
			get { return baicID; }
			set { baicID = value; }
		}
      
		private string bankName;
       

        /// <summary>
        /// 银行名称
        /// </summary>
		public string BankName
		{
			get { return bankName; }
			set { bankName = value; }
		}

        private string name;


        /// <summary>
        /// 银行名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


		private string lastNumber;
       

        /// <summary>
        /// 尾号
        /// </summary>
		public string LastNumber
		{
			get { return lastNumber; }
			set { lastNumber = value; }
		}
      
		private string cardType;
       

        /// <summary>
        /// 类型
        /// </summary>
		public string CardType
		{
			get { return cardType; }
			set { cardType = value; }
		}
      
		private string isPayment;
       
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
