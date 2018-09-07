using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class LoginReq:BaseReq
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Vercode { get; set; }
        public string Smscode { get; set; }
        public string Identitycard { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
    }
}
