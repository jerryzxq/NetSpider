using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class ProvidentFundReserveRes
    {
        /// <summary>
        /// 基本薪资
        /// </summary>
        public decimal SalaryBase { get; set; }
        /// <summary>
        /// 个人月缴费
        /// </summary>
        public decimal PersonalMonthPayAmount { get; set; }
        /// <summary>
        /// 公司月缴费
        /// </summary>
        public decimal CompanyMonthPayAmount { get; set; }
        /// <summary>
        /// 个人缴费比率
        /// </summary>
        public decimal PersonalMonthPayRate { get; set; }
        /// <summary>
        /// 公司缴费比率
        /// </summary>
        public decimal CompanyMonthPayRate { get; set; }
        /// <summary>
        /// 账户总额
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 账户状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 开户时间
        /// </summary>
        public string OpenTime { get; set; }
        /// <summary>
        /// 公司编号
        /// </summary>
        public string CompanyNo { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司工商登记号
        /// </summary>
        public string CompanyLicense { get; set; }
        /// <summary>
        /// 公司地址
        /// </summary>
        public string CompanyAddress { get; set; }
        /// <summary>
        /// 公司所在地区
        /// </summary>
        public string CompanyDistrict { get; set; }
        /// <summary>
        /// 公司创建时间
        /// </summary>
        public string CompanyOpenTime { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public string BankCardNo { get; set; }
        /// <summary>
        /// 开户银行
        /// </summary>
        public string Bank { get; set; }
        /// <summary>
        /// 银行卡开户时间
        /// </summary>
        public string BankCardOpenTime { get; set; }
        /// <summary>
        /// 公积金所在城市
        /// </summary>
        public string ProvidentFundCity { get; set; }
        /// <summary>
        /// 公积金账号
        /// </summary>
        public string ProvidentFundNo { get; set; }
        /// <summary>
        /// 缴费总月数
        /// </summary>
        public int PaymentMonths { get; set; }
        /// <summary>
        /// 24个月连续缴费月数
        /// </summary>
        public int PaymentMonths_Continuous { get; set; }
        /// <summary>
        /// 最后缴费时间
        /// </summary>
        public string LastProvidentFundTime { get; set; }
        /// <summary>
        /// 24个月连续缴费状态
        /// </summary>
        public string Payment_State { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 补充公积金明细
        /// </summary>
        private List<ProvidentFundDetail> _ProvicentReserveFundDetailList = new List<ProvidentFundDetail>();
        public List<ProvidentFundDetail> ProvidentReserveFundDetailList
        {
            get { return _ProvicentReserveFundDetailList; }
            set { this._ProvicentReserveFundDetailList = value; }
        }
    }
}