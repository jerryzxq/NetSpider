using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region ProvidentFundEntity

    /// <summary>
    /// ProvidentFundEntity object for NHibernate mapped table 'ProvidentFund'.
    /// </summary>
    public class ProvidentFundEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string IdentityCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string EmployeeNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Sex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? SalaryBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? PersonalMonthPayAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? CompanyMonthPayAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? PersonalMonthPayRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? CompanyMonthPayRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? TotalAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? OpenTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CompanyNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CompanyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CompanyLicense { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CompanyAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string CompanyDistrict { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? CompanyOpenTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BankCardNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Bank { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? BankCardOpenTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string ProvidentFundCity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool? IsLocal { get; set; }

        private DateTime _CreateTime = DateTime.Now;
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        public virtual string ProvidentFundNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? PaymentMonths { get; set; }
        /// <summary>
        /// 24个月最近连续缴费月数
        /// </summary>
        public virtual int? PaymentMonths_Continuous { get; set; }
        /// <summary>
        /// 24个月连续缴费状态
        /// </summary>
        public virtual string Payment_State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? Score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public virtual int? Bid { get; set; }
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
        public virtual string LastProvidentFundTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Description { get; set; }
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
        public virtual IList<ProvidentFundDetailEntity> ProvidentFundDetailList { get; set; }
        public virtual ProvidentFundLoanEntity ProvidentFundLoanRes { get; set; }
        public virtual ProvidentFundReserveEntity ProvidentFundReserveRes { get; set; }
    }
    #endregion
}