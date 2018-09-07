using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.WeiXin.RestService
{
    public class ServiceConstants
    {
        public const int StatusCode_success = 0;
        public const int StatusCode_fail = 1;
        public const int StatusCode_error = 110;
        #region 社保采集城市
        public const string SocialSecurityCity_Qingdao = "QINGDAO";
        public const string SocialSecurityCity_Shanghai = "SHANGHAI";
        #endregion

        #region RC
        public const string RC_Model_KKD = "卡卡贷";
        public const string RC_Model_KKD_All = "卡卡贷全国版";
        public const string RC_Model_KKD_For58 = "卡卡贷58";
        public const string RC_Model_KKD_All_For58 = "卡卡贷全国版58";
        public const string RC_Model_Shebao = "网络版社保评分";
        public const string RC_Model_KKD_ZhiCe = "卡卡贷智策版";
        public const string RC_Model_KKD_All2016 = "卡卡贷全国版201601";
        public const string RC_Model_KKD_MobileScore = "卡卡贷手机申请评分";

        public const string RC_Result_KKD_CreditAmount = "*预授信额度";
        public const string RC_Result_KKD_MonthlyManageRate = "*月管理费率";
        public const string RC_Result_KKD_FormalitiesRate = "*手续费率";
        public const string RC_Result_KKD_LoanPeriod = "*贷款期限";
        public const string RC_Result_KKD_MonthlyInterestRate = "*月利率";
        public const string RC_Result_KKD_MonthlyServiceRate = "*月服务费率";
        public const string RC_Result_KKD_ExamineLoanMoney = "*审批金额";
        public const string RC_Result_KKD_MonthlyDebtRate = "月负债比例";
        public const string RC_Result_KKD_HighestPeriods = "*最高期数";
        public const string RC_Result_KKD_MobileScore = "*[手机评分_卡卡贷]卡卡贷手机申请评分";

        public const string RC_Model_XXQD = "星星钱袋";
        public const string RC_Model_XXQD_EduLoan = "星星钱袋-学历贷";
        public const string RC_Model_XXQD_MobileScore = "星星钱袋手机申请评分";
        public const string RC_Result_XXQD_MobileScore = "*[手机评分_星星钱袋]星星钱袋手机申请评分";
        #endregion

        #region 业务类型
        public const string BusType_Kakadai = "KAKADAI";
        public const string BusType_XXQD = "XXQD";
        #endregion
    }
}
