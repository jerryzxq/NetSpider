using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
	public class CRD_QR_RECORDDTL
	{
		 /// <summary>
		 /// 
		 /// </summary>
		public  DateTime ? QueryDate{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string Querier{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  string QueryReason{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  int ? COUNTCARDIN3M{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  int ? COUNTCARDIN1M{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  int ? COUNTloanIN3M{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public  int ? COUNTloanIN1M{get; set;}
	}
}