using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("Suning_Order")]
    //[Schema("dbo")]
	public class SuningOrderEntity
	{
	
		public SuningOrderEntity() { }


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
      
		private string orderNo;
       

        /// <summary>
        /// ������
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private DateTime? orderTime;
       

        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? OrderTime
		{
			get { return orderTime; }
			set { orderTime = value; }
		}
        private string seller;

        /// <summary>
        /// �Է�
        /// </summary>
        public string Seller
        {
            get { return seller; }
            set { seller = value; }
        }
		private string orderType;
       

        /// <summary>
        /// �������
        /// </summary>
		public string OrderType
		{
			get { return orderType; }
			set { orderType = value; }
		}
      
		private string orderStatus;
       

        /// <summary>
        /// ����״̬
        /// </summary>
		public string OrderStatus
		{
			get { return orderStatus; }
			set { orderStatus = value; }
		}
      
		private string payType;
       

        /// <summary>
        /// ֧����ʽ
        /// </summary>
		public string PayType
		{
			get { return payType; }
			set { payType = value; }
		}
      
		private decimal? totalAmount;
       

        /// <summary>
        /// �����ܶ�
        /// </summary>
		public decimal? TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}
      
		private decimal? payAmount;
       

        /// <summary>
        /// ʵ���ܶ�
        /// </summary>
		public decimal? PayAmount
		{
			get { return payAmount; }
			set { payAmount = value; }
		}
      
		private string receiver;
       

        /// <summary>
        /// �ջ�������
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string telephone;
       

        /// <summary>
        /// �ջ��˵绰
        /// </summary>
		public string Telephone
		{
			get { return telephone; }
			set { telephone = value; }
		}
      
		private string adress;
       

        /// <summary>
        /// �ջ��˵�ַ
        /// </summary>
		public string Adress
		{
			get { return adress; }
			set { adress = value; }
		}
      
		private string invoiceHead;
       

        /// <summary>
        /// ��Ʊ̧ͷ
        /// </summary>
		public string InvoiceHead
		{
			get { return invoiceHead; }
			set { invoiceHead = value; }
		}
      
		private string invoiceContent;
       

        /// <summary>
        /// ��Ʊ����
        /// </summary>
		public string InvoiceContent
		{
			get { return invoiceContent; }
			set { invoiceContent = value; }
		}
      
		private decimal? freight;
       

        /// <summary>
        /// �˷�
        /// </summary>
		public decimal? Freight
		{
			get { return freight; }
			set { freight = value; }
		}
      
		private string logistics;
       

        /// <summary>
        /// ����
        /// </summary>
		public string Logistics
		{
			get { return logistics; }
			set { logistics = value; }
		}
      
		private string receivingMode;
       

        /// <summary>
        /// �ջ���ʽ
        /// </summary>
		public string ReceivingMode
		{
			get { return receivingMode; }
			set { receivingMode = value; }
		} 
		private string coupon;
       

        /// <summary>
        /// ��ȯ/�Ż�
        /// </summary>
		public string Coupon
		{
			get { return coupon; }
			set { coupon = value; }
		}

        private decimal? goodsAmount;
        /// <summary>
        /// �ϲ�������Ʒ�ܽ��
        /// </summary>
        public decimal? GoodsAmount
        {
            get { return goodsAmount; }
            set { goodsAmount = value; }
        } 
		private long? userId;
       

        /// <summary>
        /// �û����
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private string accountName;
       

        /// <summary>
        /// �û��˺�
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
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

        /// <summary>
        /// ������Ʒ
        /// </summary> 
        private List<SuningGoodsEntity> _OrderGoods = new List<SuningGoodsEntity>();
        [Ignore]
        public List<SuningGoodsEntity> OrderGoods
        {
            get { return _OrderGoods; }
            set { _OrderGoods = value; }

        }

        /// <summary>
        /// ��������
        /// </summary> 
        private List<SuningLogisticsDetailEntity> _OrderLogistics = new List<SuningLogisticsDetailEntity>();
        [Ignore]
        public List<SuningLogisticsDetailEntity> OrderLogistics
        {
            get { return _OrderLogistics; }
            set { _OrderLogistics = value; }

        } 
		#endregion

	}
}
