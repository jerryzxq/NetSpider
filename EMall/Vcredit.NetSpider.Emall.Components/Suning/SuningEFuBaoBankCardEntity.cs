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
        /// ���
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string bankName  ;
        /// <summary>
        /// ����
        /// </summary>
		public string BankName
		{
			get { return bankName; }
			set { bankName = value; }
		}
      
		private string lastNumber  ;
        /// <summary>
        /// β��
        /// </summary>
		public string LastNumber
		{
			get { return lastNumber; }
			set { lastNumber = value; }
		}
      
		private string cardType  ;
        /// <summary>
        /// ������
        /// </summary>
		public string CardType
		{
			get { return cardType; }
			set { cardType = value; }
		}
      
		private DateTime? openingTime  ;
        /// <summary>
        /// ��ͨʱ��
        /// </summary>
		public DateTime? OpeningTime
		{
			get { return openingTime; }
			set { openingTime = value; }
		}
      
		private string currentStatus  ;
        /// <summary>
        /// ��ǰ״̬
        /// </summary>
		public string CurrentStatus
		{
			get { return currentStatus; }
			set { currentStatus = value; }
		}
      
		private long? userId  ;
        /// <summary>
        /// �ͻ����
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// �˻���
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
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
