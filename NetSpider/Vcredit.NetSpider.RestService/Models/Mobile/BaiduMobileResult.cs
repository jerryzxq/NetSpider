using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vcredit.NetSpider.RestService.Models.Mobile
{
    public class BaiduMobileResult
    {
        public string phone { set; get; }
        public string prefix { set; get; }
        public string supplier { set; get; }
        public string province { set; get; }
        public string city { set; get; }
        public string suit { set; get; }
    }
}