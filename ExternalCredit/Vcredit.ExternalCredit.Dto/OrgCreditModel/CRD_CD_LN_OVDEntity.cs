using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_CD_LN_OVDDto
    {


        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        public decimal Report_Id { get; set; }
        public decimal Card_Id { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string Month_Dw { get; set; }
        /// <summary>
        /// 持续月数
        /// </summary>
        public decimal? Last_Months { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Amount { get; set; }


    }
}