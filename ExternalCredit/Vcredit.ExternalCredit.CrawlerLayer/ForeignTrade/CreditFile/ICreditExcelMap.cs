using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    interface  ICreditExcelMap
    {
        Dictionary<string, DataTable> DealingSpecialWork(Dictionary<string, DataTable> dic);

    }
}
