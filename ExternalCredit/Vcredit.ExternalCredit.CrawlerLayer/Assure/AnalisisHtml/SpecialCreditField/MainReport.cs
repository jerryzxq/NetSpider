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
    /// 征信报告主表信息处理
    /// </summary>
    class MainReport: ICreditSpecialWork<Element[]>
    {
        /// <summary>
        /// 处理征信报告主表
        /// </summary>
        /// <param name="dic"></param>
        /// <returns>特殊处理后的字典表</returns>
        public Dictionary<string, DataTable> DealingSpecialWork(Dictionary<string, Element[]> dic)
        {
            Dictionary<string, DataTable> dtDic = new Dictionary<string, DataTable>();
            if (dic.Keys.Contains(CommonData.creditReportTable))
            {
                var reprotElementArray = dic[CommonData.creditReportTable];
                Dictionary<string, Type> columnDatas = new Dictionary<string, Type>()
                { {"Report_Sn",typeof(string)},
                  {"Query_Time",typeof(DateTime)}, 
                  {"Report_Create_Time",typeof(DateTime)},
                  {"Name",typeof(string)}, 
                  {"Cert_Type",typeof(string)} ,
                  {"Cert_No",typeof(string)},
                  {"Query_Org",typeof(string)},
                  {"Query_Reason",typeof(string)}};

                DataTable dt = DataTableHelper.CreateDataTable(columnDatas);
                dt.Rows.Add(dt.NewRow());

                SetReportInfo(reprotElementArray[0],dt);
                dtDic.Add(CommonData.creditReportTable,SetReportUserInfo(reprotElementArray[1],dt));
                dic.Remove(CommonData.creditReportTable);
            }

            return dtDic;
        }
        private DataTable  SetReportUserInfo(Element ele, DataTable dt)
        {
     
            ele = ele.GetElementsByTag(CommonData.tbody).FirstOrDefault();
            var eles = ele.Children[1].Children;
            int index = 0;
            foreach (var item in eles)
            {
                dt.Rows[0][index + 3] = item.Text();
                index++;
            }
            return dt;
        }
        private  void  SetReportInfo(Element reprotElement, DataTable dt)
        {
            string elestr = reprotElement.Text();
            dt.Rows[0][0]=  DataTableOperator.GetfieldData(elestr,CommonData.reportsn+":",CommonData.queryTime);
            dt.Rows[0][1] =DataTableOperator.SetDefaultValue(dt.Columns[1].DataType,
                DataTableOperator.GetfieldData(elestr, CommonData.queryTime+":", CommonData.reporttime));
            dt.Rows[0][2] =DataTableOperator.SetDefaultValue(dt.Columns[2].DataType,
                elestr.Substring(elestr.IndexOf(CommonData.reporttime) + CommonData.reporttime.Length+1).Trim());
            
        }
    }
}
