using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_CD_STN_SPLDto
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
        /// 类型
        /// </summary>
        public string Type_Dw { get; set; }
      
        /// <summary>
        /// 发生日期
        /// </summary>
        public DateTime? Get_Time { get; set; }
      
        /// <summary>
        /// 变更月数
        /// </summary>
        public decimal? Changing_Months { get; set; }
      
        /// <summary>
        /// 发生金额
        /// </summary>
        public decimal? Changing_Amount { get; set; }
        /// <summary>
        /// 明细记录
        /// </summary>
        public string Content { get; set; }
     

    }
}