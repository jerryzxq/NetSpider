using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.SocialSecurity
{
    public class SocialSecurityReq:LoginReq
    {
        private string _LoginType = "1";
        public string LoginType
        {
            get { return _LoginType; }
            set { this._LoginType = value; }
        }
        public string Citizencard { get; set; }
        public string CompanyName { get; set; }
        public string BusType { get; set; }
    }
}
