using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_district")]
	public class TaobaoDistrictEntity
	{
		public TaobaoDistrictEntity() { }

		#region Attributes
      
		private int id  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public int Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private string code  ;
        /// <summary>
        /// 当前编码
        /// </summary>
		public string Code
		{
			get { return code; }
			set { code = value; }
		}
      
		private string name  ;
        /// <summary>
        /// 名称
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private string parentCode  ;
        /// <summary>
        /// 父节点编码
        /// </summary>
		public string ParentCode
		{
			get { return parentCode; }
			set { parentCode = value; }
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
