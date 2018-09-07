using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_growthRecord")]
	public class TaobaoGrowthRecordEntity
	{
		public TaobaoGrowthRecordEntity() { }

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
      
		private string details  ;
        /// <summary>
        /// 交易详情
        /// </summary>
		public string Details
		{
			get { return details; }
			set { details = value; }
		}
      
		private string orderNumber  ;
        /// <summary>
        /// 订单号
        /// </summary>
		public string OrderNumber
		{
			get { return orderNumber; }
			set { orderNumber = value; }
		}
      
		private decimal? amount  ;
        /// <summary>
        /// 交易额
        /// </summary>
		public decimal? Amount
		{
			get { return amount; }
			set { amount = value; }
		}
      
		private int? growthValue  ;
        /// <summary>
        /// 成长值
        /// </summary>
		public int? GrowthValue
		{
			get { return growthValue; }
			set { growthValue = value; }
		}
      
		private DateTime? getTime  ;
        /// <summary>
        /// 获得时间
        /// </summary>
		public DateTime? GetTime
		{
			get { return getTime; }
			set { getTime = value; }
		}
      
		private string remark  ;
        /// <summary>
        /// 备注(成长值获得来源)
        /// </summary>
		public string Remark
		{
			get { return remark; }
			set { remark = value; }
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
