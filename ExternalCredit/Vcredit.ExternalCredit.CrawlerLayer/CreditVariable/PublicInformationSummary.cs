using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.CreditVariable
{
    public class PublicInformationSummary
    {
        /// <summary>
        /// 行政处罚记录段
        /// </summary>
        public int AdminpnshmCount { get; set; }
        /// <summary>
        /// 民事判决记录段
        /// </summary>
        public int CiviljdgmCount { get; set; }

        /// <summary>
        /// 强制执行记录段
        /// </summary>
        public int ForceexctnCount { get; set; }

        /// <summary>
        /// 欠税记录段
        /// </summary>
        public int TaxarrearCount { get; set; }
        /// <summary>
        /// 电信缴费记录段
        /// </summary>
        public int TelpntCount { get; set; }
    }
}
