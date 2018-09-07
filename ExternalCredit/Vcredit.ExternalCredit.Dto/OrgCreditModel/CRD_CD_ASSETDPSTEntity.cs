using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_CD_ASSETDPSTDto : BaseDto
    {


        /// <summary>
        /// 资产管理公司
        /// </summary>
        public string Organ_Name { get; set; }
     
        /// <summary>
        /// 债务接收日期
        /// </summary>
        public DateTime? Get_Time { get; set; }

        /// <summary>
        /// 接收的债务金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 最近一次还款日期
        /// </summary>
        public string Latest_Repay_Date { get; set; }
   
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? Balance { get; set; }


    }
}