using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    public  class QueryRequestEntity
    {
        public  int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string State { get; set; }
        public string Cert_No { get; set; }
        public string StartDate { get; set; }
        public string  EndDate { get; set; }
        public string BusType { get; set; }
        public string  SourceType { get; set; }

        public string  Name { get; set; }

    }
}
