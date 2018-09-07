using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public  class RequestEntity
    {
        public string  brNo { get; set; }
        /// <summary>
        /// 接口编号
        /// </summary>
        public string  txCode { get; set; }
        /// <summary>
        /// 请求日期
        /// </summary>
        public string  reqDate { get; set; }
        /// <summary>
        /// 请求时间
        /// </summary>
        public string reqTime { get; set; }
        /// <summary>
        /// 口令
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public string reqSerial { get; set; }
        /// <summary>
        /// 请求内容
        /// </summary>
        public string content { get; set; }


    }
}
