using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vcredit.ExtTrade.CommonLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    /// <summary>
    /// 一般处理：除贷记卡和征信报告主表信息
    /// </summary>
    class NomalDealing : ICreditSpecialWork<Element[]>
    {
        //当前操作的标题栏（相当于数据库中的一个table）
        string tableName;
        /// <summary>
        /// 把从征信获取的元素转化为datatable
        /// </summary>
        /// <param name="eleArr"></param>
        /// <returns></returns>
        private DataTable ConvertElementArrToDataTable(Element[] eleArr)
        {
            int index = 0;
            //key: 标题行的索引，value 行的所有标题多用逗号隔开
            Dictionary<int, List<string>> columnNames = new Dictionary<int, List<string>>();
            List<List<string>> columnDatas = new List<List<string>>();
         
            foreach (Element ele in eleArr)
            {
                var eleChildren = ele.GetElementsByTag(CommonData.tbody)[0].Children;//tds
                foreach (var eleCol in eleChildren)
                {
                    List<string> list = new List<string>();
                    var bTab = eleCol.GetElementsByTag(CommonData.b);
                    foreach (var eleitem in eleCol.Children)
                    {
                        list.Add(eleitem.Text().Trim());
                    }

                    if (bTab != null && bTab.Count != 0)//说明是列
                    {
                        columnNames.Add(index, list);
                    }
                    else
                    {
                        columnDatas.Add(list);
                    }
                    index++;

                }
                index = 0;
            }
            return GetDataTable(columnNames, columnDatas);
        }
        /// <summary>
        /// 把从征信获取的列和行转化为Datatable
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="columnDatas"></param>
        /// <returns></returns>
        private DataTable GetDataTable(Dictionary<int, List<string>> columnNames, List<List<string>> columnDatas)
        {
            DataTable dt = new DataTable();
            if (columnNames.Count == 1)//一表头多行的数据
            {
                dt = DataTableOperator.CreateDataTableByCol(tableName, columnNames.First().Value, columnDatas);
            }
            else if (columnNames.Count == 2 && columnNames.Keys.First() + 1 == columnNames.Keys.Last())//汇总表（查询记录汇总和逾期信息汇总）
            {
                List<string> columnNameList = new List<string>();
                for (int i = 0; i < columnNames[1].Count; i++)
                {
                    columnNameList.Add(i.ToString());
                }
                dt = DataTableOperator.CreateDataTableByCol(tableName, columnNameList, columnDatas);
            }
            else//多表头多行数据
            {
                List<string> columnList;
                List<List<string>> realColumnDatas;
                GetTableData(columnNames, columnDatas, out columnList, out realColumnDatas);
                dt = DataTableOperator.CreateDataTableByCol(tableName, columnList, realColumnDatas);
            }
            return dt;
        }
        /// <summary>
        /// 把从html征信获取的原始列和数据行合并成一列多行的数据
        /// </summary>
        /// <param name="columnNames">列名集合</param>
        /// <param name="columnDatas">数据行集合</param>
        /// <param name="columnList">输出列</param>
        /// <param name="realColumnDatas">输出行</param>
        private void GetTableData(Dictionary<int, List<string>> columnNames, List<List<string>> columnDatas,out List<string> columnList, out List<List<string>> realColumnDatas)
        {
            columnList = new List<string>();
            realColumnDatas = new List<List<string>>();
            if(columnNames.Count==0)//如果没有获取到列
            {
                string[] namelist = new string[columnDatas.First().Count];
                columnDatas.First().CopyTo(namelist);
                columnNames.Add(0, namelist.ToList());
                columnDatas.RemoveAt(0);

            }                
            int rowNum = columnDatas.Count / columnNames.Count;
            var columnNamesValues = columnNames.Values.ToList();
            bool havenum = columnNamesValues[0][0] == CommonData.num;
            int beginindex = havenum ? 1 : 0;
            //合并列
            for (int i = 0; i < columnNames.Count; i++)
            {
                for (int index = beginindex; index < columnNamesValues[i].Count; index++)
                {
                    columnList.Add(columnNamesValues[i][index]);
                }
            }
            //合并数据行
            for (int i = 0; i < rowNum; i++)
            {
                List<string> list = new List<string>();
                for (int index = i; index < columnDatas.Count; index += rowNum)
                {
                    if (havenum)//如果有编号去掉第一个数据
                    {
                        columnDatas[index].RemoveAt(0);//
                    }
                    list.AddRange(columnDatas[index]);
                }
                realColumnDatas.Add(list);
            }
        }

        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, Element[]> dic)
        {
            Dictionary<string, System.Data.DataTable> dtdic = new Dictionary<string, DataTable>();

            int index = 0;
            foreach (var item in dic)
            {
                tableName = item.Key;//当前操作的表
              
                dtdic.Add(item.Key, ConvertElementArrToDataTable(item.Value));
                index++;
            }
            return dtdic;

        }
    }
}
