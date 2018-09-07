using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.RestService
{
    public class ServiceConstants
    {
        public const int StatusCode_success =0;
        public const int StatusCode_fail = 1;
        public const int StatusCode_error = 110;
        /// <summary>
        /// 多线程中上锁使用的对象
        /// </summary>
        private static object lockObject = new object();
        private static Dictionary<string, TaobaoSellerRes> _TaobaoSellerRess;
        public static Dictionary<string, TaobaoSellerRes> TaobaoSellerRess
        {
            get
            {
                lock (lockObject)
                {
                    if (_TaobaoSellerRess == null)
                    {
                        _TaobaoSellerRess = new Dictionary<string, TaobaoSellerRes>();
                    }
                }
                return _TaobaoSellerRess;
            }

        }

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

        public const string RC_Result_KKD_CreditAmount = "*预授信额度";
        public const string RC_Result_KKD_MonthlyManageRate = "*月管理费率";
        public const string RC_Result_KKD_FormalitiesRate = "*手续费率";
        public const string RC_Result_KKD_LoanPeriod = "*贷款期限";
        public const string RC_Result_KKD_MonthlyInterestRate = "*月利率";
        public const string RC_Result_KKD_MonthlyServiceRate = "*月服务费率";
        public const string RC_Result_KKD_ExamineLoanMoney = "*审批金额";
        public const string RC_Result_KKD_MonthlyDebtRate = "月负债比例";
        #endregion

        #region 业务类型
        public const string BusType_ZHITONGDDAI = "ZHITONGDDAI";
        #endregion
    }
}
