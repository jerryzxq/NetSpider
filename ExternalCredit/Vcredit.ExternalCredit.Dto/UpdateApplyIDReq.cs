using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Dto
{
    public class UpdateApplyIDReq
    {
        public string Cert_No { get; set; }
        public string Name { get; set; }
        public string BusType { get; set; }
        public string  Date { get; set; }
        public int? ApplyID { get; set; }
    }
}
