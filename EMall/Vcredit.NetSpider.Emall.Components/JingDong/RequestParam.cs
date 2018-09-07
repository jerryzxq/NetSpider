using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.Entity.JingDong
{
    public  class RequestParam
    {
        /// <summary>
        /// 令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        ///开始时间
        /// </summary>
        public DateTime? StartTIme { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }


    }
}
