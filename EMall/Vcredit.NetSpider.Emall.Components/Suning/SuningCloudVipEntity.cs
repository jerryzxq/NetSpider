using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_CloudVipOrder")] 
	public class SuningCloudVipOrderEntity
	{
		public SuningCloudVipOrderEntity() { }

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
      
		private string businessName  ;
        /// <summary>
        /// �̻���
        /// </summary>
		public string BusinessName
		{
			get { return businessName; }
			set { businessName = value; }
		}
      
		private string source  ;
        /// <summary>
        /// ��Դ
        /// </summary>
		public string Source
		{
			get { return source; }
			set { source = value; }
		}
      
		private int? cloudVip  ;
        /// <summary>
        /// ����
        /// </summary>
		public int? CloudVip
		{
			get { return cloudVip; }
			set { cloudVip = value; }
		}
      
		private int? cloudVipType  ;
        /// <summary>
        /// ��������
        /// </summary>
		public int? CloudVipType
		{
			get { return cloudVipType; }
			set { cloudVipType = value; }
		}
      
		private DateTime? cloudVipCreateTime  ;
        /// <summary>
        /// �������ʱ��
        /// </summary>
		public DateTime? CloudVipCreateTime
		{
			get { return cloudVipCreateTime; }
			set { cloudVipCreateTime = value; }
		}
      
		private DateTime? cloudVipEndTime  ;
        /// <summary>
        /// �������ʱ��
        /// </summary>
		public DateTime? CloudVipEndTime
		{
			get { return cloudVipEndTime; }
			set { cloudVipEndTime = value; }
		}
        private string remark;

        /// <summary>
        /// ��ע
        /// </summary>
        public string Remark
        {
            get { return remark; }
            set { remark = value; }
        }
		private long? userId  ;
        /// <summary>
        /// �ͻ����
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName  ;
        /// <summary>
        /// �ͻ�����
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
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
