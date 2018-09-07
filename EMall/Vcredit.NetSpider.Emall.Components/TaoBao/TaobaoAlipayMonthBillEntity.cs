using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("t_taobao_alipaymonthbill")]

    public class TaobaoAlipayMonthBillEntity
    {

        public TaobaoAlipayMonthBillEntity() { }


        #region Attributes

        private UInt64 id;
       

        /// <summary>
        /// 
        /// </summary>
		public UInt64 Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private long userid;
       

        /// <summary>
        /// 
        /// </summary>
		public long Userid
		{
			get { return userid; }
			set { userid = value; }
		}
      
		private string account;
       

        /// <summary>
        /// 
        /// </summary>
		public string Account
		{
			get { return account; }
			set { account = value; }
		}
      
		private decimal online;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Online
		{
			get { return online; }
			set { online = value; }
		}
      
		private decimal offline;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Offline
		{
			get { return offline; }
			set { offline = value; }
		}
      
		private decimal payment;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Payment
		{
			get { return payment; }
			set { payment = value; }
		}
      
		private decimal recharge;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Recharge
		{
			get { return recharge; }
			set { recharge = value; }
		}
      
		private decimal consumeother;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Consumeother
		{
			get { return consumeother; }
			set { consumeother = value; }
		}
      
		private decimal total;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Total
		{
			get { return total; }
			set { total = value; }
		}
      
		private int month;
       

        /// <summary>
        /// 
        /// </summary>
		public int Month
		{
			get { return month; }
			set { month = value; }
		}
      
		private DateTime createtime = DateTime.Now;


        /// <summary>
        /// 
        /// </summary>
        public DateTime Createtime
		{
			get { return createtime; }
			set { createtime = value; }
		}
      
		private DateTime updatetime = DateTime.Now;


        /// <summary>
        /// 
        /// </summary>
        public DateTime Updatetime
		{
			get { return updatetime; }
			set { updatetime = value; }
		}
      
		private DateTime created_time;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime Created_time
		{
			get { return created_time; }
			set { created_time = value; }
		}
      
		private DateTime updated_time;
       

        /// <summary>
        /// 
        /// </summary>
		public DateTime Updated_time
		{
			get { return updated_time; }
			set { updated_time = value; }
		}
		#endregion

	}
}
