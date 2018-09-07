using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel
{
    public class ManyCreditReq:NewForeignBaseReq
    {

        /// <summary>
        /// 记录数
        /// </summary>
        public int dataCnt { get; set; }

        List<SingleCreditReq> _singleReqList = new List<SingleCreditReq>();

        public List<SingleCreditReq> singleReqList
        {
            get { return _singleReqList; }
            set { _singleReqList = value; }
        }
    }
}
