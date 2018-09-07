using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExternalCredit.CommonLayer;

namespace Vcredit.ExtTrade.ModelLayer
{
    [Alias("CRD_HD_REPORT")]
    [Schema("credit")]
	public partial class CRD_HD_REPORTEntity
	{
        public CRD_HD_REPORTEntity()
        {
            Time_Stamp = DateTime.Now;
            SourceType = (byte)SysEnums.SourceType.Trade;
        }

        public string EvaluationResult { get; set; }
        /// <summary>
        /// 信用报告主表ID
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public decimal Report_Id { get; set; }

		 /// <summary>
		 /// 报告编号
		 /// </summary>
		public string Report_Sn{get; set;}
		 /// <summary>
		 /// 查询请求时间
		 /// </summary>
		public DateTime? Query_Time{get; set;}
		 /// <summary>
		 /// 报告时间
		 /// </summary>
		public DateTime? Report_Create_Time{get; set;}
		 /// <summary>
		 /// 被查询者姓名
		 /// </summary>
		public string Name{get; set;}
		 /// <summary>
		 /// 被查询者证件类型
		 /// </summary>
		public string Cert_Type{get; set;}
		 /// <summary>
		 /// 被查询者证件号码
		 /// </summary>
		public string Cert_No{get; set;}
		 /// <summary>
		 /// 查询原因
		 /// </summary>
		public string Query_Reason{get; set;}
		 /// <summary>
		 /// 查询版式
		 /// </summary>
		public string Query_Format{get; set;}
		 /// <summary>
		 /// 查询机构
		 /// </summary>
		public string Query_Org{get; set;}
		 /// <summary>
		 /// 查询操作员
		 /// </summary>
		public string User_Code{get; set;}
		 /// <summary>
		 /// 查询结果提示
		 /// </summary>
		public string Query_Result_Cue{get; set;}
		 /// <summary>
		 /// PCQS更新时间
		 /// </summary>
        public DateTime? Time_Stamp{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public DateTime? ImportTime{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public int Bh{get; set;}

		 /// <summary>
		 /// 
		 /// </summary>
		public string CreditcardNo{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public string BusIdentityCard{get; set;}
        /// <summary>
        /// 征信报告类型
        /// </summary>
        public byte SourceType { get; set; }


	}
}