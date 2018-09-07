using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    public class MonthBill
    {
        /// <summary>
        /// 计费周期
        /// </summary>
        public string BillCycle { get; set; }
        /// <summary>
        /// 套餐金额
        /// </summary>
        public string PlanAmt { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public string TotalAmt { get; set; }
    }
}
