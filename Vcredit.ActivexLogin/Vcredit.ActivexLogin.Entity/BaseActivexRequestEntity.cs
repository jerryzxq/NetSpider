using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vcredit.ActivexLogin.Common.ProjectEnums;

namespace Vcredit.ActivexLogin.Entity
{
    public class BaseActivexRequestEntity
    {
        public string Token { get; set; }

        public string Password { get; set; }

        public WebSiteType SiteType { get; set; }

        public string UrlParam { get; set; }
    }
}
