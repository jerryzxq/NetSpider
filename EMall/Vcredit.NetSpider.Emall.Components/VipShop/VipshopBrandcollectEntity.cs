using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Vipshop
{
	[Alias("vipshop_brandcollect")]
	public class VipshopBrandcollectEntity
	{
		public VipshopBrandcollectEntity() { }

		#region Attributes
      
		private long id  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public long Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private long userId  ;
        /// <summary>
        /// 
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private long? brandId  ;
        /// <summary>
        /// 
        /// </summary>
		public long? BrandId
		{
			get { return brandId; }
			set { brandId = value; }
		}
      
		private string brandName  ;
        /// <summary>
        /// 
        /// </summary>
		public string BrandName
		{
			get { return brandName; }
			set { brandName = value; }
		}
      
		private string brandLogo  ;
        /// <summary>
        /// 
        /// </summary>
		public string BrandLogo
		{
			get { return brandLogo; }
			set { brandLogo = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
      
		private DateTime? updateTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
