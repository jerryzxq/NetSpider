using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
using ServiceStack.DataAnnotations;
namespace   Vcredit.NetSpider.Emall.Entity 
{
    [Alias("JD_ExpenseRecord")]
    
    public class ExpenseRecordEntity : BaseEntity  //消费记录
    {

        [Ignore]
        public string  CostDate { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        [Alias("CostDate")]
        public DateTime? CostDateReal { get { return CostDate.ToDateTime(); } }

        /// <summary>
        /// 订单号
        /// </summary>
        
        public string OrderNo { get; set; }

        /// <summary>
        /// 消费额扣减
        /// </summary>
       
      
        public string Costdeduct { get; set; }

        /// <summary>
        /// 消费额增加
        /// </summary>
        
        public string CostAdd { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
       
       
        public string Remark { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }

    }
}
