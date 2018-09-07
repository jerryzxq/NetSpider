using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_ReturnGoodsOrder")] 
	public class SuningReturnGoodsOrderEntity 
    {
        public SuningReturnGoodsOrderEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}

        private string sellerName;

        /// <summary>
        /// 商家名称
        /// </summary>
        public string SellerName
        {
            get { return sellerName; }
            set { sellerName = value; }
        }
        private string orderNo;
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo
        {
            get { return orderNo; }
            set { orderNo = value; }
        }
        private string goodName;
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodName
        {
            get { return goodName; }
            set { goodName = value; }
        }
        private string orderTime;
        /// <summary>
        /// 下单时间或交易时间
        /// </summary>
        public string OrderTime
        {
            get { return orderTime; }
            set { orderTime = value; }
        }
        private int? returnStatus;
        /// <summary>
        /// 订单类型
        /// </summary>
        public int? ReturnStatus
        {
            get { return returnStatus; }
            set { returnStatus = value; }
        }
        private string submitTime;
        /// <summary>
        /// 提交时间
        /// </summary>
        public string SubmitTime
        {
            get { return submitTime; }
            set { submitTime = value; }
        }
        private string serviceType;
        /// <summary>
        /// 服务类型
        /// </summary>
        public string ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }
        private int? returnType;
        /// <summary>
        /// 退货类型
        /// </summary>
        public int? ReturnType
        {
            get { return returnType; }
            set { returnType = value; }
        }
        private string orderStatus;

        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus
        {
            get { return orderStatus; }
            set { orderStatus = value; }
        }
		private long? userId  ;
        /// <summary>
        /// 客户编号
        /// </summary>
		public long? UserId
		{
			get { return userId; }
			set { userId = value; }
		} 
		private string accountName  ;
        /// <summary>
        /// 客户账号
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}
      
		private DateTime createTime = DateTime.Now ;
        /// <summary>
        /// 该记录创建时间
        /// </summary>
		public DateTime CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
        private SuningReturnGoodsDetailEntity _ReturnDetail = new SuningReturnGoodsDetailEntity();

        [Ignore]
        public SuningReturnGoodsDetailEntity ReturnDetail
        {
            get { return _ReturnDetail; }
            set { _ReturnDetail = value; }
        }
        private List<SuningReturnGoodProcessEntity> _ReturnProcessList = new List<SuningReturnGoodProcessEntity>();

        [Ignore]
        public List<SuningReturnGoodProcessEntity> ReturnProcessList
        {
            get { return _ReturnProcessList; }
            set { _ReturnProcessList = value; }
        }

		#endregion
    }
}
