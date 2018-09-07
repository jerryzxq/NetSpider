using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.VipShop
{
    [Alias("vipshop_userInfo")]
    public class VipshopUserInfoEntity
    {
        public VipshopUserInfoEntity() { }

        #region Attributes

        private long id;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        private string token;
        /// <summary>
        /// 
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

        private string fixedPhone;
        /// <summary>
        /// 固定电话
        /// </summary>
		public string FixedPhone
        {
            get { return fixedPhone; }
            set { fixedPhone = value; }
        }

        private string password;
        /// <summary>
        /// 密码
        /// </summary>
		public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private string name;
        /// <summary>
        /// 姓名
        /// </summary>
		public string Name
        {
            get { return name; }
            set { name = value; }
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

        private string sex;
        /// <summary>
        /// 性别
        /// </summary>
		public string Sex
        {
            get { return sex; }
            set { sex = value; }
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

        private string area;
        /// <summary>
        /// 个人所在地
        /// </summary>
		public string Area
        {
            get { return area; }
            set { area = value; }
        }

        private string address;
        /// <summary>
        /// 详细地址
        /// </summary>
		public string Address
        {
            get { return address; }
            set { address = value; }
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

        private bool isCrawled;
        /// <summary>
        /// 
        /// </summary>
		public bool IsCrawled
        {
            get { return isCrawled; }
            set { isCrawled = value; }
        }

        private DateTime? createTime = DateTime.Now;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private DateTime? updateTime;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
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

        #endregion
    }
}
