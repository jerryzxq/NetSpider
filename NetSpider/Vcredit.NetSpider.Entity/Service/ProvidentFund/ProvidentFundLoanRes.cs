﻿using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class ProvidentFundLoanRes
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
        /// 手机号
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 贷款编号/委托书编号
        /// </summary>
        public string Loan_Sid { get; set; }
        /// <summary>
        /// 合同号
        /// </summary>
        public string Con_No { get; set; }
        /// <summary>
        /// 贷款账号
        /// </summary>
        public string Account_Loan { get; set; }
        /// <summary>
        /// 还款账号
        /// </summary>
        public string Account_Repay { get; set; }
        /// <summary>
        /// 公积金账号
        /// </summary>
        public string ProvidentFundNo { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 贷款金额
        /// </summary>
        public decimal Loan_Credit { get; set; }
        /// <summary>
        /// 还款期数(总期数)
        /// </summary>
        public string Period_Total { get; set; }
        /// <summary>
        /// 贷款利率
        /// </summary>
        public string Loan_Rate { get; set; }
        /// <summary>
        /// 罚息率
        /// </summary>
        public string Interest_Penalty_Rate { get; set; }
        /// <summary>
        /// 贷款余额
        /// </summary>
        public decimal Loan_Balance { get; set; }
        /// <summary>
        /// 贷款类型
        /// </summary>
        public string Loan_Type { get; set; }
        /// <summary>
        /// 还款方式
        /// </summary>
        public string Repay_Type { get; set; }
        /// <summary>
        /// 支取还贷类型
        /// </summary>
        public string Withdrawal_Type { get; set; }
        /// <summary>
        /// 委托划转签订日期
        /// </summary>
        public string Delegate_Date { get; set; }
        /// <summary>
        /// 商业贷款账号
        /// </summary>
        public string Account_CommercialLoan { get; set; }
        /// <summary>
        /// 委托银行
        /// </summary>
        public string Bank_Delegate { get; set; }
        /// <summary>
        /// 开户银行
        /// </summary>
        public string Bank_Opening { get; set; }
        /// <summary>
        /// 还款银行
        /// </summary>
        public string Bank_Repay { get; set; }
        /// <summary>
        /// 贷款开户日期
        /// </summary>
        public string Loan_Opening_Date { get; set; }
        /// <summary>
        /// 贷款开始日期
        /// </summary>
        public string Loan_Start_Date { get; set; }
        /// <summary>
        /// 贷款结束日期
        /// </summary>
        public string Loan_End_Date { get; set; }
        /// <summary>
        /// 末次还款时间（最近还款时间）
        /// </summary>
        public string LatestRepayTime { get; set; }
        /// <summary>
        /// 实际终止日期
        /// </summary>
        public string Loan_Actual_End_Date { get; set; }
        /// <summary>
        /// 已还本金
        /// </summary>
        public decimal Principal_Payed { get; set; }
        /// <summary>
        /// 已还款期数
        /// </summary>
        public int Period_Payed { get; set; }
        /// <summary>
        /// 总利息
        /// </summary>
        public decimal Interest_Total { get; set; }
        /// <summary>
        /// 已还利息
        /// </summary>
        public decimal Interest_Payed { get; set; }
        /// <summary>
        /// 逾期期数（网站）
        /// </summary>
        public int Overdue_Period { get; set; }
        /// <summary>
        /// 逾期期数（计算）
        /// </summary>
        public int Overdue_Period_Cal { get; set; }
        /// <summary>
        /// 逾期本金（网站）
        /// </summary>
        public decimal Overdue_Principal { get; set; }
        /// <summary>
        /// 逾期本金（计算）
        /// </summary>
        public decimal Overdue_Principal_Cal { get; set; }
        /// <summary>
        /// 逾期利息（网站）
        /// </summary>
        public decimal Overdue_Interest { get; set; }
        /// <summary>
        /// 逾期利息（计算）
        /// </summary>
        public decimal Overdue_Interest_Cal { get; set; }
        /// <summary>
        /// 应还未还利息
        /// </summary>
        public decimal Interest_UnPayed { get; set; }
        /// <summary>
        /// 罚息
        /// </summary>
        public decimal Interest_Penalty { get; set; }
        /// <summary>
        /// 当期还款日期
        /// </summary>
        public string Current_Repay_Date { get; set; }
        /// <summary>
        /// 当期还款金额
        /// </summary>
        public decimal Current_Repay_Total { get; set; }
        /// <summary>
        /// 当期还款本金
        /// </summary>
        public decimal Current_Repay_Principal { get; set; }
        /// <summary>
        /// 当期还款利息
        /// </summary>
        public decimal Current_Repay_Interest { get; set; }
        /// <summary>
        /// 房贷地址
        /// </summary>
        public string House_Purchase_Address { get; set; }
        /// <summary>
        /// 购房类型
        /// </summary>
        public string House_Purchase_Type { get; set; }
        /// <summary>
        /// 房屋类型
        /// </summary>
        public string House_Type { get; set; }
        /// <summary>
        /// 网站数据更新日期
        /// </summary>
        public string Record_Date { get; set; }
        /// <summary>
        /// 配偶姓名
        /// </summary>
        public string Couple_Name { get; set; }
        /// <summary>
        /// 配偶身份证号
        /// </summary>
        public string Couple_IdentifyCard { get; set; }
        /// <summary>
        /// 配偶手机号
        /// </summary>
        public string Couple_Phone { get; set; }
        /// <summary>
        /// 配偶地址
        /// </summary>
        public string Couple_Address { get; set; }
        /// <summary>
        /// 配偶单位名称
        /// </summary>
        public string Couple_CompanyName { get; set; }
        /// <summary>
        /// 配偶公积金帐号
        /// </summary>
        public string Couple_ProvidentFundNo { get; set; }
        /// <summary>
        /// 配偶月缴额
        /// </summary>
        public decimal Couple_MonthPay { get; set; }
        /// <summary>
        /// 配偶账户状态
        /// </summary>
        public string Couple_Status { get; set; }
        /// <summary>
        /// 配偶账户余额
        /// </summary>
        public decimal Couple_Balance { get; set; }
        /// <summary>
        /// 参与还款人信息
        /// </summary>
        public string JoinPerson { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        private List<ProvidentFundLoanDetail> _ProvidentFundLoanDetail = new List<ProvidentFundLoanDetail>();
        public List<ProvidentFundLoanDetail> ProvidentFundLoanDetailList
        {
            get { return _ProvidentFundLoanDetail; }
            set { this._ProvidentFundLoanDetail = value; }
        }
    }
}