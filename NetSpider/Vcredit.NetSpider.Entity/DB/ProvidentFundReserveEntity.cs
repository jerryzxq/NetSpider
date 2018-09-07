using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region ProvidentFundReserveEntity
    /// <summary>
    /// ProvidentFundReserveEntity object for NHibernate mapped table 'ProvidentFundReserve'.
    /// </summary>
    public class ProvidentFundReserveEntity
    {
        public virtual long Id { get; set; }
        /// <summary>
        /// 主表ID
        /// </summary>
        public virtual long? ProvidentFundId { get; set; }
        /// <summary>
        /// 基本薪资
        /// </summary>
        public virtual decimal? SalaryBase { get; set; }
        /// <summary>
        /// 个人月缴费
        /// </summary>
        public virtual decimal? PersonalMonthPayAmount { get; set; }
        /// <summary>
        /// 公司月缴费
        /// </summary>
        public virtual decimal? CompanyMonthPayAmount { get; set; }
        /// <summary>
        /// 个人缴费比率
        /// </summary>
        public virtual decimal? PersonalMonthPayRate { get; set; }
        /// <summary>
        /// 公司缴费比率
        /// </summary>
        public virtual decimal? CompanyMonthPayRate { get; set; }
        /// <summary>
        /// 账户总额
        /// </summary>
        public virtual decimal? TotalAmount { get; set; }
        /// <summary>
        /// 账户状态
        /// </summary>
        public virtual string Status { get; set; }
        /// <summary>
        /// 开户时间
        /// </summary>
        public virtual DateTime? OpenTime { get; set; }
        /// <summary>
        /// 公司编号
        /// </summary>
        public virtual string CompanyNo { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public virtual string CompanyName { get; set; }
        /// <summary>
        /// 公司工商登记号
        /// </summary>
        public virtual string CompanyLicense { get; set; }
        /// <summary>
        /// 公司地址
        /// </summary>
        public virtual string CompanyAddress { get; set; }
        /// <summary>
        /// 公司所在地区
        /// </summary>
        public virtual string CompanyDistrict { get; set; }
        /// <summary>
        /// 公司创建时间
        /// </summary>
        public virtual DateTime? CompanyOpenTime { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public virtual string BankCardNo { get; set; }
        /// <summary>
        /// 开户银行
        /// </summary>
        public virtual string Bank { get; set; }
        /// <summary>
        /// 银行卡开户时间
        /// </summary>
        public virtual DateTime? BankCardOpenTime { get; set; }
        /// <summary>
        /// 公积金所在城市
        /// </summary>
        public virtual string ProvidentFundCity { get; set; }
        /// <summary>
        /// 公积金账号
        /// </summary>
        public virtual string ProvidentFundNo { get; set; }
        /// <summary>
        /// 缴费总月数
        /// </summary>
        public virtual int? PaymentMonths { get; set; }
        /// <summary>
        /// 24个月连续缴费月数
        /// </summary>
        public virtual int? PaymentMonths_Continuous { get; set; }
        /// <summary>
        /// 最后缴费时间
        /// </summary>
        public virtual string LastProvidentFundTime { get; set; }
        /// <summary>
        /// 24个月连续缴费状态
        /// </summary>
        public virtual string Payment_State { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string Description { get; set; }
        /// <summary>
        /// 补充公积金明细
        /// </summary>
        public virtual IList<ProvidentFundReserveDetailEntity> ProvidentReserveFundDetailList { get; set; }
    }
    #endregion
}
