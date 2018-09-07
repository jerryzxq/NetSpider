using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_PI_COMPETENCE")]
    [Schema("credit")]
    public partial class CRD_PI_COMPETENCEEntity : BaseEntity
	{



		 /// <summary>
		 /// 执业资格名称
		 /// </summary>
		public string Competency_Name{get; set;}
		 /// <summary>
		 /// 等级
		 /// </summary>
		public string Grade{get; set;}
		 /// <summary>
		 /// 获得日期
		 /// </summary>
		public string Award_Date{get; set;}
		 /// <summary>
		 /// 到期日期
		 /// </summary>
		public string End_Date{get; set;}
		 /// <summary>
		 /// 吊销日期
		 /// </summary>
		public string Revoke_Date{get; set;}
		 /// <summary>
		 /// 颁发机构
		 /// </summary>
		public string Organ_Name{get; set;}
		 /// <summary>
		 /// 机构所在地
		 /// </summary>
		public string Area{get; set;}


	}
}