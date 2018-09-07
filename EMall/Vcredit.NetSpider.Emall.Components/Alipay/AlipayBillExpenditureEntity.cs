using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_BillExpenditure")]
    
	public class AlipayBillExpenditureEntity
	{
	
		public AlipayBillExpenditureEntity() { }


		#region Attributes
      
		private long iD;
       
     [AutoIncrement]
        /// <summary>
        /// 主键
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private long billID;
       

        /// <summary>
        /// 账单编号
        /// </summary>
		public long BillID
		{
			get { return billID; }
			set { billID = value; }
		}
      
	
      
		private string name;
       

        /// <summary>
        /// 消费名称
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private string orderNumber;
       

        /// <summary>
        /// 商户订单号
        /// </summary>
		public string OrderNumber
		{
			get { return orderNumber; }
			set { orderNumber = value; }
		}
      
		private decimal? amountPaid;
       

        /// <summary>
        /// 实付金额
        /// </summary>
		public decimal? AmountPaid
		{
			get { return amountPaid; }
			set { amountPaid = value; }
		}
      
		private decimal? discount;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal? Discount
		{
			get { return discount; }
			set { discount = value; }
		}
      
		private decimal? redAmount;
       

        /// <summary>
        /// 红包
        /// </summary>
		public decimal? RedAmount
		{
			get { return redAmount; }
			set { redAmount = value; }
		}
      
		private decimal? jiFenBao;
       

        /// <summary>
        /// 集分宝
        /// </summary>
		public decimal? JiFenBao
		{
			get { return jiFenBao; }
			set { jiFenBao = value; }
		}
      
		private decimal? serviceCharge;
       

        /// <summary>
        /// 服务费
        /// </summary>
		public decimal? ServiceCharge
		{
			get { return serviceCharge; }
			set { serviceCharge = value; }
		}
      
		private decimal? total;
       

        /// <summary>
        /// 总额
        /// </summary>
		public decimal? Total
		{
			get { return total; }
			set { total = value; }
		}
      
		private string otherInformation;
       

        /// <summary>
        /// 对方信息
        /// </summary>
		public string OtherInformation
		{
			get { return otherInformation; }
			set { otherInformation = value; }
		}
      
		private DateTime? expenditureTime;
       

        /// <summary>
        /// 支付创建时间
        /// </summary>
		public DateTime? ExpenditureTime
		{
			get { return expenditureTime; }
			set { expenditureTime = value; }
		}
      
		private DateTime? paymentTime;
       

        /// <summary>
        /// 付款时间
        /// </summary>
		public DateTime? PaymentTime
		{
			get { return paymentTime; }
			set { paymentTime = value; }
		}
      
		private DateTime? endTime;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime? EndTime
		{
			get { return endTime; }
			set { endTime = value; }
		}
      
		private string category;
       

        /// <summary>
        /// 分类
        /// </summary>
		public string Category
		{
			get { return category; }
			set { category = value; }
		}
      
		private string state;
       

        /// <summary>
        /// 状态
        /// </summary>
		public string State
		{
			get { return state; }
			set { state = value; }
		}
      
		private DateTime? createTime;
       

        /// <summary>
        /// 创建时间
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
		#endregion

	}
}
