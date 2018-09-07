using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vcredit.WeiXin.RestService.Models
{
    public class BankCardInfo
    {
        public string BankName { get; set; }
        public string Length { get; set; }
        public string Value { get; set; }
        public string CardType { get; set; }
    }
}