using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Processor.Operation.JsonModel.ProvidentFund
{
    internal class XiamenDetail
    {
        /// <summary>
        /// 状态
        /// </summary>
        public string centSumy { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string centDealDate { get; set; }
        /// <summary>
        ///余额
        /// </summary>
        public string bal { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public string creditAmt { get; set; }
    }
}
