using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("t_taobao_alipayinfo")]
     
	public class TaobaoAlipayinfoEntity
    {
	
		public TaobaoAlipayinfoEntity() { }


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
      
		private decimal flowersbalance;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Flowersbalance
		{
			get { return flowersbalance; }
			set { flowersbalance = value; }
		}
      
		private decimal floweravailable;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Floweravailable
		{
			get { return floweravailable; }
			set { floweravailable = value; }
		}
      
		private string flowercreditauthresult;
       

        /// <summary>
        /// 
        /// </summary>
		public string Flowercreditauthresult
		{
			get { return flowercreditauthresult; }
			set { flowercreditauthresult = value; }
		}
      
		private decimal jiebaibalance;
       

        /// <summary>
        /// 
        /// </summary>
		public decimal Jiebaibalance
		{
			get { return jiebaibalance; }
			set { jiebaibalance = value; }
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
