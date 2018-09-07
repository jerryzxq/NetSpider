using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Cache.Common
{
    internal class CacheKeys
    {
        /// <summary>
        /// 存放爬虫cookie的key
        /// </summary>
        [Description("存放爬虫cookie的key")]
        public const string CookieBaseKey = "Cookies:";
    }
}
