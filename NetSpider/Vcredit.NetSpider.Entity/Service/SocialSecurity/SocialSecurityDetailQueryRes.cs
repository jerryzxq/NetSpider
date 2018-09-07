using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class SocialSecurityDetailQueryRes : BaseRes
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 身份证
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 公司名
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 缴费类型
        /// </summary>
        public string PaymentType { get; set; }
        /// <summary>
        /// 缴费标记
        /// </summary>
        public string PaymentFlag { get; set; }
        /// <summary>
        /// 缴费年月
        /// </summary>
        public string PayTime { get; set; }
        /// <summary>
        /// 应属年月
        /// </summary>
        public string SocialInsuranceTime { get; set; }
        /// <summary>
        /// 社保基数
        /// </summary>
        public decimal SocialInsuranceBase { get; set; }
        /// <summary>
        /// 个人缴费（养老）
        /// </summary>
        public decimal PensionAmount { get; set; }
        /// <summary>
        /// 单位划入帐户
        /// </summary>
        public decimal CompanyPensionAmount { get; set; }
        /// <summary>
        /// 社平划入
        /// </summary>
        public decimal NationPensionAmount { get; set; }
        /// <summary>
        /// 个人缴费(医保)
        /// </summary>
        public decimal MedicalAmount { get; set; }
        /// <summary>
        /// 单位划入个人账户额（医保）
        /// </summary>
        public decimal CompanyMedicalAmount { get; set; }
        /// <summary>
        /// 大额救助
        /// </summary>
        public decimal IllnessMedicalAmount { get; set; }
        /// <summary>
        /// 共划入账户
        /// </summary>
        public decimal EnterAccountMedicalAmount { get; set; }
        /// <summary>
        /// 公务员补助账户
        /// </summary>
        public decimal CivilServantMedicalAmount { get; set; }
        /// <summary>
        /// 失业缴费
        /// </summary>
        public decimal UnemployAmount { get; set; }
        /// <summary>
        /// 生育保险缴费
        /// </summary>
        public decimal MaternityAmount { get; set; }
        /// <summary>
        /// 工伤保险缴费
        /// </summary>
        public decimal EmploymentInjuryAmount { get; set; }
        /// <summary>
        /// 年缴费月数
        /// </summary>
        public int YearPaymentMonths { get; set; }
        /// <summary>
        /// 社保总金额
        /// </summary>
        public decimal SocialInsuranceTotalAmount { get; set; }
        /// <summary>
        /// 个人账户社保金额
        /// </summary>
        public decimal PersonalTotalAmount { get; set; }
    }
}
