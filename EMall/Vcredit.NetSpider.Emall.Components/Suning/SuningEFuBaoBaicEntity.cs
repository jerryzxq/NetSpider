using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaobaic")]
	public class SuningEFuBaoBaicEntity
	{
		public SuningEFuBaoBaicEntity() { }

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
      
		private string eFuBaoAccountName  ;
        /// <summary>
        /// �׸����˺�
        /// </summary>
		public string EFuBaoAccountName
		{
			get { return eFuBaoAccountName; }
			set { eFuBaoAccountName = value; }
		}
        private string realName;

        /// <summary>
        /// ��ʵ����
        /// </summary>
        public string RealName
        {
            get { return realName; }
            set { realName = value; }
        }
		private string iDCard  ;
        /// <summary>
        /// ���֤��
        /// </summary>
		public string IDCard
		{
			get { return iDCard; }
			set { iDCard = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// �ֻ�
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

        private string email;

        /// <summary>
        /// ����
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; }
        } 
      
		private string eGoAccount  ;
        /// <summary>
        /// �׹��˺�
        /// </summary>
		public string EGoAccount
		{
			get { return eGoAccount; }
			set { eGoAccount = value; }
		}
      
		private string occupation  ;
        /// <summary>
        /// ְҵ
        /// </summary>
		public string Occupation
		{
			get { return occupation; }
			set { occupation = value; }
		}
      
		private string contactAddress  ;
        /// <summary>
        /// ��ϵ��ַ
        /// </summary>
		public string ContactAddress
		{
			get { return contactAddress; }
			set { contactAddress = value; }
		}
      
		private string verified  ;
        /// <summary>
        /// ʵ����֤
        /// </summary>
		public string Verified
		{
			get { return verified; }
			set { verified = value; }
		}
      
		private decimal? eFuBaoBalance  ;
        /// <summary>
        /// �׸������
        /// </summary>
		public decimal? EFuBaoBalance
		{
			get { return eFuBaoBalance; }
			set { eFuBaoBalance = value; }
		}
      
		private decimal? baoChange  ;
        /// <summary>
        /// ��Ǯ��
        /// </summary>
		public decimal? BaoChange
		{
			get { return baoChange; }
			set { baoChange = value; }
		}
      
		private decimal? yeasterdayIncome  ;
        /// <summary>
        /// ��������
        /// </summary>
		public decimal? YeasterdayIncome
		{
			get { return yeasterdayIncome; }
			set { yeasterdayIncome = value; }
		}
      
		private decimal? incomeAmount  ;
        /// <summary>
        /// �ۼ�����
        /// </summary>
		public decimal? IncomeAmount
		{
			get { return incomeAmount; }
			set { incomeAmount = value; }
		}
      
		private decimal? renXingFuAvailAmount  ;
        /// <summary>
        /// ���Ը��������
        /// </summary>
		public decimal? RenXingFuAvailAmount
		{
			get { return renXingFuAvailAmount; }
			set { renXingFuAvailAmount = value; }
		}
      
		private decimal? renXingFuUsedAmount  ;
        /// <summary>
        /// ���Ը����ý��
        /// </summary>
		public decimal? RenXingFuUsedAmount
		{
			get { return renXingFuUsedAmount; }
			set { renXingFuUsedAmount = value; }
		}
      
		private decimal? renXingFuTotalAmount  ;
        /// <summary>
        /// ���Ը��ܶ��
        /// </summary>
		public decimal? RenXingFuTotalAmount
		{
			get { return renXingFuTotalAmount; }
			set { renXingFuTotalAmount = value; }
		}
      
		private decimal? otherAmount  ;
        /// <summary>
        /// �����˻����
        /// </summary>
		public decimal? OtherAmount
		{
			get { return otherAmount; }
			set { otherAmount = value; }
		}
      
		private decimal? otherIncome  ;
        /// <summary>
        /// ��������
        /// </summary>
		public decimal? OtherIncome
		{
			get { return otherIncome; }
			set { otherIncome = value; }
		}
      
		private int? quickCard  ;
        /// <summary>
        /// ������п�
        /// </summary>
		public int? QuickCard
		{
			get { return quickCard; }
			set { quickCard = value; }
		}
      
		private int? eFuBaoElecCoupons  ;
        /// <summary>
        /// �׸�������ȯ
        /// </summary>
		public int? EFuBaoElecCoupons
		{
			get { return eFuBaoElecCoupons; }
			set { eFuBaoElecCoupons = value; }
		}
      
		private int? renXingFuElecCoupons  ;
        /// <summary>
        /// ���Ը�����ȯ
        /// </summary>
		public int? RenXingFuElecCoupons
		{
			get { return renXingFuElecCoupons; }
			set { renXingFuElecCoupons = value; }
		}
      
		private decimal? withdrawalAvaliAmount  ;
        /// <summary>
        /// ��ȡ�����
        /// </summary>
		public decimal? WithdrawalAvaliAmount
		{
			get { return withdrawalAvaliAmount; }
			set { withdrawalAvaliAmount = value; }
		}
      
		private decimal? withdrawalUsedAmount  ;
        /// <summary>
        /// ��ȡ�����
        /// </summary>
		public decimal? WithdrawalUsedAmount
		{
			get { return withdrawalUsedAmount; }
			set { withdrawalUsedAmount = value; }
		}
      
		private decimal? withdrawalTotalAmount  ;
        /// <summary>
        /// ��ȡ����
        /// </summary>
		public decimal? WithdrawalTotalAmount
		{
			get { return withdrawalTotalAmount; }
			set { withdrawalTotalAmount = value; }
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
        /// �ͻ���¼�˺�
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
