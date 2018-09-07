using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class PaginateReq
    {
        public int? Start { get; set; }
        public string Sort { get; set; }
        public string Order { get; set; }
        public int? Size { get; set; }
    }
}
