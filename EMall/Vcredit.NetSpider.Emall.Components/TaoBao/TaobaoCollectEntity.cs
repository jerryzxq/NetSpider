using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_collect")]
	public class TaobaoCollectEntity
	{
		public TaobaoCollectEntity() { }

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
        /// 基础信息编号
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private int? collectProductCount  ;
        /// <summary>
        /// 收藏的宝贝数
        /// </summary>
		public int? CollectProductCount
		{
			get { return collectProductCount; }
			set { collectProductCount = value; }
		}
      
		private int? cutPriceCount  ;
        /// <summary>
        /// 降价宝贝数
        /// </summary>
		public int? CutPriceCount
		{
			get { return cutPriceCount; }
			set { cutPriceCount = value; }
		}
      
		private int? expiresCount  ;
        /// <summary>
        /// 失效宝贝数
        /// </summary>
		public int? ExpiresCount
		{
			get { return expiresCount; }
			set { expiresCount = value; }
		}
      
		private int? collectShopCount  ;
        /// <summary>
        /// 收藏的店铺数
        /// </summary>
		public int? CollectShopCount
		{
			get { return collectShopCount; }
			set { collectShopCount = value; }
		}
      
		private string shopTag  ;
        /// <summary>
        /// 店铺分类标签
        /// </summary>
		public string ShopTag
		{
			get { return shopTag; }
			set { shopTag = value; }
		}
      
		private int? shopHasBuyCount  ;
        /// <summary>
        /// 已经购买商品的店铺数
        /// </summary>
		public int? ShopHasBuyCount
		{
			get { return shopHasBuyCount; }
			set { shopHasBuyCount = value; }
		}
      
		private string shopRemark  ;
        /// <summary>
        /// 店铺备注
        /// </summary>
		public string ShopRemark
		{
			get { return shopRemark; }
			set { shopRemark = value; }
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
      
		private string createUser  ;
        /// <summary>
        /// 
        /// </summary>
		public string CreateUser
		{
			get { return createUser; }
			set { createUser = value; }
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
      
		private string updateUser  ;
        /// <summary>
        /// 
        /// </summary>
		public string UpdateUser
		{
			get { return updateUser; }
			set { updateUser = value; }
		}
		#endregion
	}
}
