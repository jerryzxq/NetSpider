using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel
{
    public class QueryResult:NewForeignBaseReq
    {
        /// <summary>
        /// 证件号码
        /// </summary>
        public string  idNo { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int  pageNo { get; set; }
        /// <summary>
        /// 页数据条数
        /// </summary>
        public int pageSize { get; set; }
    }
}
