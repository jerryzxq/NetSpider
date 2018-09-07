using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region SocialSecurityDetailEntity

	/// <summary>
	/// SocialSecurityDetailEntity object for NHibernate mapped table 'SocialSecurityDetail'.
	/// </summary>
	public class SocialSecurityDetailEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 社保信息表ID
		 /// </summary>
		public virtual int ? SocialSecurityId{get; set;}
		 /// <summary>
		 /// 姓名
		 /// </summary>
		public virtual string Name{get; set;}
		 /// <summary>
		 /// 身份证号
		 /// </summary>
		public virtual string IdentityCard{get; set;}
		 /// <summary>
		 /// 单位名称
		 /// </summary>
		public virtual string CompanyName{get; set;}
		 /// <summary>
		 /// 缴费类型
		 /// </summary>
		public virtual string PaymentType{get; set;}
		 /// <summary>
		 /// 缴费标志
		 /// </summary>
		public virtual string PaymentFlag{get; set;}
		 /// <summary>
		 /// 缴费年月
		 /// </summary>
		public virtual string PayTime{get; set;}
		 /// <summary>
		 /// 应属年月
		 /// </summary>
		public virtual string SocialInsuranceTime{get; set;}
		 /// <summary>
		 /// 缴费基数
		 /// </summary>
		public virtual decimal ? SocialInsuranceBase{get; set;}
		 /// <summary>
		 /// 个人缴费（养老）
		 /// </summary>
		public virtual decimal ? PensionAmount{get; set;}
		 /// <summary>
		 /// 单位划入帐户
		 /// </summary>
		public virtual decimal ? CompanyPensionAmount{get; set;}
		 /// <summary>
		 /// 社平划入
		 /// </summary>
		public virtual decimal ? NationPensionAmount{get; set;}
		 /// <summary>
		 /// 个人缴费(医保)
		 /// </summary>
		public virtual decimal ? MedicalAmount{get; set;}
		 /// <summary>
		 /// 单位划入个人账户额（医保）
		 /// </summary>
		public virtual decimal ? CompanyMedicalAmount{get; set;}
		 /// <summary>
		 /// 大额救助
		 /// </summary>
		public virtual decimal ? IllnessMedicalAmount{get; set;}
		 /// <summary>
		 /// 共划入账户
		 /// </summary>
		public virtual decimal ? EnterAccountMedicalAmount{get; set;}
		 /// <summary>
		 /// 公务员补助账户
		 /// </summary>
		public virtual decimal ? CivilServantMedicalAmount{get; set;}
		 /// <summary>
		 /// 失业缴费
		 /// </summary>
		public virtual decimal ? UnemployAmount{get; set;}
        /// <summary>
        /// 生育保险缴费
        /// </summary>
        public virtual decimal ? MaternityAmount { get; set; }
        /// <summary>
        /// 工伤保险缴费
        /// </summary>
        public virtual decimal ? EmploymentInjuryAmount { get; set; }
		 /// <summary>
		 /// 年缴费月数
		 /// </summary>
		public virtual int ? YearPaymentMonths{get; set;}
		 /// <summary>
		 /// 社保总金额
		 /// </summary>
		public virtual decimal ? SocialInsuranceTotalAmount{get; set;}
		 /// <summary>
		 /// 个人账户社保金额
		 /// </summary>
		public virtual decimal ? PersonalTotalAmount{get; set;}
	}
	#endregion
}