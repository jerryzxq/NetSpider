using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Mobile
{
    public class MobileCall
    {

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 通话地点
        /// </summary>
        public string CallPlace { get; set; }

        /// <summary>
        /// 对方号码
        /// </summary>
        public string OtherCallPhone { get; set; }

        /// <summary>
        /// 通话时长
        /// </summary>
        public string UseTime { get; set; }

        /// <summary>
        /// 通话类型
        /// </summary>
        public string CallType { get; set; }

        /// <summary>
        /// 呼叫类型（主叫被叫）
        /// </summary>
        public string InitType { get; set; }

        /// <summary>
        /// 通话费用
        /// </summary>
        public decimal SubTotal { get; set; }
    }
}
