using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class ContractEntity
    {
        /// <summary>
        /// 甲方（借款人）姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 微信Open lD
        /// </summary>
        public string WeChatId { get; set; }
        /// <summary>
        /// 乙方（贷款人）或公司
        /// </summary>
        public string Serve { get; set; }
        /// <summary>
        /// 乙方住所
        /// </summary>
        public string ServeAddress { get; set; }
        /// <summary>
        /// 城市(上海市)
        /// </summary>
        public string ServeCity { get; set; }
        /// <summary>
        /// 城市(上海市)
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 借款金额
        /// </summary>
        public string LoanAmount { get; set; }
        /// <summary>
        /// 还款金额
        /// </summary>
        public String ApplyAmount { get; set; }
        /// <summary>
        /// 借款期限
        /// </summary>
        public string LoanPeriod { get; set; }
        /// <summary>
        /// 借款天数
        /// </summary>
        public string ApproveDay { get; set; }
        /// <summary>
        /// 期限类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 每月还款金额
        /// </summary>
        public string MonthlyRepay { get; set; }
        /// <summary>
        /// 每月本息还款金额
        /// </summary>
        public string MonthlyBaseAndInterestPmt { get; set; }
        /// <summary>
        /// 年化利率
        /// </summary>
        public string YearInterestRate { get; set; }
        /// <summary>
        /// 月服务费率
        /// </summary>
        public string MonthlyServiceRate { get; set; }
        /// <summary>
        /// 手续费率
        /// </summary>
        public string FormalitiesRate { get; set; }
        /// <summary>
        /// 甲方提供信用卡开户行
        /// </summary>
        public string CreditCardBankName { get; set; }
        /// <summary>
        /// 甲方提供接受的信用卡号
        /// </summary>
        public string CreditCardNo { get; set; }
        /// <summary>
        /// 甲方提供还款的开户行
        /// </summary>
        public string CashCardBankName { get; set; }
        /// <summary>
        /// 甲方提供还款的卡号
        /// </summary>
        public string CashCardNo { get; set; }
        /// <summary>
        /// 变更开户行开户行
        /// </summary>
        public string ChangeCardBankName { get; set; }
        /// <summary>
        /// 变更银行的卡号
        /// </summary>
        public string ChangeCardNo { get; set; }
        /// <summary>
        /// 甲方住所人民法院名称,上海市静安区人民法院
        /// </summary>
        public string ContractRegionCourt { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 微信订单号
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// VBS业务号
        /// </summary>
        public int Bid { get; set; }
        /// <summary>
        /// 合同类别
        /// </summary>
        public string ContractType { get; set; }
        /// <summary>
        /// 合同编号
        /// </summary>
        public string ContractNo { get; set; }
        /// <summary>
        /// 个人借款服务与咨询合同查询时间
        /// </summary>
        public DateTime ConsultingServiceTime { get; set; }
        /// <summary>
        /// 个人客户扣款授权书查询时间
        /// </summary>
        public DateTime DeductTime { get; set; }
        /// <summary>
        /// 个人信用贷款合同查询时间
        /// </summary>
        public DateTime LoanTime { get; set; }
        /// <summary>
        /// 特别提示函查询时间
        /// </summary>
        public DateTime PromptTime { get; set; }
        /// <summary>
        /// 服务协议查询时间
        /// </summary>
        public DateTime ServiceTime { get; set; }

        /// <summary>
        /// 贷款用途声明时间
        /// </summary>
        public DateTime LoanUseConfirmTime { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// 签约时间
        /// </summary>
        public DateTime SignTime { get; set; }

        /// <summary>
        /// 额度审批时间
        /// </summary>
        public string ExamineTime { get; set; }
        /// <summary>
        /// 额度审批金额
        /// </summary>
        public string ExamineLoanMoney { get; set; }
        /// <summary>
        /// 是否本地籍
        /// </summary>
        public string IsLocal { get; set; }
        /// <summary>
        /// 申请交单时间
        /// </summary>
        public string ApplyTime { get; set; }
        /// <summary>
        /// 收入
        /// </summary>
        public string IncomeMonth { get; set; }
        

        /// <summary>
        /// 借款人地址
        /// </summary>
        public string ClientAddress { get; set; }
        /// <summary>
        /// 罚息利率（百分制）
        /// </summary>
        public string AmerceRate { get; set; }
        /// <summary>
        /// 支付给丙方的违约金（外贸合同）
        /// </summary>
        public string PayForB { get; set; }
        /// <summary>
        /// 甲方支付给丁方的违约金（外贸合同）
        /// </summary>
        public string PayForD { get; set; }
        /// <summary>
        /// 甲方支付担保费违约金
        /// </summary>
        public string PayForBandD { get; set; }
        /// <summary>
        /// 放款方
        /// </summary>
        public string Loaner { get; set; }

        /// <summary>
        /// 婚姻状况
        /// </summary>
        public string Marriage { get; set; }
        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; }
    }
  
}
