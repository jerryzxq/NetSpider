using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Vcredit.NetSpider.Entity.Service
{
    public class TaobaoSellerRes : BaseRes
    {
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public int ItemCount { get; set; }
        public double UseMinute { get; set; }
        public DateTime? OpenTime { get; set; }
        public decimal GoodRate { get; set; }
        public string CompanyName { get; set; }
        private List<TaobaoSellerMS> _MonthStatistics = new List<TaobaoSellerMS>();
        public List<TaobaoSellerMS> MonthStatistics
        {
            get { return _MonthStatistics; }
            set { this._MonthStatistics = value; }
        }
    }
}