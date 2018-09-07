using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_receiveaddress")]
	public class AmazonReceiveAddressEntity
	{
		public AmazonReceiveAddressEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// ���
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string name  ;
        /// <summary>
        /// �ջ�������
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// �绰
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string tel  ;
        /// <summary>
        /// �̻�
        /// </summary>
		public string Tel
		{
			get { return tel; }
			set { tel = value; }
		}
      
		private string province  ;
        /// <summary>
        /// ʡ
        /// </summary>
		public string Province
		{
			get { return province; }
			set { province = value; }
		}
      
		private string city  ;
        /// <summary>
        /// ��
        /// </summary>
		public string City
		{
			get { return city; }
			set { city = value; }
		}
      
		private string area  ;
        /// <summary>
        /// ��
        /// </summary>
		public string Area
		{
			get { return area; }
			set { area = value; }
		}
      
		private string address  ;
        /// <summary>
        /// ��ַ
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string zip  ;
        /// <summary>
        /// �ʱ�
        /// </summary>
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
      
		private string deliverMethods  ;
        /// <summary>
        /// ���ͷ�ʽ
        /// </summary>
		public string DeliverMethods
		{
			get { return deliverMethods; }
			set { deliverMethods = value; }
		}
      
		private int? defaultAdressFlag  ;
        /// <summary>
        /// Ĭ�ϵ�ַ��־
        /// </summary>
		public int? DefaultAdressFlag
		{
			get { return defaultAdressFlag; }
			set { defaultAdressFlag = value; }
		}
      
		private string aKeyOrder  ;
        /// <summary>
        /// һ���µ�
        /// </summary>
		public string AKeyOrder
		{
			get { return aKeyOrder; }
			set { aKeyOrder = value; }
		}
      
		private string adressOtherName  ;
        /// <summary>
        /// ��ַ����
        /// </summary>
		public string AdressOtherName
		{
			get { return adressOtherName; }
			set { adressOtherName = value; }
		}
        private int userId;
        /// <summary>
        /// �ͻ����
        /// </summary>
        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        private string accountName;
         
        /// <summary>
        /// �ͻ��˺�
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// ��������
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
