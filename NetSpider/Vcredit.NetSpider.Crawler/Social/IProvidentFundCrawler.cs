using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;

namespace Vcredit.NetSpider.Crawler.Social
{
    public interface IProvidentFundCrawler
    {
        VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq=null);
        ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq);
    }
}
