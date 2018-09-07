using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    class DealingExcelData
    {

        internal OperatorLog operatorLog = null;
        private string fileName;
        internal string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        public DealingExcelData(OperatorLog operatorLog)
        {
            this.operatorLog = operatorLog;
        }
        #region 把excel数据分割成多个datatable
        /// <summary>
        /// 从excel读取到的Datatable进行分表
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        internal Dictionary<string, DataTable> SpliteTable(DataTable dt)
        {
            Dictionary<string, DataTable> saveDealDIc = new Dictionary<string, DataTable>();//保存处理后的数据
            List<DataRow> dicvalueList = new List<DataRow>();
            string tableMark = string.Empty;
            int markIndex = 0;
            int index = 0;
            int maxColumnNum = dt.Columns.Count;
            string key = string.Empty;
            int dtRows = dt.Rows.Count;
            foreach (var item in dt.Select())
            {
                if (item[1].ToString().Trim() == string.Empty || dtRows == index + 1)//是标题或者最后一行
                {
                    markIndex++;
                    if (index > 0 && markIndex != 2)//是小标题且不是第一行
                    {
                        if (dtRows == index + 1)//最后一行
                        {
                            dicvalueList.Add(item);
                            if(!saveDealDIc.ContainsKey(key))
                            saveDealDIc.Add(key, CombineDataRowToTable(dicvalueList, key, maxColumnNum));
                            break;
                        }
                        else
                        {
                            saveDealDIc.Add(key, CombineDataRowToTable(dicvalueList, key, maxColumnNum));//保存标题和相应的数据
                        }

                        key = string.Empty;
                        dicvalueList.Clear();
                    }
                }
                //是小标题
                if ((markIndex == 1 && dt.Rows[index + 1][1].ToString() != string.Empty) || markIndex == 2 || index == 0)
                {
                    markIndex = 0;
                    key = item[0].ToString().Trim();
                }
                else if (markIndex == 0)//数据行
                {
                  
                    dicvalueList.Add(item);
                }
                index++;

            };
            return SpecialTableTreatment(saveDealDIc);
        }
        DataTable CombineDataRowToTable(List<DataRow> drList, string tableName, int maxcolumnNum)
        {
            StringBuilder error = new StringBuilder();
            List<DataColumn> dataColumns = new List<DataColumn>();
            string columnName = string.Empty;

            //获取列
            DataTable dt = CreateColumns(drList, tableName, maxcolumnNum, dataColumns);

            //获取行
            CreateDataRow(drList, tableName, error, dataColumns, dt);
            if (error.Length != 0)
            {
                throw new Exception(error.ToString());
            }
            return dt;
        }

        DataTable CreateColumns(List<DataRow> drList, string tableName, int maxcolumnNum, List<DataColumn> dataColumns)
        {
          
            DataTable dt = new DataTable();
            var columns = drList[0];
            string fname = string.Empty;
            IEnumerable<PropertyInfo> pinfos = null;
            string columnName = string.Empty;
            //获取列
            for (int i = 0; i < maxcolumnNum; i++)
            {
                columnName = columns[i].ToString().Trim();
                if (columnName == "")
                {
                    break;
                }           
                fname =TableMapData.GetFileName(tableName, columnName);
                if (MappingModel.proInfosDic.Keys.Contains(tableName))
                {
                    pinfos = MappingModel.proInfosDic[tableName].Where(item => item.Name == fname);//判断是否有相应的类型
                }
                if (pinfos != null && pinfos.Count() != 0)
                {                    
                    dataColumns.Add(new DataColumn(fname, pinfos.FirstOrDefault().PropertyType));
                }
                else
                {
                    dataColumns.Add(new DataColumn(fname));
                }

            }
            dt.Columns.AddRange(dataColumns.ToArray());
            return dt;
        }

        void CreateDataRow(List<DataRow> drList, string tableName, StringBuilder error, List<DataColumn> dataColumns, DataTable dt)
        {
            int columnNum = dataColumns.Count;

            for (int i = 1; i < drList.Count; i++)
            {
                var dr = dt.NewRow();
                for (int cln = 0; cln < columnNum; cln++)
                {
                  
                    if (dataColumns[cln].DataType == typeof(string))
                    {
                        dr[cln] = drList[i][cln];
                    }
                    else
                    {
                        var tuple = SetDefaultValue(dataColumns[cln].DataType, drList[i][cln]);//验证时间类型和数值类型
                        if (tuple.Item2 == string.Empty)
                        {
                            dr[cln] = tuple.Item1;
                        }
                        else
                        {
                            error.Append("fileName:"+FileName+",tableName:" + tableName + ",columnName:" + dataColumns[cln].ColumnName + tuple.Item2);
                            operatorLog.AddfailReson(new FailReason(FileName, tableName, "第" + i.ToString() + "行", dataColumns[cln].ColumnName, tuple.Item2));
                 
                        }
                    }
                }
                dt.Rows.Add(dr);
            }
           
        }

        #endregion

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>值，验证信息</returns>
        Tuple<object, string> SetDefaultValue(Type type, object value)
        {
            Tuple<object, string> tuple = new Tuple<object, string>(value, string.Empty);
            string valStr = value.ToString().Trim();
            if (valStr.Equals(CommonData.nullstr, StringComparison.CurrentCultureIgnoreCase) || valStr == string.Empty || valStr == CommonData.doubleTrunk)
            {
                tuple = new Tuple<object, string>(DBNull.Value,string.Empty);
            }
            else if (type == typeof(int) || type == typeof(decimal))
            {
                int val = 0;
                tuple = int.TryParse(value.ToString(), out val) ?
                 new Tuple<object, string>(value, string.Empty) :
                 new Tuple<object, string>(value,value+"数字转换失败");

            }
            else if (type == typeof(DateTime))
            {    
                 var valstr= value.ToString().Trim();
                 if(Regex.IsMatch(value.ToString(),"^[0-9]*$"))
                 {
                    tuple=  new Tuple<object, string>(DateTime.FromOADate(Convert.ToInt32(valstr)).ToString("d"), string.Empty);
                 }
                 else
                 {
                     DateTime dt = CommonData.defaultTime;
                     tuple = DateTime.TryParse(value.ToString(), out dt) ?
                        new Tuple<object, string>(value, string.Empty) :
                        new Tuple<object, string>(value, value + "时间转换失败");
                 }
            }
            return tuple;

        }
      
        /// <summary>
        /// 特殊处理
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, DataTable> SpecialTableTreatment(Dictionary<string, DataTable> saveDealDIc)
        {
           
            List<ICreditExcelMap> specialWork = new SpecialWork().list;
            foreach (var item in specialWork)
            {
                item.DealingSpecialWork(saveDealDIc);
            }
            return saveDealDIc;
        }



    }
}
