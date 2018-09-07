using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class JxlSubmitInfo
    {
        public string id { get; set; }
        public int orderid { get; set; }
        public string name { get; set; }
        public string identitycard { get; set; }
        public string mobile { get; set; }
        public string busType { get; set; }
        public string busId { get; set; }
    }
}
