using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.VipShop
{
	[Alias("vipshop_goods")]
	public class VipshopGoodsEntity
	{
		public VipshopGoodsEntity() { }

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
      
		private string accountName  ;
        /// <summary>
        /// 用户淘宝账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string goodsName  ;
        /// <summary>
        /// 商品名称
        /// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
		}
      
		private string goodsProperty  ;
        /// <summary>
        /// 商品属性
        /// </summary>
		public string GoodsProperty
		{
			get { return goodsProperty; }
			set { goodsProperty = value; }
		}
      
		private int? goodsCount  ;
        /// <summary>
        /// 每个商品购买数量
        /// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}
      
		private string goodsType  ;
        /// <summary>
        /// 商品类别
        /// </summary>
		public string GoodsType
		{
			get { return goodsType; }
			set { goodsType = value; }
		}

        private string _imageUrl;
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; }
        }

        private string goodsUrl;
        /// <summary>
        /// 
        /// </summary>
		public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }

        private decimal? price  ;
        /// <summary>
        /// 商品单价
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private string extraService  ;
        /// <summary>
        /// 增值服务
        /// </summary>
		public string ExtraService
		{
			get { return extraService; }
			set { extraService = value; }
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

        private DateTime? updateTime;
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
