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
		/// �Ự����
		/// </summary>
		public string Token
		{
			get { return token; }
			set { token = value; }
		}

		private string account;
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

		private string email;
		/// <summary>
		/// ����
		/// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}

		private string mobilePhone;
		/// <summary>
		/// �ֻ�����
		/// </summary>
		public string MobilePhone
		{
			get { return mobilePhone; }
			set { mobilePhone = value; }
		}

		private string userName;
		/// <summary>
		/// �û���
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		private string nickName;
		/// <summary>
		/// �ǳ�
		/// </summary>
		public string NickName
		{
			get { return nickName; }
			set { nickName = value; }
		}

		private string area;
		/// <summary>
		/// ��ס��
		/// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}

		private string photo;
		/// <summary>
		/// ͷ���ַ
		/// </summary>
		public string Photo
		{
			get { return photo; }
			set { photo = value; }
		}

		private string sex;
		/// <summary>
		/// �Ա�
		/// </summary>
		public string Sex
		{
			get { return sex; }
			set { sex = value; }
		}

		private string identity;
		/// <summary>
		/// ���
		/// </summary>
		public string Identity
		{
			get { return identity; }
			set { identity = value; }
		}

		private DateTime? birthDay;
		/// <summary>
		/// ����
		/// </summary>
		public DateTime? BirthDay
		{
			get { return birthDay; }
			set { birthDay = value; }
		}

		private string blogUrl;
		/// <summary>
		/// ���͵�ַ
		/// </summary>
		public string BlogUrl
		{
			get { return blogUrl; }
			set { blogUrl = value; }
		}

		private string livingCondition;
		/// <summary>
		/// ��ס״̬
		/// </summary>
		public string LivingCondition
		{
			get { return livingCondition; }
			set { livingCondition = value; }
		}

		private string hobby;
		/// <summary>
		/// ��Ȥ����
		/// </summary>
		public string Hobby
		{
			get { return hobby; }
			set { hobby = value; }
		}

		private string likePeople;
		/// <summary>
		/// ���͵���
		/// </summary>
		public string LikePeople
		{
			get { return likePeople; }
			set { likePeople = value; }
		}

		private string introduce;
		/// <summary>
		/// ���ҽ���
		/// </summary>
		public string Introduce
		{
			get { return introduce; }
			set { introduce = value; }
		}

		private string securityLevel;
		/// <summary>
		/// ��ȫ�ȼ�
		/// </summary>
		public string SecurityLevel
		{
			get { return securityLevel; }
			set { securityLevel = value; }
		}

		private bool isCrawled;
		/// <summary>
		/// �����Ƿ�ץȡ
		/// </summary>
		public bool IsCrawled
		{
			get { return isCrawled; }
			set { isCrawled = value; }
		}

		private DateTime? createTime = DateTime.Now;
		/// <summary>
		/// ���ʱ��
		/// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

		private DateTime? updateTime;
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
