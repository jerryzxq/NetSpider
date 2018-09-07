using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    class HaCombineTable : ICreditSpecialWork<System.Data.DataTable>
    {
        #region  需要合并表信息的表名
        string joinTable;
        string mainTable;
        #endregion

        public HaCombineTable(string mainTable, string joinTable)
        {
            this.mainTable = mainTable;
            this.joinTable = joinTable;
        }
        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, System.Data.DataTable> dic)
        {

            if (dic.Keys.Contains(joinTable) && dic.Keys.Contains(mainTable))
            {
                DataTable mateDic = dic[joinTable];
                mateDic.Columns["姓名"].ColumnName = "Mate_Name";
                DataTable idengtityDic = dic[mainTable];
                
                foreach(DataColumn item in mateDic.Columns)
                {
                    idengtityDic.Columns.Add(item.ColumnName, item.DataType);//添加主表列名
                    idengtityDic.Rows[0][item.ColumnName] = mateDic.Rows[0][item.ColumnName];//合并值
                }
                dic.Remove(joinTable);
                dic[mainTable] = idengtityDic;
            }
            return dic;
        }
    }
}
