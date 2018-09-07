using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Vcredit.NetSpider.Entity.Service
{
    public class TaobaoSellerMS
    {
        public string YearAndMonth { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}