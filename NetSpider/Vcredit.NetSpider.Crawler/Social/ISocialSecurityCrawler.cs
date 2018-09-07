using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;

namespace Vcredit.NetSpider.Crawler.Social
{
    public interface ISocialSecurityCrawler
    {
        VerCodeRes SocialSecurityInit();
        SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq fundReq);
    }
}
