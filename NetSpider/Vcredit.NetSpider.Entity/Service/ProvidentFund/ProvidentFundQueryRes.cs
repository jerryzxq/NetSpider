using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class ProvidentFundQueryRes : BaseRes
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 雇员编号
        /// </summary>
        public string EmployeeNo { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }
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
        /// 是否本地
        /// </summary>
        public bool? IsLocal { get; set; }
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
        /// 账号
        /// </summary>
        public string Loginname { get; set; }
        /// <summary>
        /// 社保评分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 普通公积金明细
        /// </summary>
        private List<ProvidentFundDetail> _ProvidentFundDetailList = new List<ProvidentFundDetail>();
        public List<ProvidentFundDetail> ProvidentFundDetailList
        {
            get { return _ProvidentFundDetailList; }
            set { this._ProvidentFundDetailList = value; }
        }
        /// <summary>
        /// 补充公积金
        /// </summary>
        private ProvidentFundReserveRes _ProvidentFundReserveRes = new ProvidentFundReserveRes();
        public ProvidentFundReserveRes ProvidentFundReserveRes
        {
            get { return _ProvidentFundReserveRes; }
            set { this._ProvidentFundReserveRes = value; }
        }
        /// <summary>
        /// 贷款
        /// </summary>
        private ProvidentFundLoanRes _ProvidentFundLoanRes = new ProvidentFundLoanRes();
        public ProvidentFundLoanRes ProvidentFundLoanRes 
        {
            get { return _ProvidentFundLoanRes; }
            set { this._ProvidentFundLoanRes = value; }
        }
    }
}