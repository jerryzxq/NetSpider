using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Gome
{
	[Alias("gome_collectgoods")]
	public class GomeCollectgoodsEntity
	{
		public GomeCollectgoodsEntity() { }

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
      
		private string brandName  ;
        /// <summary>
        /// 
        /// </summary>
		public string BrandName
		{
			get { return brandName; }
			set { brandName = value; }
		}
      
		private string imageUrl  ;
        /// <summary>
        /// 
        /// </summary>
		public string ImageUrl
		{
			get { return imageUrl; }
			set { imageUrl = value; }
		}
      
		private string goodsName  ;
        /// <summary>
        /// 
        /// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
		}
      
		private decimal? marketPrice  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? MarketPrice
		{
			get { return marketPrice; }
			set { marketPrice = value; }
		}
      
		private decimal? vipPrice  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? VipPrice
		{
			get { return vipPrice; }
			set { vipPrice = value; }
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
