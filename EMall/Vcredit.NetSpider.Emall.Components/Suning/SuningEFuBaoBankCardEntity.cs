using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaobankcard")]
	public class SuningEFuBaoBankCardEntity
	{
		public SuningEFuBaoBankCardEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string bankName  ;
        /// <summary>
        /// 卡名
        /// </summary>
		public string BankName
		{
			get { return bankName; }
			set { bankName = value; }
		}
      
		private string lastNumber  ;
        /// <summary>
        /// 尾号
        /// </summary>
		public string LastNumber
		{
			get { return lastNumber; }
			set { lastNumber = value; }
		}
      
		private string cardType  ;
        /// <summary>
        /// 卡类型
        /// </summary>
		public string CardType
		{
			get { return cardType; }
			set { cardType = value; }
		}
      
		private DateTime? openingTime  ;
        /// <summary>
        /// 开通时间
        /// </summary>
		public DateTime? OpeningTime
		{
			get { return openingTime; }
			set { openingTime = value; }
		}
      
		private string currentStatus  ;
        /// <summary>
        /// 当前状态
        /// </summary>
		public string CurrentStatus
		{
			get { return currentStatus; }
			set { currentStatus = value; }
		}
      
		private long? userId  ;
        /// <summary>
        /// 客户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// 账户名
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
