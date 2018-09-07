using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_QR_RECORDDTLDto : BaseDto
    {


        /// <summary>
        /// 三个月内信用卡审批查询次数
        /// </summary>
        public int? COUNT_CARD_IN3M { get; set; }
      
        /// <summary>
        /// 一个月内信用卡审批查询次数
        /// </summary>
        public int? COUNT_CARD_IN1M { get; set; }
      
        /// <summary>
        /// 三个月内贷款审批查询次数
        /// </summary>
        public int? COUNT_loan_IN3M { get; set; }
      
        /// <summary>
        /// 一个月内贷款审批查询次数
        /// </summary>
        public int? COUNT_loan_IN1M { get; set; }
      

    }
}