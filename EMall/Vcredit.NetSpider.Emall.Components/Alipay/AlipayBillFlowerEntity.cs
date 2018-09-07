using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillFlower")]
    
    public class AlipayBillFlowerEntity
    {

        public AlipayBillFlowerEntity() { }


        #region Attributes

        private long iD;

        [AutoIncrement]
        /// <summary>
        /// ����
        /// </summary>
        public long ID
        {
            get { return iD; }
            set { iD = value; }
        }
         
        private long baicID;


        /// <summary>
        /// �˻����
        /// </summary>
        public long BaicID
        {
            get { return baicID; }
            set { baicID = value; }
        }

        private string accountName;


        /// <summary>
        /// �˻��˺�
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        private string flowerMonth;


        /// <summary>
        /// �˵��·�
        /// </summary>
        public string FlowerMonth
        {
            get { return flowerMonth; }
            set { flowerMonth = value; }
        }

        private decimal? billAmount;


        /// <summary>
        /// �����˵����
        /// </summary>
        public decimal? BillAmount
        {
            get { return billAmount; }
            set { billAmount = value; }
        }

        private decimal? overdueInterest;


        /// <summary>
        /// ������Ϣ
        /// </summary>
        public decimal? OverdueInterest
        {
            get { return overdueInterest; }
            set { overdueInterest = value; }
        }

        private decimal? amortizationAmount;


        /// <summary>
        /// ���ڻ�����
        /// </summary>
        public decimal? AmortizationAmount
        {
            get { return amortizationAmount; }
            set { amortizationAmount = value; }
        }

        private decimal? hasAmount;


        /// <summary>
        /// �����ѻ����
        /// </summary>
        public decimal? HasAmount
        {
            get { return hasAmount; }
            set { hasAmount = value; }
        }
        //
        private string orderStatus;

        /// <summary>
        /// ����״̬
        /// </summary>
        public string OrderStatus
        {
            get { return orderStatus; }
            set { orderStatus = value; }
        }
        private int? orderState;

        /// <summary>
        /// ����״̬���
        /// </summary>
        public int? OrderState
        {
            get { return orderState; }
            set { orderState = value; }
        }

        private decimal? noHasAmount;

        /// <summary>
        /// ����δ�����
        /// </summary>
        public decimal? NoHasAmount
        {
            get { return noHasAmount; }
            set { noHasAmount = value; }
        }

        private DateTime? finalRepayDate;


        /// <summary>
        /// ��󻹿�����
        /// </summary>
        public DateTime? FinalRepayDate
        {
            get { return finalRepayDate; }
            set { finalRepayDate = value; }
        }

        private DateTime createTime;


        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private DateTime? updateTime;


        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime? UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
        }

        /// <summary>
        /// ����������ϸ
        /// </summary> 
        private List<AlipayBillFlowerOrderDetailEntity> _BillFlowerOrderDetail = new List<AlipayBillFlowerOrderDetailEntity>();
        [Ignore]
        public List<AlipayBillFlowerOrderDetailEntity> BillFlowerOrderDetail
        {
            get { return _BillFlowerOrderDetail; }
            set { _BillFlowerOrderDetail = value; }

        } 
        /// <summary>
        /// ����������ϸ
        /// </summary> 
        private List<AlipayBillFlowerRePaymentEntity> _BillFlowerRePayment = new List<AlipayBillFlowerRePaymentEntity>();
        [Ignore]
        public List<AlipayBillFlowerRePaymentEntity> BillFlowerRePayment
        {
            get { return _BillFlowerRePayment; }
            set { _BillFlowerRePayment = value; }

        }
        #endregion

    }
}
