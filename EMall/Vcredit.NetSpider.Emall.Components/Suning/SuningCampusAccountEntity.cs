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
        /// ���
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long? baicID;
       

        /// <summary>
        /// ������Ϣ���
        /// </summary>
		public long? BaicID
		{
			get { return baicID; }
			set { baicID = value; }
		}
      
		private string realName;
       

        /// <summary>
        /// ��ʵ����
        /// </summary>
		public string RealName
		{
			get { return realName; }
			set { realName = value; }
		}
      
		private string gender;
       

        /// <summary>
        /// �Ա�
        /// </summary>
		public string Gender
		{
			get { return gender; }
			set { gender = value; }
		}
      
		private string schoolName;
       

        /// <summary>
        /// ѧУ����
        /// </summary>
		public string SchoolName
		{
			get { return schoolName; }
			set { schoolName = value; }
		}
      
		private string startYear;
       

        /// <summary>
        /// ��ѧ���
        /// </summary>
		public string StartYear
		{
			get { return startYear; }
			set { startYear = value; }
		}
      
		private string schoolSystem;
       

        /// <summary>
        /// ѧ��
        /// </summary>
		public string SchoolSystem
		{
			get { return schoolSystem; }
			set { schoolSystem = value; }
		}
      
		private string educationLevel;
       

        /// <summary>
        /// �����̶� 
        /// </summary>
		public string EducationLevel
		{
			get { return educationLevel; }
			set { educationLevel = value; }
		}
      
		private string qQId;
       

        /// <summary>
        /// QQ�˺�
        /// </summary>
		public string QQId
		{
            get { return qQId; }
            set { qQId = value; }
		}
      
		private DateTime? createTime;

        private string accountName;

        /// <summary>
        /// �˻���
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
