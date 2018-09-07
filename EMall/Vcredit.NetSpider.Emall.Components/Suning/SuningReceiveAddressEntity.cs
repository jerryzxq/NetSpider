using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_ReceiveAddress")]
    //[Schema("dbo")]
	public class SuningReceiveAddressEntity
	{
	
		public SuningReceiveAddressEntity() { }


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

     private long userId;
       

        /// <summary>
        /// ������Ϣ���
        /// </summary>
     public long UserId
		{
            get { return userId; }
            set { userId = value; }
		}

        private string receiver;
       

        /// <summary>
        /// ����
        /// </summary>
        public string Receiver
		{
            get { return receiver; }
            set { receiver = value; }
		}
      
		private string mobile;
       

        /// <summary>
        /// �ֻ�  
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}

        private string telephone;
       

        /// <summary>
        /// �̶��绰
        /// </summary>
        public string Telephone
		{
            get { return telephone; }
            set { telephone = value; }
		}
      
		private string province;
       

        /// <summary>
        /// ʡ
        /// </summary>
		public string Province
		{
			get { return province; }
			set { province = value; }
		}
      
		private string city;
       

        /// <summary>
        /// ��
        /// </summary>
		public string City
		{
			get { return city; }
			set { city = value; }
		}
      
		private string area;
       

        /// <summary>
        /// ��
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string address;
       

        /// <summary>
        /// ��ַ
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
        private int? defaultAdressFlag;

        /// <summary>
        /// Ĭ���ջ���ַ
        /// </summary>
        public int? DefaultAdressFlag
        {
            get { return defaultAdressFlag; }
            set { defaultAdressFlag = value; }
        }

        private string acountName;

        /// <summary>
        /// �˻���
        /// </summary>
        public string AcountName
        {
            get { return acountName; }
            set { acountName = value; }
        }

        private DateTime? createTime;
         
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion

	}
}
