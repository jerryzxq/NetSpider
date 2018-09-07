using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_goods")]
	public class TaobaoGoodsEntity
	{
		public TaobaoGoodsEntity() { }

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
      
		private string accountName = string.Empty;
        /// <summary>
        /// 用户淘宝账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private string orderNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string goodsName = string.Empty;
        /// <summary>
        /// 商品名称
        /// </summary>
		public string GoodsName
		{
			get { return goodsName; }
			set { goodsName = value; }
		}
      
		private string goodsProperty = string.Empty;
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
      
		private string goodsType = string.Empty;
        /// <summary>
        /// 商品类别
        /// </summary>
		public string GoodsType
		{
			get { return goodsType; }
			set { goodsType = value; }
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
      
		private string extraService = string.Empty;
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

        private string _goodsUrl;
        /// <summary>
        /// 商品地址
        /// </summary>
		public string GoodsUrl
        {
            get { return _goodsUrl; }
            set { _goodsUrl = value; }
        }
        private string _imageUrl = string.Empty;
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; }
        }
        #endregion
    }
}
