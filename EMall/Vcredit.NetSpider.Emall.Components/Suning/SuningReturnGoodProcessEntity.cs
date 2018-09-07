using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_ReturnGoodProcess")] 
    public class SuningReturnGoodProcessEntity
    {
        public SuningReturnGoodProcessEntity() { }

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

        private long returnID;
        /// <summary>
        /// 退货订单编号
        /// </summary>
        public long ReturnID
        {
            get { return returnID; }
            set { returnID = value; }
        }

        private DateTime? processTime;
        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? ProcessTime
        {
            get { return processTime; }
            set { processTime = value; }
        }
        private string processContent;
        /// <summary>
        /// 处理内容
        /// </summary>
        public string ProcessContent
        {
            get { return processContent; }
            set { processContent = value; }
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
