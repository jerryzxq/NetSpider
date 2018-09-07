using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan
{
    public class QueryRequestMsg
    {
        public string  QueryName { get; set; }
        public string UserCode { get; set; }
        public string UserCodePw { get; set; }
        public string  OrgCode { get; set; }
        public string ReportVersion { get; set; }
        public string QueryReason { get; set; }
        public string QueryType { get; set; }
    }
    public class ScantionQueryRequestMsg : QueryRequestMsg
    {
        public string QueryCertType { get; set; }
        public string QueryCredNum { get; set; }
        public string AuthoFile { get; set; }
        public string CertFile { get; set; }



    }
    public class EleSignatureQueryRequestMsg : QueryRequestMsg
    {
        public string QueryAuthCode { get; set; }
        public string QueryAuthCodeSign { get; set; }
        public string UnionPayResult { get; set; }
        public string   AuthorTime { get; set; }

        public string QueryResponseUrl { get; set; }

    }

}
