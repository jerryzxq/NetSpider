using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaotradeprocess")]
	public class SuningEFuBaoTradeProcessEntity
	{
		public SuningEFuBaoTradeProcessEntity() { }

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
      
		private long? tradeID  ;
        /// <summary>
        /// 交易账号
        /// </summary>
		public long? TradeID
		{
			get { return tradeID; }
			set { tradeID = value; }
		}
      
		private string processName  ;
        /// <summary>
        /// 处理名称
        /// </summary>
		public string ProcessName
		{
			get { return processName; }
			set { processName = value; }
		}

        private DateTime? processTime;
        /// <summary>
        /// 处理时间
        /// </summary>
		public DateTime? ProcessTime
		{
			get { return processTime; }
			set { processTime = value; }
		}
      
		private decimal? amount  ;
        /// <summary>
        /// 金额
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
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
