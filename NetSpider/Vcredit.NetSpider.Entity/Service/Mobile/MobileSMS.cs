using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Mobile
{
    public class MobileSMS
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 通信地点
        /// </summary>
        public string SmsPlace { get; set; }

        /// <summary>
        /// 对方号码
        /// </summary>
        public string OtherSmsPhone { get; set; }

        /// <summary>
        /// 通信类型（短信，彩信）
        /// </summary>
        public string SmsType { get; set; }

        /// <summary>
        /// 通信方式（发送接收）
        /// </summary>
        public string InitType { get; set; }

        /// <summary>
        /// 通信费
        /// </summary>
        public decimal SubTotal { get; set; }
    }
}
