using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillOrderLogistics")]

    public class AlipayBillOrderLogisticsEntity
    {

        public AlipayBillOrderLogisticsEntity() { }


        #region Attributes

        private long id;

        [AutoIncrement]
        /// <summary>
        /// 
        /// </summary>
        public long ID
        {
            get { return id; }
            set { id = value; }
        }

        private long? billOrderID;


        /// <summary>
        /// 
        /// </summary>
        public long? BillOrderID
        {
            get { return billOrderID; }
            set { billOrderID = value; }
        }

        private string logisticsNumber;


        /// <summary>
        /// 货运单号
        /// </summary>
        public string LogisticsNumber
        {
            get { return logisticsNumber; }
            set { logisticsNumber = value; }
        }

        private string logisticsType;


        /// <summary>
        /// 发货方式
        /// </summary>
        public string LogisticsType
        {
            get { return logisticsType; }
            set { logisticsType = value; }
        }

        private string logisticsPhone;


        /// <summary>
        /// 承运人电话
        /// </summary>
        public string LogisticsPhone
        {
            get { return logisticsPhone; }
            set { logisticsPhone = value; }
        }

        private string logisticsCompany;


        /// <summary>
        /// 承运人(公司)
        /// </summary>
        public string LogisticsCompany
        {
            get { return logisticsCompany; }
            set { logisticsCompany = value; }
        }

        private string userAddress;


        /// <summary>
        /// 
        /// </summary>
        public string UserAddress
        {
            get { return userAddress; }
            set { userAddress = value; }
        }

        private DateTime? createTime;


        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreateTime
        {
            get
            {

                if (createTime == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return createTime;
                }
            }
            set { createTime = value; }
        }

        private string telephone;


        /// <summary>
        /// 
        /// </summary>
        public string Telephone
        {
            get { return telephone; }
            set { telephone = value; }
        }

        private string receiver;


        /// <summary>
        /// 
        /// </summary>
        public string Receiver
        {
            get { return receiver; }
            set { receiver = value; }
        }

        #endregion

    }
}
