using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_order")]
	public class AmazonOrderEntity
	{
		public AmazonOrderEntity() { }

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
      
		private string orderNo  ;
        /// <summary>
        /// ������
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private DateTime? orderTime  ;
        /// <summary>
        /// �µ�ʱ��
        /// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}
      
		private string orderStatus  ;
        /// <summary>
        /// ����״̬
        /// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}
      
		private string orderStatusDesc  ;
        /// <summary>
        /// ����״̬����
        /// </summary>
		public string OrderStatusDesc
		{
			get { return orderStatusDesc; }
			set { orderStatusDesc = value; }
		}
      
		private decimal? totalAmount  ;
        /// <summary>
        /// ��Ʒ�ܶ�
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private decimal? freightAge  ;
        /// <summary>
        /// �˷�
        /// </summary>
		public decimal? FreightAge
		{
			get { return freightAge; }
			set { freightAge = value; }
		}
      
		private decimal? orderAmount  ;
        /// <summary>
        /// �����ܶ�
        /// </summary>
		public decimal? OrderAmount
		{
			get { return orderAmount; }
			set { orderAmount = value; }
		}
      
		private decimal? promotions  ;
        /// <summary>
        /// �Ż�
        /// </summary>
		public decimal? Promotions
		{
			get { return promotions; }
			set { promotions = value; }
		}
      
		private decimal? payAmount  ;
        /// <summary>
        /// ֧���ܶ�
        /// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}
      
		private string receiver  ;
        /// <summary>
        /// �ջ���
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
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
      
		private string zip  ;
        /// <summary>
        /// �ʱ�
        /// </summary>
		public string Zip
		{
			get { return zip; }
			set { zip = value; }
		}
      
		private string adress  ;
        /// <summary>
        /// ��ַ
        /// </summary>
		public string Adress
		{
			get { return adress; }
			set { adress = value; }
		}
        private string mobile;

        public string Mobile
        {
            get { return mobile; }
            set { mobile = value; }
        }

		private string payMethod  ;
        /// <summary>
        /// ֧����ʽ
        /// </summary>
		public string PayMethod
		{
			get { return payMethod; }
			set { payMethod = value; }
		}
      
		private string deliverMethod  ;
        /// <summary>
        /// ���ͷ�ʽ
        /// </summary>
		public string DeliverMethod
		{
			get { return deliverMethod; }
			set { deliverMethod = value; }
		}
      
		private string deliverTime  ;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public string DeliverTime
		{
			get { return deliverTime; }
			set { deliverTime = value; }
		}
      
		private string deliverLike  ;
        /// <summary>
        /// ����ϲ��
        /// </summary>
		public string DeliverLike
		{
			get { return deliverLike; }
			set { deliverLike = value; }
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
        /// �ͻ��˺���
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private List<AmazonGoodsEntity> _GoodsList = new List<AmazonGoodsEntity>();
        [Ignore]
        public List<AmazonGoodsEntity> GoodsList
        {
            get { return _GoodsList; }
            set { _GoodsList = value; }
        }
		#endregion
	}
}
