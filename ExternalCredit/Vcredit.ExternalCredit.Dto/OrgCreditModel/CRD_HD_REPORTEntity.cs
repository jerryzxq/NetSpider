using System;
using System.Collections.Generic;

using Vcredit.ExtTrade.CommonLayer;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_HD_REPORTDto
    {
       
        public decimal Report_Id { get; set; }
        /// <summary>
        /// 报告编号
        /// </summary>
        public string Report_Sn { get; set; }
      
        /// <summary>
        /// 查询请求时间
        /// </summary>
        public DateTime? Query_Time { get; set; }
      
        /// <summary>
        /// 报告时间
        /// </summary>
        public DateTime? Report_Create_Time { get; set; }
        /// <summary>
        /// 被查询者姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 被查询者证件类型
        /// </summary>
        public string Cert_Type { get; set; }
        /// <summary>
        /// 被查询者证件号码
        /// </summary>
        public string Cert_No { get; set; }
        /// <summary>
        /// 查询原因
        /// </summary>
        public string Query_Reason { get; set; }
    
        /// <summary>
        /// 查询机构
        /// </summary>
        public string Query_Org { get; set; }

        /// <summary>
        /// PCQS更新时间
        /// </summary>
        public DateTime? Time_Stamp { get; set; }
    }
}