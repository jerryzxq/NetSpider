using System;
using System.Collections.Generic;
using Vcredit.ExternalCredit.CommonLayer.Utility;
using Newtonsoft.Json;

namespace  Vcredit.ExternalCredit.Dto.OrgCreditModel
{
    public partial class CRD_QR_REORDSMRDto : BaseDto
	{


		 /// <summary>
		 /// 统计类型
		 /// </summary>
		public string Type_Id{get; set;}
		 /// <summary>
		 /// 统计原因
		 /// </summary>
		public string Reason{get; set;}
      
		 /// <summary>
		 /// 统计数量
		 /// </summary>
		public decimal? Sum_Dw{get; set;}


	}
}