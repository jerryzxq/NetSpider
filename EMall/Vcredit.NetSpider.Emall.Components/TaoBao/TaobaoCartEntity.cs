using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_cart")]
	public class TaobaoCartEntity
	{
		public TaobaoCartEntity() { }

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
      
		private string seller  ;
        /// <summary>
        /// 卖家
        /// </summary>
		public string Seller
		{
			get { return seller; }
			set { seller = value; }
		}
      
		private string shopUrl  ;
        /// <summary>
        /// 店铺地址
        /// </summary>
		public string ShopUrl
		{
			get { return shopUrl; }
			set { shopUrl = value; }
		}
      
		private string productTitle  ;
        /// <summary>
        /// 商品标题
        /// </summary>
		public string ProductTitle
		{
			get { return productTitle; }
			set { productTitle = value; }
		}
      
		private string description  ;
        /// <summary>
        /// 商品说明
        /// </summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}
      
		private decimal? price  ;
        /// <summary>
        /// 单价
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private int? count  ;
        /// <summary>
        /// 数量
        /// </summary>
		public int? Count
		{
			get { return count; }
			set { count = value; }
		}
      
		private decimal? totalAmount  ;
        /// <summary>
        /// 总额
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private string service  ;
        /// <summary>
        /// 服务
        /// </summary>
		public string Service
		{
			get { return service; }
			set { service = value; }
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

        private bool isValid;
        /// <summary>
        /// 
        /// </summary>
		public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; }
        }


        private string _goodsUrl;
        /// <summary>
        /// 商品地址
        /// </summary>
		public string GoodsUrl
        {
            get { return _goodsUrl; }
            set { _goodsUrl = value; }
        }

        private string _imageUrl;
        /// <summary>
        /// 商品图片地址
        /// </summary>
		public string ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; }
        }

        #endregion
    }
}
