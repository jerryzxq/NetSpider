using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.TaoBao
{
	[Alias("taobao_userinfo")]
	public class TaobaoUserInfoEntity
	{
		public TaobaoUserInfoEntity() { }

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
      
		private string token  ;
        /// <summary>
        /// 
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
      
        //private string password  ;
        ///// <summary>
        ///// 密码
        ///// </summary>
        //public string Password
        //{
        //    get { return password; }
        //    set { password = value; }
        //}
      
		private string umToken  ;
        /// <summary>
        /// 
        /// </summary>
		public string UmToken
		{
			get { return umToken; }
			set { umToken = value; }
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
      
		private string constellation  ;
        /// <summary>
        /// 星座
        /// </summary>
		public string Constellation
		{
			get { return constellation; }
			set { constellation = value; }
		}
      
		private string residenceCode  ;
        /// <summary>
        /// 居住地
        /// </summary>
		public string ResidenceCode
		{
			get { return residenceCode; }
			set { residenceCode = value; }
		}
      
		private string hometownCode  ;
        /// <summary>
        /// 家乡
        /// </summary>
		public string HometownCode
		{
			get { return hometownCode; }
			set { hometownCode = value; }
		}
      
		private string securityLevels  ;
        /// <summary>
        /// 安全等级
        /// </summary>
		public string SecurityLevels
		{
			get { return securityLevels; }
			set { securityLevels = value; }
		}
      
		private bool? isAuthentication  ;
        /// <summary>
        /// 身份认证
        /// </summary>
		public bool? IsAuthentication
		{
			get { return isAuthentication; }
			set { isAuthentication = value; }
		}
      
		private string authChannel  ;
        /// <summary>
        /// 认证渠道
        /// </summary>
		public string AuthChannel
		{
			get { return authChannel; }
			set { authChannel = value; }
		}
      
		private string authPasstime  ;
        /// <summary>
        /// 认证通过时间
        /// </summary>
		public string AuthPasstime
		{
			get { return authPasstime; }
			set { authPasstime = value; }
		}
      
		private string authName  ;
        /// <summary>
        /// 
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
      
		private string identityCardExpiry  ;
        /// <summary>
        /// 身份证有效期
        /// </summary>
		public string IdentityCardExpiry
		{
			get { return identityCardExpiry; }
			set { identityCardExpiry = value; }
		}
      
		private string extraAuth  ;
        /// <summary>
        /// 淘宝补充认证
        /// </summary>
		public string ExtraAuth
		{
			get { return extraAuth; }
			set { extraAuth = value; }
		}
      
		private bool? isSetPassword  ;
        /// <summary>
        /// 是否设置登录密码
        /// </summary>
		public bool? IsSetPassword
		{
			get { return isSetPassword; }
			set { isSetPassword = value; }
		}
      
		private bool? isSetQuestion  ;
        /// <summary>
        /// 是否设置密保问题
        /// </summary>
		public bool? IsSetQuestion
		{
			get { return isSetQuestion; }
			set { isSetQuestion = value; }
		}
      
		private bool? isBoundPhone  ;
        /// <summary>
        /// 是否绑定手机
        /// </summary>
		public bool? IsBoundPhone
		{
			get { return isBoundPhone; }
			set { isBoundPhone = value; }
		}
      
		private string accessMyDyna  ;
        /// <summary>
        /// 谁可以访问我的动态
        /// </summary>
		public string AccessMyDyna
		{
			get { return accessMyDyna; }
			set { accessMyDyna = value; }
		}
      
		private bool? isSearchMe  ;
        /// <summary>
        /// 允许搜找到我
        /// </summary>
		public bool? IsSearchMe
		{
			get { return isSearchMe; }
			set { isSearchMe = value; }
		}
      
		private string followMe  ;
        /// <summary>
        /// 允许搜找到我
        /// </summary>
		public string FollowMe
		{
			get { return followMe; }
			set { followMe = value; }
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
      
		private string area  ;
        /// <summary>
        /// 个人所在地
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string address  ;
        /// <summary>
        /// 详细地址
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string zip  ;
        /// <summary>
        /// 邮政编码
        /// </summary>
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
      
		private string fixedPhone  ;
        /// <summary>
        /// 固定电话
        /// </summary>
		public string FixedPhone
		{
			get { return fixedPhone; }
			set { fixedPhone = value; }
		}
      
		private bool? isFixedPhoneInTrading  ;
        /// <summary>
        /// 固定电话是否为交易联系方式
        /// </summary>
		public bool? IsFixedPhoneInTrading
		{
			get { return isFixedPhoneInTrading; }
			set { isFixedPhoneInTrading = value; }
		}
      
		private string creditRank  ;
        /// <summary>
        /// 信用等级
        /// </summary>
		public string CreditRank
		{
			get { return creditRank; }
			set { creditRank = value; }
		}
      
		private int? credit  ;
        /// <summary>
        /// 信用度
        /// </summary>
		public int? Credit
		{
			get { return credit; }
			set { credit = value; }
		}
      
		private decimal? favorableRate  ;
        /// <summary>
        /// 好评率
        /// </summary>
		public decimal? FavorableRate
		{
			get { return favorableRate; }
			set { favorableRate = value; }
		}
      
		private decimal? yearPurchaseAmount  ;
        /// <summary>
        /// 最近一年购买总金额
        /// </summary>
		public decimal? YearPurchaseAmount
		{
			get { return yearPurchaseAmount; }
			set { yearPurchaseAmount = value; }
		}
      
		private decimal? averageAmount  ;
        /// <summary>
        /// 平均每单金额
        /// </summary>
		public decimal? AverageAmount
		{
			get { return averageAmount; }
			set { averageAmount = value; }
		}
      
		private int? issueCount  ;
        /// <summary>
        /// 交易纠纷数
        /// </summary>
		public int? IssueCount
		{
			get { return issueCount; }
			set { issueCount = value; }
		}
      
		private int? notPayCount  ;
        /// <summary>
        /// 拍下未付款笔数
        /// </summary>
		public int? NotPayCount
		{
			get { return notPayCount; }
			set { notPayCount = value; }
		}
      
		private bool isCrawled  ;
        /// <summary>
        /// 
        /// </summary>
		public bool IsCrawled
		{
			get { return isCrawled; }
			set { isCrawled = value; }
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
