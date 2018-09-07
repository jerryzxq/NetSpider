using System;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.YiHaoDian
{
	[Alias("yhd_userinfo")]
	public class YhdUserinfoEntity
	{
		public YhdUserinfoEntity() { }

		#region Attributes

		private long id  ;
		/// <summary>
		/// 唯一编号
		/// </summary>
		[AutoIncrement]
		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		private string token  ;
		/// <summary>
		/// Token
		/// </summary>
		public string Token
		{
			get { return token; }
			set { token = value; }
		}

		private string account  ;
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

		private string email  ;
		/// <summary>
		/// 邮箱
		/// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}

		private string mobilePhone  ;
		/// <summary>
		/// 手机号码
		/// </summary>
		public string MobilePhone
		{
			get { return mobilePhone; }
			set { mobilePhone = value; }
		}

		private string userName  ;
		/// <summary>
		/// 用户名
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		private string name  ;
		/// <summary>
		/// 真实姓名
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string nickName  ;
		/// <summary>
		/// 昵称
		/// </summary>
		public string NickName
		{
			get { return nickName; }
			set { nickName = value; }
		}

		private string photo  ;
		/// <summary>
		/// 头像地址
		/// </summary>
		public string Photo
		{
			get { return photo; }
			set { photo = value; }
		}

		private string sex  ;
		/// <summary>
		/// 性别
		/// </summary>
		public string Sex
		{
			get { return sex; }
			set { sex = value; }
		}

		private DateTime? birthDay  ;
		/// <summary>
		/// 生日
		/// </summary>
		public DateTime? BirthDay
		{
			get { return birthDay; }
			set { birthDay = value; }
		}

		private int? securityScore  ;
		/// <summary>
		/// 安全评分
		/// </summary>
		public int? SecurityScore
		{
			get { return securityScore; }
			set { securityScore = value; }
		}

		private string authName  ;
		/// <summary>
		/// 实名认证姓名
		/// </summary>
		public string AuthName
		{
			get { return authName; }
			set { authName = value; }
		}

		private string identityCard  ;
		/// <summary>
		/// 身份证号
		/// </summary>
		public string IdentityCard
		{
			get { return identityCard; }
			set { identityCard = value; }
		}

		private string memberLevel  ;
		/// <summary>
		/// 会员等级
		/// </summary>
		public string MemberLevel
		{
			get { return memberLevel; }
			set { memberLevel = value; }
		}

		private int? totalGrowthValue  ;
		/// <summary>
		/// 成长值
		/// </summary>
		public int? TotalGrowthValue
		{
			get { return totalGrowthValue; }
			set { totalGrowthValue = value; }
		}

		private bool isCrawled  ;
		/// <summary>
		/// 数据是否抓取
		/// </summary>
		public bool IsCrawled
		{
			get { return isCrawled; }
			set { isCrawled = value; }
		}

		private DateTime? createTime = DateTime.Now ;
		/// <summary>
		/// 添加时间
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime  ;
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
