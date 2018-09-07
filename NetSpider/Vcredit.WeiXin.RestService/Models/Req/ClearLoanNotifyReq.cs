using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    class ClearLoanNotifyReq
    {
        public int guid { get; set; }
        public string status { get; set; }
        public string description { get; set; }
    }
}
