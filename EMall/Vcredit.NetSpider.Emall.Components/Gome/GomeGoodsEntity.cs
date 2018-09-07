using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Gome
{
	[Alias("gome_goods")]
	public class GomeGoodsEntity
	{
		public GomeGoodsEntity() { }

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
        /// 
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// 
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
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
      
		private int? goodsCount  ;
        /// <summary>
        /// 
        /// </summary>
		public int? GoodsCount
		{
			get { return goodsCount; }
			set { goodsCount = value; }
		}
      
		private string goodsType  ;
        /// <summary>
        /// 
        /// </summary>
		public string GoodsType
		{
			get { return goodsType; }
			set { goodsType = value; }
		}
      
		private decimal? price  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Price
		{
			get { return price; }
			set { price = value; }
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

        private string goodsUrl;
        /// <summary>
        /// 
        /// </summary>
		public string GoodsUrl
        {
            get { return goodsUrl; }
            set { goodsUrl = value; }
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
