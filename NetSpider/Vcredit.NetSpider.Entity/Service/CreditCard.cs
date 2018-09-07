using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.Service
{
    public class CreditCard
    {
        public int LoancardId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Cue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? HighestAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? UsedAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? OpenTime { get; set; }
    }
}