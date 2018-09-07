using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    public class QueryOrderReq:BaseReq
    {
        /// <summary>
        /// 订单开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 订单结束时间
        /// </summary>
        public string EndTime { get; set; }
    }
}
