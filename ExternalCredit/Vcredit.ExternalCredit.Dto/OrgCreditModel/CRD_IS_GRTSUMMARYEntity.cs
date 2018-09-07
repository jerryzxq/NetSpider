using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{
    public partial class CRD_IS_GRTSUMMARYDto : BaseDto
    {
      

        /// <summary>
        /// 担保笔数
        /// </summary>
        public decimal? Guarantee_Count { get; set; }
      
        /// <summary>
        /// 担保金额
        /// </summary>
        public decimal? Guarantee_Amount { get; set; }
      
        /// <summary>
        /// 担保本金余额
        /// </summary>
        public decimal? Guarantee_Balance { get; set; }


    }
}