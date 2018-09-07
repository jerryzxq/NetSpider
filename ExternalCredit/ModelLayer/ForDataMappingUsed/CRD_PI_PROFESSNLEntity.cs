using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_PI_PROFESSNL")]
    [Schema("credit")]
    public partial class CRD_PI_PROFESSNLEntity : BaseEntity
	{

		 /// <summary>
		 /// 工作单位
		 /// </summary>
		public string Employer{get; set;}
		 /// <summary>
		 /// 单位地址
		 /// </summary>
		public string Employer_Address{get; set;}
		 /// <summary>
		 /// 职业
		 /// </summary>
		public string Occupation{get; set;}
		 /// <summary>
		 /// 行业
		 /// </summary>
		public string Industry{get; set;}
		 /// <summary>
		 /// 职务
		 /// </summary>
		public string Duty{get; set;}
		 /// <summary>
		 /// 职称
		 /// </summary>
		public string Title_Dw{get; set;}
		 /// <summary>
		 /// 进入本单位年份
		 /// </summary>
		public string Start_Year{get; set;}
		 /// <summary>
		 /// 信息更新日期
		 /// </summary>
		public DateTime Get_Time{get; set;}


	}
}