using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class RCResult
    {
        /// <summary>
        /// 月利率
        /// </summary>
        public decimal MonthlyInterestRate { get; set; }
        /// <summary>
        /// 月管理费率
        /// </summary>
        public decimal MonthlyManageRate { get; set; }
        /// <summary>
        /// 手续费率
        /// </summary>
        public decimal FormalitiesRate { get; set; }
        /// <summary>
        /// 审批金额
        /// </summary>
        public decimal ExamineLoanMoney { get; set; }
        /// <summary>
        /// 授信金额
        /// </summary>
        public decimal CreditAmount { get; set; }
        /// <summary>
        /// 月服务费率
        /// </summary>
        public decimal MonthlyServiceRate { get; set; }
        /// <summary>
        /// 月还款额
        /// </summary>
        public decimal MonthlyPmt { get; set; }
        /// <summary>
        /// 月本息还款额
        /// </summary>
        public decimal MonthlyBaseAndInterestPmt { get; set; }
        /// <summary>
        /// 贷款期限
        /// </summary>
        public int LoanPeriod { get; set; }
        /// <summary>
        /// 征信评分
        /// </summary>
        public int CreditScore { get; set; }
        /// <summary>
        /// 手机评分
        /// </summary>
        public int MobileScore { get; set; }
        /// <summary>
        /// 客户分类结果
        /// </summary>
        public string CustomerRating { get; set; }
        /// <summary>
        /// 最高期数
        /// </summary>
        public int HighestPeriods { get; set; }
        public List<RCRule> RejectReasons { get; set; }
       
    }
}
