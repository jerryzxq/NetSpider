using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    class CombineTable:ICreditExcelMap
    {
        #region  需要合并表信息的表名
        string joinTable;
        string mainTable;
        #endregion

        public CombineTable (string mainTable,string joinTable)
        {
            this.mainTable = mainTable;
            this.joinTable = joinTable;
        }
        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, System.Data.DataTable> dic)
        {

            if (dic.Keys.Contains(joinTable) && dic.Keys.Contains(mainTable))
            {
                DataTable mateDic = dic[joinTable];
                DataTable idengtityDic = dic[mainTable];
                DataTable dt = DataTableHelper.Join(idengtityDic, mateDic, idengtityDic.Columns[0], mateDic.Columns[0]);
                dic.Remove(joinTable);
                dic[mainTable] = dt;
            }

            return dic;
        }
    }
}
