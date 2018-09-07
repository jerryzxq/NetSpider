using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_footprint")]
	public class TaobaoFootprintEntity
	{
		public TaobaoFootprintEntity() { }

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
      
		private DateTime? visitTime  ;
        /// <summary>
        /// 访问日期
        /// </summary>
		public DateTime? VisitTime
		{
			get { return visitTime; }
			set { visitTime = value; }
		}
      
		private string productCategory  ;
        /// <summary>
        /// 商品类目
        /// </summary>
		public string ProductCategory
		{
			get { return productCategory; }
			set { productCategory = value; }
		}
      
		private decimal? productPrice  ;
        /// <summary>
        /// 商品单价
        /// </summary>
		public decimal? ProductPrice
		{
			get { return productPrice; }
			set { productPrice = value; }
		}
      
		private string productBrand  ;
        /// <summary>
        /// 商品品牌
        /// </summary>
		public string ProductBrand
		{
			get { return productBrand; }
			set { productBrand = value; }
		}
      
		private bool? isPromote  ;
        /// <summary>
        /// 是否促销
        /// </summary>
		public bool? IsPromote
		{
			get { return isPromote; }
			set { isPromote = value; }
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
