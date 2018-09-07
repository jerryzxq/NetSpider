using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public  class Message
    {
        List<NewForeignContainer> creditData;

        public List<NewForeignContainer> CreditDataList
        {
            get { return creditData; }
            set { creditData = value; }
        }

        List<ErrorData> errorData;

        public List<ErrorData> ErrorDataList
        {
            get { return errorData; }
            set { errorData = value; }
        }
    }


}
