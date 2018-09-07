using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_Logistics")]
    
    public class LogisticsEntity:BaseEntity
    {
        /// <summary>
        /// 订单号
        /// </summary>

        public string  OrderNo { get; set; }
        /// <summary>
        /// 货运单号
        /// </summary>
        public string WaybillNumber { get; set; }
        /// <summary>
        /// 配送方式
        /// </summary>
        public string DistributionMode { get; set; }
        /// <summary>
        /// 送货方式
        /// </summary>
        public string DeliveryMode { get; set; }
        /// <summary>
        /// 承运人
        /// </summary>
        public string CarrierPhone { get; set; }
        /// <summary>
        /// 承运人电话
        /// </summary>
        public string Carrier { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }
        /// <summary>
        /// 配送结束时间
        /// </summary>
        public DateTime? DeliveryTime { get; set; }

    }
}
