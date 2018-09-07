using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class UnionPayReq
    {
        public string Token { get; set; }
        public string creditcardNo { get; set; }
        public string mobile { get; set; }
        public string cvn2 { get; set; }
        public string expire { get; set; }
        public string smsCode { get; set; }
        public string credentialType { get; set; }
        public string credential { get; set; }
        public string name { get; set; }
        public string password { get; set; }

    }
}
