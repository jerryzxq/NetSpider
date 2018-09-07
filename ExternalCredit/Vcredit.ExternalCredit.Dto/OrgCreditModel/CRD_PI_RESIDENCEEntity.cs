using System;
using System.Collections.Generic;

using Vcredit.ExternalCredit.CommonLayer.Utility;
using Newtonsoft.Json;

namespace  Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_PI_RESIDENCEDto : BaseDto
	{
     
		 /// <summary>
		 /// 居住地址
		 /// </summary>
		public string Address{get; set;}
		 /// <summary>
		 /// 居住状况
		 /// </summary>
		public string Residence_Type{get; set;}
        
		 /// <summary>
		 /// 信息更新日期
		 /// </summary>
		public DateTime? Get_Time{get; set;}


	}
}