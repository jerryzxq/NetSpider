using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    [Serializable]
    public class AliPayWinServiceRes
    {
        public string Token { get; set; }
        public string ResToken { get; set; }

        public string Ua { get; set; }
        public string Value { get; set; }
        public string Password { get; set; }
        public string PK1 { get; set; }
        public string TS1 { get; set; }
        public string Sid1 { get; set; }

        public string PK2 { get; set; }
        public string Ksk1 { get; set; }
        public string Pk3 { get; set; }

        public string Timestamp1 { get; set; }

        public int LoanType { get; set; }
        
    }
}
