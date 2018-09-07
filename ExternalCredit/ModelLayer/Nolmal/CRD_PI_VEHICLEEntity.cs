using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_VEHICLE")]
    [Schema("credit")]
    public partial class CRD_PI_VEHICLEEntity : BaseEntity
	{



		 /// <summary>
		 /// 发送机号
		 /// </summary>
		public string Engine_Code{get; set;}
		 /// <summary>
		 /// 车牌号码
		 /// </summary>
		public string License_Code{get; set;}
		 /// <summary>
		 /// 品牌
		 /// </summary>
		public string Brand{get; set;}
		 /// <summary>
		 /// 车辆类型
		 /// </summary>
		public string Car_Type{get; set;}
		 /// <summary>
		 /// 使用性质
		 /// </summary>
		public string Use_Character{get; set;}
		 /// <summary>
		 /// 车辆状态
		 /// </summary>
		public string State{get; set;}
		 /// <summary>
		 /// 抵押标记
		 /// </summary>
		public string Pledge_Flag{get; set;}
        [JsonConverter(typeof(DesJsonConverter))]
		 /// <summary>
		 /// 信息更新日期
		 /// </summary>
		public DateTime? Get_Time{get; set;}


	}
}