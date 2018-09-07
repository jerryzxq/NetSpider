using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class WechatQueryReq
    {
        /// <summary>
        /// 登录名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string Vercode { get; set; }
        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Smscode { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string Identitycard { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 会话令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 微信订单id
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// 查询码
        /// </summary>
        public string Querycode { get; set; }
        /// <summary>
        /// 是否本地籍
        /// </summary>
        public string IsLocal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IsPaySocial { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 教育程度
        /// </summary>
        public string Education { get; set; }
        /// <summary>
        /// 婚姻状态
        /// </summary>
        public string Marriage { get; set; }
        /// <summary>
        /// 征信评分
        /// </summary>
        public int CreditScore { get; set; }
        /// <summary>
        /// 社保评分
        /// </summary>
        public int SocialSecurityScore { get; set; }
        /// <summary>
        /// 手机评分
        /// </summary>
        public int MobileScore { get; set; }
        /// <summary>
        /// 手机评分
        /// </summary>
        public int MobileScore2 { get; set; }
        /// <summary>
        /// 百融评分
        /// </summary>
        public int BaiRongScore { get; set; }
        /// <summary>
        /// 期限
        /// </summary>
        public string LoanPeriod { get; set; }
        /// <summary>
        /// 申请金额
        /// </summary>
        public string ApplyLoanMoney { get; set; }
        /// <summary>
        /// 月服务费率
        /// </summary>
        public decimal MonthlyServiceRate { get; set; }
        /// <summary>
        /// 月利率
        /// </summary>
        public decimal MonthlyInterestRate { get; set; }
        /// <summary>
        /// 申请交单时间
        /// </summary>
        public string ApplyTime { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public string Age { get; set; }
        /// <summary>
        /// 申请地
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 月收入
        /// </summary>
        public string IncomeMonth { get; set; }
        /// <summary>
        /// VBS业务号
        /// </summary>
        public int Bid { get; set; }
        /// <summary>
        /// 渠道
        /// </summary>
        public string ChannelSource { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// [智策]风险分
        /// </summary>
        public string UNAD_rskScore { get; set; }

        /// <summary>
        /// [智策]近一个月消费金额
        /// </summary>
        public string UNAD_lastmonthAmt { get; set; }

        /// <summary>
        /// [智策]历史最早交易日期
        /// </summary>
        public string UNAD_firstTradeDate { get; set; }

        /// <summary>
        /// [智策]最近年单笔最大消费金额
        /// </summary>
        public string UNAD_threeYearsMaxAmt { get; set; }
        /// <summary>
        /// 是否缴纳公积金
        /// </summary>
        public string PayHousingFund { get; set; }
    }
}
