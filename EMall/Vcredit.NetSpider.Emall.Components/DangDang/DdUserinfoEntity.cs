using System;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.DangDang
{
	[Alias("dd_userinfo")]
	public class DdUserinfoEntity
	{
		public DdUserinfoEntity() { }

		#region Attributes

		private long id;
		/// <summary>
		/// ID
		/// </summary>
		[AutoIncrement]
		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		private string token;
		/// <summary>
		/// 会话令牌
		/// </summary>
		public string Token
		{
			get { return token; }
			set { token = value; }
		}

		private string account;
		/// <summary>
		/// 账号
		/// </summary>
		public string Account
		{
			get { return account; }
			set { account = value; }
		}

		private string accountId;
		/// <summary>
		/// 账号ID
		/// </summary>
		public string AccountId
		{
			get { return accountId; }
			set { accountId = value; }
		}

		private string email;
		/// <summary>
		/// 邮箱
		/// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}

		private string mobilePhone;
		/// <summary>
		/// 手机号码
		/// </summary>
		public string MobilePhone
		{
			get { return mobilePhone; }
			set { mobilePhone = value; }
		}

		private string userName;
		/// <summary>
		/// 用户名
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		private string nickName;
		/// <summary>
		/// 昵称
		/// </summary>
		public string NickName
		{
			get { return nickName; }
			set { nickName = value; }
		}

		private string area;
		/// <summary>
		/// 居住地
		/// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}

		private string photo;
		/// <summary>
		/// 头像地址
		/// </summary>
		public string Photo
		{
			get { return photo; }
			set { photo = value; }
		}

		private string sex;
		/// <summary>
		/// 性别
		/// </summary>
		public string Sex
		{
			get { return sex; }
			set { sex = value; }
		}

		private string identity;
		/// <summary>
		/// 身份
		/// </summary>
		public string Identity
		{
			get { return identity; }
			set { identity = value; }
		}

		private DateTime? birthDay;
		/// <summary>
		/// 生日
		/// </summary>
		public DateTime? BirthDay
		{
			get { return birthDay; }
			set { birthDay = value; }
		}

		private string blogUrl;
		/// <summary>
		/// 博客地址
		/// </summary>
		public string BlogUrl
		{
			get { return blogUrl; }
			set { blogUrl = value; }
		}

		private string livingCondition;
		/// <summary>
		/// 居住状态
		/// </summary>
		public string LivingCondition
		{
			get { return livingCondition; }
			set { livingCondition = value; }
		}

		private string hobby;
		/// <summary>
		/// 兴趣爱好
		/// </summary>
		public string Hobby
		{
			get { return hobby; }
			set { hobby = value; }
		}

		private string likePeople;
		/// <summary>
		/// 欣赏的人
		/// </summary>
		public string LikePeople
		{
			get { return likePeople; }
			set { likePeople = value; }
		}

		private string introduce;
		/// <summary>
		/// 自我介绍
		/// </summary>
		public string Introduce
		{
			get { return introduce; }
			set { introduce = value; }
		}

		private string securityLevel;
		/// <summary>
		/// 安全等级
		/// </summary>
		public string SecurityLevel
		{
			get { return securityLevel; }
			set { securityLevel = value; }
		}

		private bool isCrawled;
		/// <summary>
		/// 数据是否抓取
		/// </summary>
		public bool IsCrawled
		{
			get { return isCrawled; }
			set { isCrawled = value; }
		}

		private DateTime? createTime = DateTime.Now;
		/// <summary>
		/// 添加时间
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime;
		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
