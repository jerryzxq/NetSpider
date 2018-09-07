using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("suning_efubaotrade")]
	public class SuningEFuBaoTradeEntity
	{
		public SuningEFuBaoTradeEntity() { }

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
      
		private DateTime? tradeTime  ;
        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? TradeTime
		{
			get { return tradeTime; }
			set { tradeTime = value; }
		}
      
		private string orderNo  ;
        /// <summary>
        /// �������
        /// </summary>
		public string OrderNo
		{
			get { return orderNo; }
			set { orderNo = value; }
		}
      
		private string tradeNo  ;
        /// <summary>
        /// ���׺�
        /// </summary>
		public string TradeNo
		{
			get { return tradeNo; }
			set { tradeNo = value; }
		}
      
		private string tradeType  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string TradeType
		{
			get { return tradeType; }
			set { tradeType = value; }
		}
      
		private string tradeName  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string TradeName
		{
			get { return tradeName; }
			set { tradeName = value; }
		}
      
		private string trader  ;
        /// <summary>
        /// �Է�
        /// </summary>
		public string Trader
		{
			get { return trader; }
			set { trader = value; }
		}
      
		private decimal? tradeAmount  ;
        /// <summary>
        /// ���׽��
        /// </summary>
		public decimal? TradeAmount
		{
			get { return tradeAmount; }
			set { tradeAmount = value; }
		}
      
		private string tradeStatus  ;
        /// <summary>
        /// ����״̬
        /// </summary>
		public string TradeStatus
		{
			get { return tradeStatus; }
			set { tradeStatus = value; }
		}
      
		private string paymentMethod  ;
        /// <summary>
        /// ���׷�ʽ
        /// </summary>
		public string PaymentMethod
		{
			get { return paymentMethod; }
			set { paymentMethod = value; }
		}
      
		private string paymentDesc  ;
        /// <summary>
        /// ��������
        /// </summary>
		public string PaymentDesc
		{
			get { return paymentDesc; }
			set { paymentDesc = value; }
		}
      
		private decimal? orderAmount  ;
        /// <summary>
        /// �������
        /// </summary>
		public decimal? OrderAmount
		{
			get { return orderAmount; }
			set { orderAmount = value; }
		}
      
		private decimal? privilege  ;
        /// <summary>
        /// �Ż�
        /// </summary>
		public decimal? Privilege
		{
			get { return privilege; }
			set { privilege = value; }
		}
      
		private decimal? payableAmount  ;
        /// <summary>
        /// ʵ��֧��
        /// </summary>
		public decimal? PayableAmount
		{
			get { return payableAmount; }
			set { payableAmount = value; }
		}
      
		private string finalTradeStatus  ;
        /// <summary>
        /// ���ս���״̬
        /// </summary>
		public string FinalTradeStatus
		{
			get { return finalTradeStatus; }
			set { finalTradeStatus = value; }
		}
      
		private string refundNo  ;
        /// <summary>
        /// �˿���
        /// </summary>
		public string RefundNo
		{
			get { return refundNo; }
			set { refundNo = value; }
		}
      
		private DateTime? refundTime  ;
        /// <summary>
        /// �˿�ʱ��
        /// </summary>
		public DateTime? RefundTime
		{
			get { return refundTime; }
			set { refundTime = value; }
		}
      
		private decimal? refundAmount  ;
        /// <summary>
        /// �˿���
        /// </summary>
		public decimal? RefundAmount
		{
			get { return refundAmount; }
			set { refundAmount = value; }
		}
      
		private string refundMethod  ;
        /// <summary>
        /// �˿ʽ
        /// </summary>
		public string RefundMethod
		{
			get { return refundMethod; }
			set { refundMethod = value; }
		}
      
		private string refundDesc  ;
        /// <summary>
        /// �˿�����
        /// </summary>
		public string RefundDesc
		{
			get { return refundDesc; }
			set { refundDesc = value; }
		}
      
		private string refundRemark  ;
        /// <summary>
        /// �˿ע
        /// </summary>
		public string RefundRemark
		{
			get { return refundRemark; }
			set { refundRemark = value; }
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
        /// �ͻ��˺�
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

        private List<SuningEFuBaoTradeProcessEntity> _TradeProcess = new List<SuningEFuBaoTradeProcessEntity>();
        [Ignore]
        public List<SuningEFuBaoTradeProcessEntity> TradeProcess
        {
            get { return _TradeProcess; }
            set { _TradeProcess = value; }
        }

		#endregion
	}
}
