using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_CD_OverDueBreakeDto : BaseDto
    {

      

        /// <summary>
        /// 
        /// </summary>
        public int? BadBebtNum { get; set; }
      
        /// <summary>
        /// 
        /// </summary>
        public decimal? BadBebtMoney { get; set; }
      
        /// <summary>
        /// 
        /// </summary>
        public int? AssetDisposalNum { get; set; }
      
        /// <summary>
        /// 
        /// </summary>
        public decimal? AssetDisposalBalance { get; set; }
      
        /// <summary>
        /// 
        /// </summary>
        public int? GuarantorCompensatoryNum { get; set; }
      
        /// <summary>
        /// 
        /// </summary>
        public decimal? GuarantorCompensatoryBalance { get; set; }

    }
}