using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Processor.Operation.JsonModel.ProvidentFund
{
    internal class NanJingDetail
    {
        /// <summary>
        /// 单位名称
        /// </summary>
        public string unitaccname { get; set; }
        /// <summary>
        /// 交易日期
        /// </summary>
        public string transdate { get; set; }
        /// <summary>
        /// 缴费说明（有公积金所属年月）
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        ///余额
        /// </summary>
        public string payvouamt { get; set; }
        /// <summary>
        /// 缴费金额
        /// </summary>
        public string basenum { get; set; }
    }
}
