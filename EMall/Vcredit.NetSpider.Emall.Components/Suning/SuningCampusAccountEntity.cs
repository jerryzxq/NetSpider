using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_CampusAccount")]
    //[Schema("dbo")]
	public class SuningCampusAccountEntity
	{
	
		public SuningCampusAccountEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 编号
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? baicID;
       

        /// <summary>
        /// 基本信息编号
        /// </summary>
		public long? BaicID
		{
			get { return baicID; }
			set { baicID = value; }
		}
      
		private string realName;
       

        /// <summary>
        /// 真实姓名
        /// </summary>
		public string RealName
		{
			get { return realName; }
			set { realName = value; }
		}
      
		private string gender;
       

        /// <summary>
        /// 性别
        /// </summary>
		public string Gender
		{
			get { return gender; }
			set { gender = value; }
		}
      
		private string schoolName;
       

        /// <summary>
        /// 学校名称
        /// </summary>
		public string SchoolName
		{
			get { return schoolName; }
			set { schoolName = value; }
		}
      
		private string startYear;
       

        /// <summary>
        /// 入学年份
        /// </summary>
		public string StartYear
		{
			get { return startYear; }
			set { startYear = value; }
		}
      
		private string schoolSystem;
       

        /// <summary>
        /// 学制
        /// </summary>
		public string SchoolSystem
		{
			get { return schoolSystem; }
			set { schoolSystem = value; }
		}
      
		private string educationLevel;
       

        /// <summary>
        /// 教育程度 
        /// </summary>
		public string EducationLevel
		{
			get { return educationLevel; }
			set { educationLevel = value; }
		}
      
		private string qQId;
       

        /// <summary>
        /// QQ账号
        /// </summary>
		public string QQId
		{
            get { return qQId; }
            set { qQId = value; }
		}
      
		private DateTime? createTime;

        private string accountName;

        /// <summary>
        /// 账户名
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion

	}
}
