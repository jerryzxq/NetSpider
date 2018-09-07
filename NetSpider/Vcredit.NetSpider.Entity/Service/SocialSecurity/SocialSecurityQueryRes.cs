using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class SocialSecurityQueryRes : BaseRes
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
        /// 工作日期
        /// </summary>
        public string WorkDate { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司编号
        /// </summary>
        public string CompanyNo { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// 公司类型
        /// </summary>
        public string CompanyType { get; set; }
        /// <summary>
        /// 单位状态
        /// </summary>
        public string CompanyStatus { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public string BirthDate { get; set; }
        /// <summary>
        /// 人员状态
        /// </summary>
        public string EmployeeStatus { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public string Race { get; set; }
        /// <summary>
        /// 特殊工种
        /// </summary>
        public bool IsSpecialWork { get; set; }
        /// <summary>
        /// 退休类别
        /// </summary>
        public string RetireType { get; set; }
        /// <summary>
        /// 养老统筹级别
        /// </summary>
        public string PensionLevel { get; set; }
        /// <summary>
        /// 保健类型
        /// </summary>
        public string HealthType { get; set; }
        /// <summary>
        /// 特殊缴费类型
        /// </summary>
        public string SpecialPaymentType { get; set; }
        /// <summary>
        /// 发卡银行
        /// </summary>
        public string Bank { get; set; }
        /// <summary>
        /// 银行地址
        /// </summary>
        public string BankAddress { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// 通讯地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 养老保险视同缴费月数
        /// </summary>
        public int PaymentMonths { get; set; }
        /// <summary>
        /// 养老建账户前缴费月数
        /// </summary>
        public int OldPaymentMonths { get; set; }
        /// <summary>
        /// 24个月连续缴费月数
        /// </summary>
        public int PaymentMonths_Continuous { get; set; }
        /// <summary>
        /// 截止日期和月份
        /// </summary>
        public string DeadlineYearAndMonth { get; set; }
        /// <summary>
        /// 养老缴费基数
        /// </summary>
        public decimal SocialInsuranceBase { get; set; }
        /// <summary>
        /// 账户总额
        /// </summary>
        public decimal InsuranceTotal { get; set; }
        /// <summary>
        /// 工龄
        /// </summary>
        public string WorkingAge { get; set; }
        /// <summary>
        /// 个人账户总额
        /// </summary>
        public decimal PersonalInsuranceTotal { get; set; }
        /// <summary>
        /// 是否本地
        /// </summary>
        public bool IsLocal { get; set; }
        /// <summary>
        /// 社保缴费城市
        /// </summary>
        public string SocialSecurityCity { get; set; }
        /// <summary>
        /// 缴费状态
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
        public int? Score { get; set; }

        private List<SocialSecurityDetailQueryRes> _Details = new List<SocialSecurityDetailQueryRes>();
        public List<SocialSecurityDetailQueryRes> Details
        {
            get { return _Details; }
            set { this._Details = value; }
        }
    }
}
