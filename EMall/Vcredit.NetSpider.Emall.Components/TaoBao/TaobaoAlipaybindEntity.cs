using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_alipaybind")]
	public class TaobaoAlipaybindEntity
	{
		public TaobaoAlipaybindEntity() { }

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
      
		private string account  ;
        /// <summary>
        /// 支付宝账户
        /// </summary>
		public string Account
		{
			get { return account; }
			set { account = value; }
		}
      
		private string email  ;
        /// <summary>
        /// 支付宝邮箱
        /// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// 支付宝手机
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string type  ;
        /// <summary>
        /// 支付宝账户类型
        /// </summary>
		public string Type
		{
			get { return type; }
			set { type = value; }
		}
      
		private string name  ;
        /// <summary>
        /// 姓名
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private string identityCard  ;
        /// <summary>
        /// 身份证
        /// </summary>
		public string IdentityCard
		{
			get { return identityCard; }
			set { identityCard = value; }
		}
      
		private string identityStatus  ;
        /// <summary>
        /// 实名认证状态
        /// </summary>
		public string IdentityStatus
		{
			get { return identityStatus; }
			set { identityStatus = value; }
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
