using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    interface ICreditSpecialWork<T>
    {
        Dictionary<string, DataTable> DealingSpecialWork(Dictionary<string, T> dic);

    }
}
