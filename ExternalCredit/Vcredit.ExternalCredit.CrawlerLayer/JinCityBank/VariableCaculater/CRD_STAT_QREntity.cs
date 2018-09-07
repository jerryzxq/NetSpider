
using System;
using System.Collections.Generic;

namespace Vcredit.ExternalCredit.CrawlerLayer.JinCityBank.VariableCaculater
{
    #region CRD_STAT_QREntity
    public class CRD_STAT_QREntity
    {
        public int Id { get; set; }

        public int? ReportId { get; set; }

        /// <summary>
        /// 全部查询记录统计()
        /// </summary>
        public int? M3ALLCNTTOTAL { get; set; }
        /// <summary>
        /// 贷记卡查询记录统计()
        /// </summary>
        public int? M3CREDITCNT { get; set; }
        /// <summary>
        /// 贷款查询记录统计()
        /// </summary>
        public int? M3LOANCNT { get; set; }
   
        /// <summary>
        /// 过去3个月信用卡审批查询次数
        /// </summary>
        public int L3M_ACCT_QRY_NUM { get; set; }
 
        /// <summary>
        /// 过去3个月贷款审批查询次数
        /// </summary>
        public int L3M_LN_QRY_NUM { get; set; }

        /// <summary>
        /// 非银行机构的贷款审批查询次数
        /// </summary>
        public int NON_BNK_LN_QRY_CNT { get; set; }
    }
    #endregion
}