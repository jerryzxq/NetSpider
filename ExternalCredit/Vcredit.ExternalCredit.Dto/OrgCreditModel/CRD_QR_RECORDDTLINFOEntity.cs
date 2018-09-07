using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace  Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_QR_RECORDDTLINFODto : BaseDto
	{

     
		 /// <summary>
		 /// 查询日期
		 /// </summary>
		public DateTime? Query_Date{get; set;}
		 /// <summary>
		 /// 查询操作员
		 /// </summary>
		public string Querier{get; set;}
		 /// <summary>
		 /// 查询原因
		 /// </summary>
		public string Query_Reason{get; set;}

	}
}