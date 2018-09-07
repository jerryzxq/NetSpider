using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_evaluation")]
	public class TaobaoEvaluationEntity
	{
		public TaobaoEvaluationEntity() { }

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
      
		private int waitCount  ;
        /// <summary>
        /// 待评价数
        /// </summary>
		public int WaitCount
		{
			get { return waitCount; }
			set { waitCount = value; }
		}
      
		private int totalCount  ;
        /// <summary>
        /// 总评价数
        /// </summary>
		public int TotalCount
		{
			get { return totalCount; }
			set { totalCount = value; }
		}
      
		private int buyerGoodCount  ;
        /// <summary>
        /// 买家好评数
        /// </summary>
		public int BuyerGoodCount
		{
			get { return buyerGoodCount; }
			set { buyerGoodCount = value; }
		}
      
		private int buyerMiddleCount  ;
        /// <summary>
        /// 买家中评数
        /// </summary>
		public int BuyerMiddleCount
		{
			get { return buyerMiddleCount; }
			set { buyerMiddleCount = value; }
		}
      
		private int buyerBadCount  ;
        /// <summary>
        /// 买家差评数
        /// </summary>
		public int BuyerBadCount
		{
			get { return buyerBadCount; }
			set { buyerBadCount = value; }
		}
      
		private decimal goodRate  ;
        /// <summary>
        /// 好评率
        /// </summary>
		public decimal GoodRate
		{
			get { return goodRate; }
			set { goodRate = value; }
		}
      
		private int sellerCount  ;
        /// <summary>
        /// 卖家主动评价
        /// </summary>
		public int SellerCount
		{
			get { return sellerCount; }
			set { sellerCount = value; }
		}
      
		private int sellerDefaultCount  ;
        /// <summary>
        /// 卖家默认好评
        /// </summary>
		public int SellerDefaultCount
		{
			get { return sellerDefaultCount; }
			set { sellerDefaultCount = value; }
		}
      
		private int sellerGoodCount  ;
        /// <summary>
        /// 卖家好评数
        /// </summary>
		public int SellerGoodCount
		{
			get { return sellerGoodCount; }
			set { sellerGoodCount = value; }
		}
      
		private int sellerMiddleCount  ;
        /// <summary>
        /// 卖家中评数
        /// </summary>
		public int SellerMiddleCount
		{
			get { return sellerMiddleCount; }
			set { sellerMiddleCount = value; }
		}
      
		private int sellerBadCount  ;
        /// <summary>
        /// 卖家差评数
        /// </summary>
		public int SellerBadCount
		{
			get { return sellerBadCount; }
			set { sellerBadCount = value; }
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
