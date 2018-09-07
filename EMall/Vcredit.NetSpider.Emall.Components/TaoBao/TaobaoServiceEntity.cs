using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_service")]
	public class TaobaoServiceEntity
	{
		public TaobaoServiceEntity() { }

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
      
		private int complaintCount  ;
        /// <summary>
        /// 已投诉笔数
        /// </summary>
		public int ComplaintCount
		{
			get { return complaintCount; }
			set { complaintCount = value; }
		}
      
		private decimal complaintAmount  ;
        /// <summary>
        /// 已投诉金额
        /// </summary>
		public decimal ComplaintAmount
		{
			get { return complaintAmount; }
			set { complaintAmount = value; }
		}
      
		private int refundCount  ;
        /// <summary>
        /// 退款中笔数
        /// </summary>
		public int RefundCount
		{
			get { return refundCount; }
			set { refundCount = value; }
		}
      
		private decimal refundAmount  ;
        /// <summary>
        /// 退款中金额
        /// </summary>
		public decimal RefundAmount
		{
			get { return refundAmount; }
			set { refundAmount = value; }
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
