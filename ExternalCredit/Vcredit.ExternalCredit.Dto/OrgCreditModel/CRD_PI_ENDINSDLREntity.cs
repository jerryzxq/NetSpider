using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_PI_ENDINSDLRDto : BaseDto
    {


        /// <summary>
        /// 发放地
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 离退休类别
        /// </summary>
        public string Retire_Type { get; set; }
        /// <summary>
        /// 离退休月份
        /// </summary>
        public string Retired_Date { get; set; }
        /// <summary>
        /// 参加工作月份
        /// </summary>
        public string Work_Date { get; set; }
      
        /// <summary>
        /// 本月实发养老金
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 停发原因
        /// </summary>
        public string Pause_Reason { get; set; }
        /// <summary>
        /// 原单位名称
        /// </summary>
        public string Organ_Name { get; set; }
      
        /// <summary>
        /// 信息更新日期
        /// </summary>
        public DateTime? Get_Time { get; set; }


    }
}