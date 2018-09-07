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
        /// ���
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? tradeID  ;
        /// <summary>
        /// �����˺�
        /// </summary>
		public long? TradeID
		{
			get { return tradeID; }
			set { tradeID = value; }
		}
      
		private string processName  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string ProcessName
		{
			get { return processName; }
			set { processName = value; }
		}

        private DateTime? processTime;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? ProcessTime
		{
			get { return processTime; }
			set { processTime = value; }
		}
      
		private decimal? amount  ;
        /// <summary>
        /// ���
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
