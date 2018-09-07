using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public class ErrorData
    {
        string cert_NO;

        public string Cert_NO
        {
            get { return cert_NO; }
            set { cert_NO = value; }
        }
        string cert_Type;

        public string Cert_Type
        {
            get { return cert_Type; }
            set { cert_Type = value; }
        }
        string errorCode;

        public string ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }
        string errorDes;

        public string ErrorDes
        {
            get { return errorDes; }
            set { errorDes = value; }
        }
    }
}
