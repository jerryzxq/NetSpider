using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Entity;

namespace Vcredit.NetSpider.Emall.Crawler
{
    public class AliPayData
    {
        private List<AlipayBaicEntity> _alipay_baic = new List<AlipayBaicEntity>();
        public List<AlipayBaicEntity> alipay_baic
        {
            get
            {
                return _alipay_baic;
            }
            set
            {
                _alipay_baic = value;
            }
        }

        private List<AlipayAddressEntity> _alipay_address = new List<AlipayAddressEntity>();

        public List<AlipayAddressEntity> alipay_address
        {
            get { return _alipay_address; }
            set { _alipay_address = value; }

        }
        private List<AlipayBankEntity> _alipay_bank = new List<AlipayBankEntity>();

        public List<AlipayBankEntity> alipay_bank
        {
            get { return _alipay_bank; }
            set { _alipay_bank = value; }

        }
        private List<AlipayBillEntity> _alipay_bill = new List<AlipayBillEntity>();

        public List<AlipayBillEntity> alipay_bill
        {
            get { return _alipay_bill; }
            set { _alipay_bill = value; }

        }

        /// <summary>
        /// 支付明细
        /// </summary>
        private List<AlipayBillPaymentEntity> _alipay_billpayment = new List<AlipayBillPaymentEntity>();

        public List<AlipayBillPaymentEntity> alipay_billpayment
        {
            get { return _alipay_billpayment; }
            set { _alipay_billpayment = value; }

        }
        /// <summary>
        /// 账单支付记录
        /// </summary>

        private List<AlipayBillExpenditureEntity> _alipay_billexpenditure = new List<AlipayBillExpenditureEntity>();
        public List<AlipayBillExpenditureEntity> alipay_billexpenditure
        {
            get { return _alipay_billexpenditure; }
            set { _alipay_billexpenditure = value; }

        }
        private List<AlipayBillCreditPaymentEntity> _alipay_billcreditpayment = new List<AlipayBillCreditPaymentEntity>();

        /// <summary>
        /// 账单信用卡还款
        /// </summary>


        public List<AlipayBillCreditPaymentEntity> alipay_billcreditpayment
        {
            get { return _alipay_billcreditpayment; }
            set { _alipay_billcreditpayment = value; }

        }
        private List<AlipayBillTransferEntity> _alipay_billtransfer = new List<AlipayBillTransferEntity>();
        public List<AlipayBillTransferEntity> alipay_billtransfer
        {
            get { return _alipay_billtransfer; }
            set { _alipay_billtransfer = value; }

        }
        private List<AlipayBillRefundsEntity> _alipay_billrefunds = new List<AlipayBillRefundsEntity>();
        public List<AlipayBillRefundsEntity> alipay_billrefunds
        {
            get { return _alipay_billrefunds; }
            set { _alipay_billrefunds = value; }

        }
        private List<AlipayBillOrderEntity> _alipay_billordery = new List<AlipayBillOrderEntity>();

        public List<AlipayBillOrderEntity> alipay_billorder
        {
            get { return _alipay_billordery; }
            set { _alipay_billordery = value; }

        }

        private List<AlipayBillOrderDetailEntity> _alipay_billorderdetail = new List<AlipayBillOrderDetailEntity>();

        public List<AlipayBillOrderDetailEntity> alipay_billorderdetail
        {
            get { return _alipay_billorderdetail; }
            set { _alipay_billorderdetail = value; }

        }

        private List<AlipayBillOrderLogisticsEntity> _alipay_billorderlogistics = new List<AlipayBillOrderLogisticsEntity>();

        public List<AlipayBillOrderLogisticsEntity> alipay_billorderlogistics
        {
            get { return _alipay_billorderlogistics; }
            set { _alipay_billorderlogistics = value; }

        }
        private List<AlipayBillFlowerEntity> _alipay_billflower = new List<AlipayBillFlowerEntity>();

        public List<AlipayBillFlowerEntity> alipay_billflower
        {
            get { return _alipay_billflower; }
            set { _alipay_billflower = value; }

        }
        /// <summary>
        /// 花呗消费明细
        /// </summary> 
        private List<AlipayBillFlowerOrderDetailEntity> _alipay_billflowerorderdetail = new List<AlipayBillFlowerOrderDetailEntity>();

        public List<AlipayBillFlowerOrderDetailEntity> alipay_billflowerorderdetail
        {
            get { return _alipay_billflowerorderdetail; }
            set { _alipay_billflowerorderdetail = value; }

        }
        /// <summary>
        /// 花呗消费明细
        /// </summary> 
        private List<AlipayBillFlowerRePaymentEntity> _alipay_billflowerrepayment = new List<AlipayBillFlowerRePaymentEntity>();

        public List<AlipayBillFlowerRePaymentEntity> alipay_billflowerrepayment
        {
            get { return _alipay_billflowerrepayment; }
            set { _alipay_billflowerrepayment = value; }

        }

    }

    public class AliPayEnd
    {
    }

    public class AliPayEndDetail
    {
        public string token { get; set; }
        public string user_id { get; set; }
        public long baicid { get; set; }
    }
    public class AliPayKafkaResult<T>
    {
        public string topic { get; set; }
        public string time { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }

        public T data { get; set; }
    }
}
