using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_LogisticsDetail")] 
    public class SuningLogisticsDetailEntity
    {
        public SuningLogisticsDetailEntity() { }

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

        private long orderID;
        /// <summary>
        /// 退货订单编号
        /// </summary>
        public long OrderID
        {
            get { return orderID; }
            set { orderID = value; }
        }

        private string logisticsTime;
        /// <summary>
        /// 处理时间
        /// </summary>
        public string LogisticsTime
        {
            get { return logisticsTime; }
            set { logisticsTime = value; }
        }
        private string logisticsContent;
        /// <summary>
        /// 处理内容
        /// </summary>
        public string LogisticsContent
        {
            get { return logisticsContent; }
            set { logisticsContent = value; }
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
         
		#endregion
    }
}
