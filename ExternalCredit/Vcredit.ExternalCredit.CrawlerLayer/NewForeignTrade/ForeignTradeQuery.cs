using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public class ForeignTradeQuery : NewForeignTrade
    {
        protected override List<ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel.QueryResult> GetqueryQequest()
        {
            List<QueryResult> queList = new List<QueryResult>();
            foreach (var item in credit.GetSubmitCreditInfoByCofing(SysEnums.SourceType.Trade, ConfigData.BatNo))
            {

                QueryResult query = new QueryResult()
                {
                    batchNo = item.BatNo,
                    brNo = ConfigData.orgCode,
                    idNo = item.Cert_No
                };
                queList.Add(query);
            }
            return queList;
        }
    }
}
