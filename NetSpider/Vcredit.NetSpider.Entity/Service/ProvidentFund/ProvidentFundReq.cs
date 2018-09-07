using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.ProvidentFund
{
    public class ProvidentFundReq:LoginReq
    {
        private string _LoginType = "1";
        public string LoginType
        {
            get { return _LoginType; }
            set { this._LoginType = value; }
        }
        /// <summary>
        /// 公积金账号
        /// </summary>
        public string FundAccount { get; set; }
        public string BusType { get; set; }
    }
}
