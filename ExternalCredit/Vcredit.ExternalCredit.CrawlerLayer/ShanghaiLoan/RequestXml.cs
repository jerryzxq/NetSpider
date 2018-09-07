using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan
{
    [XmlRoot("CFX")]
   public class RequestXml
    {
        public RequestHead HEAD { get; set; }
        public RequestMsg MSG { get; set; }
    }
}
