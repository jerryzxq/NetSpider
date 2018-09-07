using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_Goods")]
    //[Schema("dbo")]
	public class SuningGoodsEntity
	{
	
		public SuningGoodsEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 编号
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? orderID;
       

        /// <summary>
        /// 订单号
        /// </summary>
		public long? OrderID
		{
			get { return orderID; }
			set { orderID = value; }
		}
      
		private string goodsName;
       

        /// <summary>
        /// 产品名称
        /// </summary>
		public string GoodsName
		{
            get { return goodsName; }
            set { goodsName = value; }
		}
        private string goodsUrl;
        /// <summary>
        /// 产品路径
        /// </summary>

        public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
        }

		private int? goodsCount; 

        /// <summary>
        /// 产品数量
        /// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}

        private string imageUrl;
        /// <summary>
        /// 产品图片路径
        /// </summary>
        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; }
        }
		private string goodsType;
       

        /// <summary>
        /// 产品类型
        /// </summary>
		public string GoodsType
		{
			get { return goodsType; }
			set { goodsType = value; }
		}
      
		private string goodsProperty;
       

        /// <summary>
        /// 产品属性
        /// </summary>
		public string GoodsProperty
		{
			get { return goodsProperty; }
			set { goodsProperty = value; }
		}
      
		private decimal? price;
       

        /// <summary>
        /// 产品单价
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
		}
      
		private long? userId;
       

        /// <summary>
        /// 客户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName;
       

        /// <summary>
        /// 客户名称
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion

	}
}
