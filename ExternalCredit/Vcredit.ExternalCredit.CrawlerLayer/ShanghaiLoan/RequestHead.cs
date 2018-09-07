using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan
{
    public class RequestHead
    {

        public string VersionID { get; set; }
        public string Sender { get; set; }
        public string  SenderID { get; set; }
        public string  Receiver { get; set; }
        public string ReceiverID  { get; set; }
        public string SendDate { get; set; }
        public string SendTime { get; set; }
        public string MesgID{ get; set; }
        public string  MesgRefID { get; set; }
        public string  TradeType { get; set; }


    }
}
