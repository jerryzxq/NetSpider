using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    class MixinType :ICreditExcelMap 
    {
        string   invokeTable ;
        string[] types;
        Dictionary<string, Type> columnDatas;

        public MixinType(string invokeTable, string[] types, Dictionary<string, Type> columnDatas)
        {
            this.invokeTable = invokeTable;
            this.types = types;
            this.columnDatas = columnDatas;

        }
        void  CreateDrForOveDueTable(DataTable oveDueTable, DataRow item)
        {
            int index = 2;
            int columnCount = columnDatas.Count;
            foreach (string typeItem in types)
            {
                DataRow dr = oveDueTable.NewRow();
                int i=0;
                for (i = 0; i < columnCount-1; i++)
                {
                    if(oveDueTable.Columns[i].DataType==typeof(decimal))
                    {
                        string value=item[index].ToString().Trim();
                        int intParseValue=0;
                        if( value==string.Empty||value==CommonData.doubleTrunk||value==CommonData.nullstr)
                        {
                            dr[i] = CommonData.defaultNumValue;
                        }
                        else if(int.TryParse(value,out intParseValue))
                        {
                            dr[i] = value;
                        }
                        else
                        {
                            throw new Exception(invokeTable + "：value值转换为数值失败");
                        }

                    }                       
                    else 
                    {
                        dr[i] = item[index];
                    }
                    index ++;
                }
                dr[i] = typeItem;
                oveDueTable.Rows.Add(dr);
            }        
           
        }
        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, System.Data.DataTable> dic)
        {
            if(dic.Keys.Contains(invokeTable))
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
