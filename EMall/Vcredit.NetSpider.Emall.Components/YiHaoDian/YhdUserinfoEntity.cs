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
		/// Ψһ���
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
		/// �˺�
		/// </summary>
		public string Account
		{
			get { return account; }
			set { account = value; }
		}

		private string accountId;
		/// <summary>
		/// �˺�ID
		/// </summary>
		public string AccountId
		{
			get { return accountId; }
			set { accountId = value; }
		}

		private string email  ;
		/// <summary>
		/// ����
		/// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}

		private string mobilePhone  ;
		/// <summary>
		/// �ֻ�����
		/// </summary>
		public string MobilePhone
		{
			get { return mobilePhone; }
			set { mobilePhone = value; }
		}

		private string userName  ;
		/// <summary>
		/// �û���
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		private string name  ;
		/// <summary>
		/// ��ʵ����
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string nickName  ;
		/// <summary>
		/// �ǳ�
		/// </summary>
		public string NickName
		{
			get { return nickName; }
			set { nickName = value; }
		}

		private string photo  ;
		/// <summary>
		/// ͷ���ַ
		/// </summary>
		public string Photo
		{
			get { return photo; }
			set { photo = value; }
		}

		private string sex  ;
		/// <summary>
		/// �Ա�
		/// </summary>
		public string Sex
		{
			get { return sex; }
			set { sex = value; }
		}

		private DateTime? birthDay  ;
		/// <summary>
		/// ����
		/// </summary>
		public DateTime? BirthDay
		{
			get { return birthDay; }
			set { birthDay = value; }
		}

		private int? securityScore  ;
		/// <summary>
		/// ��ȫ����
		/// </summary>
		public int? SecurityScore
		{
			get { return securityScore; }
			set { securityScore = value; }
		}

		private string authName  ;
		/// <summary>
		/// ʵ����֤����
		/// </summary>
		public string AuthName
		{
			get { return authName; }
			set { authName = value; }
		}

		private string identityCard  ;
		/// <summary>
		/// ���֤��
		/// </summary>
		public string IdentityCard
		{
			get { return identityCard; }
			set { identityCard = value; }
		}

		private string memberLevel  ;
		/// <summary>
		/// ��Ա�ȼ�
		/// </summary>
		public string MemberLevel
		{
			get { return memberLevel; }
			set { memberLevel = value; }
		}

		private int? totalGrowthValue  ;
		/// <summary>
		/// �ɳ�ֵ
		/// </summary>
		public int? TotalGrowthValue
		{
			get { return totalGrowthValue; }
			set { totalGrowthValue = value; }
		}

		private bool isCrawled  ;
		/// <summary>
		/// �����Ƿ�ץȡ
		/// </summary>
		public bool IsCrawled
		{
			get { return isCrawled; }
			set { isCrawled = value; }
		}

		private DateTime? createTime = DateTime.Now ;
		/// <summary>
		/// ���ʱ��
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime  ;
		/// <summary>
		/// ����ʱ��
		/// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
