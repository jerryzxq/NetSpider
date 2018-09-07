using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Gome
{
    [Alias("gome_userinfo")]
    public class GomeUserinfoEntity
    {
        public GomeUserinfoEntity() { }

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
        /// 
        /// </summary>
		public string Account
        {
            get { return account; }
            set { account = value; }
        }

        private string password;
        /// <summary>
        /// 
        /// </summary>
		public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private string nickName;
        /// <summary>
        /// 
        /// </summary>
		public string NickName
        {
            get { return nickName; }
            set { nickName = value; }
        }

        private string photo;
        /// <summary>
        /// 
        /// </summary>
		public string Photo
        {
            get { return photo; }
            set { photo = value; }
        }

        private string sex;
        /// <summary>
        /// 
        /// </summary>
		public string Sex
        {
            get { return sex; }
            set { sex = value; }
        }

        private string mobilePhone;
        /// <summary>
        /// 
        /// </summary>
		public string MobilePhone
        {
            get { return mobilePhone; }
            set { mobilePhone = value; }
        }

        private string email;
        /// <summary>
        /// 
        /// </summary>
		public string Email
        {
            get { return email; }
            set { email = value; }
        }

        private DateTime? birthDay;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? BirthDay
        {
            get { return birthDay; }
            set { birthDay = value; }
        }

        private string address;
        /// <summary>
        /// 
        /// </summary>
		public string Address
        {
            get { return address; }
            set { address = value; }
        }

        private string marriage;
        /// <summary>
        /// 
        /// </summary>
		public string Marriage
        {
            get { return marriage; }
            set { marriage = value; }
        }

        private string income;
        /// <summary>
        /// 
        /// </summary>
		public string Income
        {
            get { return income; }
            set { income = value; }
        }

        private string peopleNum;
        /// <summary>
        /// 
        /// </summary>
		public string PeopleNum
        {
            get { return peopleNum; }
            set { peopleNum = value; }
        }

        private string education;
        /// <summary>
        /// 
        /// </summary>
		public string Education
        {
            get { return education; }
            set { education = value; }
        }

        private string job;
        /// <summary>
        /// 
        /// </summary>
		public string Job
        {
            get { return job; }
            set { job = value; }
        }

        private string concernCategory;
        /// <summary>
        /// 
        /// </summary>
		public string ConcernCategory
        {
            get { return concernCategory; }
            set { concernCategory = value; }
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
        /// 
        /// </summary>
		public string AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }
        #endregion
    }
}
