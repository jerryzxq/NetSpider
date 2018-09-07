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
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string eFuBaoAccountName  ;
        /// <summary>
        /// 易付宝账号
        /// </summary>
		public string EFuBaoAccountName
		{
			get { return eFuBaoAccountName; }
			set { eFuBaoAccountName = value; }
		}
        private string realName;

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName
        {
            get { return realName; }
            set { realName = value; }
        }
		private string iDCard  ;
        /// <summary>
        /// 身份证号
        /// </summary>
		public string IDCard
		{
			get { return iDCard; }
			set { iDCard = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// 手机
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

        private string email;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; }
        } 
      
		private string eGoAccount  ;
        /// <summary>
        /// 易购账号
        /// </summary>
		public string EGoAccount
		{
			get { return eGoAccount; }
			set { eGoAccount = value; }
		}
      
		private string occupation  ;
        /// <summary>
        /// 职业
        /// </summary>
		public string Occupation
		{
			get { return occupation; }
			set { occupation = value; }
		}
      
		private string contactAddress  ;
        /// <summary>
        /// 联系地址
        /// </summary>
		public string ContactAddress
		{
			get { return contactAddress; }
			set { contactAddress = value; }
		}
      
		private string verified  ;
        /// <summary>
        /// 实名认证
        /// </summary>
		public string Verified
		{
			get { return verified; }
			set { verified = value; }
		}
      
		private decimal? eFuBaoBalance  ;
        /// <summary>
        /// 易付宝余额
        /// </summary>
		public decimal? EFuBaoBalance
		{
			get { return eFuBaoBalance; }
			set { eFuBaoBalance = value; }
		}
      
		private decimal? baoChange  ;
        /// <summary>
        /// 零钱包
        /// </summary>
		public decimal? BaoChange
		{
			get { return baoChange; }
			set { baoChange = value; }
		}
      
		private decimal? yeasterdayIncome  ;
        /// <summary>
        /// 昨日收益
        /// </summary>
		public decimal? YeasterdayIncome
		{
			get { return yeasterdayIncome; }
			set { yeasterdayIncome = value; }
		}
      
		private decimal? incomeAmount  ;
        /// <summary>
        /// 累计收益
        /// </summary>
		public decimal? IncomeAmount
		{
			get { return incomeAmount; }
			set { incomeAmount = value; }
		}
      
		private decimal? renXingFuAvailAmount  ;
        /// <summary>
        /// 任性付可用余额
        /// </summary>
		public decimal? RenXingFuAvailAmount
		{
			get { return renXingFuAvailAmount; }
			set { renXingFuAvailAmount = value; }
		}
      
		private decimal? renXingFuUsedAmount  ;
        /// <summary>
        /// 任性付已用金额
        /// </summary>
		public decimal? RenXingFuUsedAmount
		{
			get { return renXingFuUsedAmount; }
			set { renXingFuUsedAmount = value; }
		}
      
		private decimal? renXingFuTotalAmount  ;
        /// <summary>
        /// 任性付总额度
        /// </summary>
		public decimal? RenXingFuTotalAmount
		{
			get { return renXingFuTotalAmount; }
			set { renXingFuTotalAmount = value; }
		}
      
		private decimal? otherAmount  ;
        /// <summary>
        /// 其他账户余额
        /// </summary>
		public decimal? OtherAmount
		{
			get { return otherAmount; }
			set { otherAmount = value; }
		}
      
		private decimal? otherIncome  ;
        /// <summary>
        /// 其他收益
        /// </summary>
		public decimal? OtherIncome
		{
			get { return otherIncome; }
			set { otherIncome = value; }
		}
      
		private int? quickCard  ;
        /// <summary>
        /// 快捷银行卡
        /// </summary>
		public int? QuickCard
		{
			get { return quickCard; }
			set { quickCard = value; }
		}
      
		private int? eFuBaoElecCoupons  ;
        /// <summary>
        /// 易付宝电子券
        /// </summary>
		public int? EFuBaoElecCoupons
		{
			get { return eFuBaoElecCoupons; }
			set { eFuBaoElecCoupons = value; }
		}
      
		private int? renXingFuElecCoupons  ;
        /// <summary>
        /// 任性付电子券
        /// </summary>
		public int? RenXingFuElecCoupons
		{
			get { return renXingFuElecCoupons; }
			set { renXingFuElecCoupons = value; }
		}
      
		private decimal? withdrawalAvaliAmount  ;
        /// <summary>
        /// 可取款余额
        /// </summary>
		public decimal? WithdrawalAvaliAmount
		{
			get { return withdrawalAvaliAmount; }
			set { withdrawalAvaliAmount = value; }
		}
      
		private decimal? withdrawalUsedAmount  ;
        /// <summary>
        /// 已取款余额
        /// </summary>
		public decimal? WithdrawalUsedAmount
		{
			get { return withdrawalUsedAmount; }
			set { withdrawalUsedAmount = value; }
		}
      
		private decimal? withdrawalTotalAmount  ;
        /// <summary>
        /// 总取款额度
        /// </summary>
		public decimal? WithdrawalTotalAmount
		{
			get { return withdrawalTotalAmount; }
			set { withdrawalTotalAmount = value; }
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
        /// 客户登录账号
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
