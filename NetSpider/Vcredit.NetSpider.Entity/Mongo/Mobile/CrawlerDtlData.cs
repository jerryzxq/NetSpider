using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    public class CrawlerDtlData
    {
        /// <summary>
        /// 抓取标题
        /// </summary>
        public string CrawlerTitle { get; set; }

        /// <summary>
        /// 抓取内容
        /// </summary>
        public byte[] CrawlerTxt { get; set; }
    }
}
