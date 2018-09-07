using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    public class Net
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 通信地点
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// 网络类型
        /// </summary>
        public string NetType { get; set; }

        /// <summary>
        /// 上网方式
        /// </summary>
        public string PhoneNetType { get; set; }

        /// <summary>
        /// 单次费用
        /// </summary>
        public decimal? SubTotal { get; set; }

        /// <summary>
        /// 单次流量
        /// </summary>
        public string SubFlow { get; set; }

        /// <summary>
        /// 上网时长
        /// </summary>
        public string UseTime { get; set; }
    }
}
