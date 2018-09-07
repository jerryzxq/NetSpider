using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    class HaMixinType : ICreditSpecialWork<DataTable>
    {
        string invokeTable;
        string[] types;
        Dictionary<string, Type> columnDatas;

        public HaMixinType(string invokeTable, string[] types, Dictionary<string, Type> columnDatas)
        {
            this.invokeTable = invokeTable;
            this.types = types;
            this.columnDatas = columnDatas;

        }
        void CreateDrForOveDueTable(DataTable oveDueTable, DataRow item)
        {
            try
            {
                int index = 0;
                int columnCount = columnDatas.Count;
                foreach (string typeItem in types)
                {
                    DataRow dr = oveDueTable.NewRow();
                    int i = 0;
                    for (i = 0; i < columnCount - 1; i++)
                    {
                        if (oveDueTable.Columns[i].DataType == typeof(decimal))
                        {
                            string value = item[index].ToString().Trim();
                            if (value == string.Empty || value == CommonData.doubleTrunk || value == CommonData.nullstr)
                            {
                                dr[i] = DBNull.Value;
                            }
                            else
                            {
                                dr[i] = value;
                            }
                        }
                        else
                        {
                            dr[i] = item[index];
                        }
                        index++;
                    }
                    dr[i] = typeItem;
                    oveDueTable.Rows.Add(dr);
                }
            }
            catch (Exception)
            {
                
                throw;
            }
          

        }
        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, System.Data.DataTable> dic)
        {
            if (dic.Keys.Contains(invokeTable))
            {
                DataTable dt = dic[invokeTable];
                DataTable oveDueTable = DataTableHelper.CreateDataTable(columnDatas);
                DataRow[] drs = dt.Select();
                foreach (var item in drs)
                {
                    CreateDrForOveDueTable(oveDueTable, item);
                }
                dic[invokeTable] = oveDueTable;
            }
            return dic;

        }
    }
}
