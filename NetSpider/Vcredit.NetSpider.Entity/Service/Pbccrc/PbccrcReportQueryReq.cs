using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class PbccrcReportQueryReq : BaseReq
    {
        public string querycode { get; set; }

        //public int bid { get; set; }
        //public string orderId { get; set; }
        public string BusId { get; set; }
        public string BusType { get; set; }
        public string IdentityCard { get; set; }
    }
}
