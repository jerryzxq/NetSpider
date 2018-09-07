using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region SocialSecurityEntity

    /// <summary>
    /// SocialSecurityEntity object for NHibernate mapped table 'SocialSecurity'.
    /// </summary>
    public class SocialSecurityEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public virtual string IdentityCard { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public virtual string Sex { get; set; }
        /// <summary>
        /// 参加工作日期
        /// </summary>
        public virtual string WorkDate { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public virtual string CompanyName { get; set; }
        /// <summary>
        /// 单位编号
        /// </summary>
        public virtual string CompanyNo { get; set; }
        /// <summary>
        /// 行政区划
        /// </summary>
        public virtual string District { get; set; }
        /// <summary>
        /// 单位类型
        /// </summary>
        public virtual string CompanyType { get; set; }
        /// <summary>
        /// 单位状态
        /// </summary>
        public virtual string CompanyStatus { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public virtual string BirthDate { get; set; }
        /// <summary>
        /// 人员状态
        /// </summary>
        public virtual string EmployeeStatus { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public virtual string Race { get; set; }
        /// <summary>
        /// 特殊工种
        /// </summary>
        public virtual bool? IsSpecialWork { get; set; }
        /// <summary>
        /// 退休类别
        /// </summary>
        public virtual string RetireType { get; set; }
        /// <summary>
        /// 养老统筹级别
        /// </summary>
        public virtual string PensionLevel { get; set; }
        /// <summary>
        /// 保健类型
        /// </summary>
        public virtual string HealthType { get; set; }
        /// <summary>
        /// 特殊缴费类型
        /// </summary>
        public virtual string SpecialPaymentType { get; set; }
        /// <summary>
        /// 发卡银行
        /// </summary>
        public virtual string Bank { get; set; }
        /// <summary>
        /// 银行地址
        /// </summary>
        public virtual string BankAddress { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public virtual string Phone { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public virtual string ZipCode { get; set; }
        /// <summary>
        /// 通讯地址
        /// </summary>
        public virtual string Address { get; set; }
        /// <summary>
        /// 养老保险视同缴费月数
        /// </summary>
        public virtual int? PaymentMonths { get; set; }
        /// <summary>
        /// 养老建账户前缴费月数
        /// </summary>
        public virtual int? OldPaymentMonths { get; set; }
        /// <summary>
        /// 24个月最近连续缴费月数
        /// </summary>
        public virtual int? PaymentMonths_Continuous { get; set; }
        /// <summary>
        /// 截止年月
        /// </summary>
        public virtual string DeadlineYearAndMonth { get; set; }
        /// <summary>
        /// 养老缴费基数
        /// </summary>
        public virtual decimal? SocialInsuranceBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? InsuranceTotal { get; set; }
        /// <summary>
        /// 工龄
        /// </summary>
        public virtual string WorkingAge { get; set; }
        /// <summary>
        /// 个人账户总额
        /// </summary>
        public virtual decimal? PersonalInsuranceTotal { get; set; }
        /// <summary>
        /// 员工编号
        /// </summary>
        public virtual string EmployeeNo { get; set; }
        /// <summary>
        /// 24个月连续缴费状态
        /// </summary>
        public virtual string Payment_State { get; set; }
        /// <summary>
        /// 是否本地
        /// </summary>
        public virtual bool IsLocal { get; set; }
        /// <summary>
        /// 社保缴费城市
        /// </summary>
        public virtual string SocialSecurityCity { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string Description { get; set; }

        private DateTime _CreateTime = DateTime.Now;
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? Score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Loginname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public virtual int? Bid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusIdentityCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusId { get; set; }

        public virtual IList<SocialSecurityDetailEntity> Details { get; set; }
    }
    #endregion
}