using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_CD_STN_OVDDto
    {
              

        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }
        /// <summary>
        /// 准贷记卡信息段ID
        /// </summary>

        public decimal Card_Id { get; set; }
        /// <summary>
        /// 透支月份
        /// </summary>
        public string Month_Dw { get; set; }
      
        /// <summary>
        /// 透支持续月数
        /// </summary>
        public decimal? Last_Months { get; set; }
      
        /// <summary>
        /// 透支金额
        /// </summary>
        public decimal? Amount { get; set; }



    }
}