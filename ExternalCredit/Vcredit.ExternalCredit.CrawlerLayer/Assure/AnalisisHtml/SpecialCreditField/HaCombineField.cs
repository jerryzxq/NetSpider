using NSoup.Nodes;
using NSoup.Select;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExtTrade.CommonLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{

    /// <summary>
    /// 贷记卡,准贷记卡，贷款。信息处理
    /// </summary>
    class HaCombineField : ICreditSpecialWork<Element[]>
    {
        Element ele;//当前操作的表格元素，如果为null 表示不是表格元素
        DataTable loadDataTable;//当前操作的datatable 
        DataTable specialTable;//当前操作的特殊交易 
        DataTable ovedueTable;//当前操作的逾期交易 
        DataTable diffTable;//当前操作的异议处理
        DataTable ANCINFO;//本人声明
        string tableName;//当前操作的标题名（相对于数据库里的一个表）
        Dictionary<string, string> cartNamefileDic = new Dictionary<string, string>();//Cart表字段字典
        Dictionary<string, string> specialNamefileDic = new Dictionary<string, string>();//特殊交易表字段字典
        Dictionary<string, string> oveDuenamefileDic = new Dictionary<string, string>();//逾期透支记录表字段字典
        Dictionary<string, string> diffNamefileDic = new Dictionary<string, string>();//异议处理信息
        Dictionary<string, string> ANCINFOfileDic = new Dictionary<string, string>();//本人声明
        public Dictionary<string, DataTable> DealingSpecialWork(Dictionary<string, Element[]> dic)
        {
            Dictionary<string, DataTable> dicData = new Dictionary<string, DataTable>();
            ContertElementArrToDataTable(dic, dicData, CommonData.CRD_CD_STNCARD);

            ContertElementArrToDataTable(dic, dicData, CommonData.CRD_CD_LN);
            ContertElementArrToDataTable(dic, dicData, CommonData.CRD_CD_LND);
            if (diffTable != null && diffTable.Rows.Count != 0)
            {
                dicData.Add(CommonData.CRD_AN_DSTINFO, diffTable);
            }
            if (ANCINFO != null && ANCINFO.Rows.Count != 0)
            {
                dicData.Add(CommonData.CRD_AN_ANCINFO, ANCINFO);
            }
            return dicData;

        }
        private void ContertElementArrToDataTable(Dictionary<string, NSoup.Nodes.Element[]> dic, Dictionary<string, DataTable> dicData, string title)
        {
            if (dic.Keys.Contains(title))
            {
                tableName = title;
                CombineFiletoPayment_State(dic[title]);

                dicData.Add(tableName, LoadDataTable.Copy());
                if (specialTable != null && specialTable.Rows.Count != 0)
                {
                    dicData.Add(tableName + CommonData.specialTradeStr, specialTable.Copy());
                }
                if (ovedueTable != null && ovedueTable.Rows.Count != 0)
                {
                    dicData.Add(tableName + CommonData.overDueStr, ovedueTable.Copy());
                }
                dic.Remove(title);
                loadDataTable = null;
                if (specialTable != null)
                    specialTable.Clear();
                if (ovedueTable != null)
                    ovedueTable.Clear();
                cartNamefileDic.Clear();
            }
        }

        private DataTable LoadDataTable
        {
            get
            {
                if (loadDataTable == null)
                {
                    loadDataTable = DataTableOperator.GetTable(tableName, cartNamefileDic);
                    loadDataTable.Columns.Add(new DataColumn("编号", typeof(int)) { DefaultValue = 0 });//添加一个编号
                }
                return loadDataTable;
            }
        }
        private DataTable SpecialTable
        {
            get
            {
                if (specialTable == null)
                {
                    specialTable = DataTableOperator.GetTable(CommonData.specialTradeStr, specialNamefileDic);
                    specialTable.Columns.Add(new DataColumn("编号", typeof(int)) { DefaultValue = 0 });//对应卡的外键
                }
                return specialTable;
            }
        }
        private DataTable _ANCINFO
        {
            get
            {
                if (ANCINFO == null)
                {
                    ANCINFO = DataTableOperator.GetTable(CommonData.CRD_AN_ANCINFO, ANCINFOfileDic);
                }
                return ANCINFO;
            }
        }
        private DataTable DiffTable
        {
            get
            {
                if (diffTable == null)
                {
                    diffTable = DataTableOperator.GetTable(CommonData.CRD_AN_DSTINFO, diffNamefileDic);
                }
                return diffTable;
            }
        }
        private DataTable OvedueTable
        {
            get
            {
                if (ovedueTable == null)
                {
                    ovedueTable = DataTableOperator.GetTable(CommonData.overDueStr, oveDuenamefileDic);
                    ovedueTable.Columns.Add(new DataColumn("编号", typeof(int)) { DefaultValue = 0 });//对应卡的外键
                }
                return ovedueTable;
            }
        }

        /// <summary>
        /// 解析贷记卡记录标题字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private void GetDataFromStr(Element strEle)
        {
            DataRow dr = null;
            string str = strEle.Text();
            if (ele != null)
            {
                //获取贷记卡表格数据
                dr = CreateDataRow();
            }
            if (dr == null)
                dr = LoadDataTable.NewRow();
            //获取贷记卡表格上面的数据
            CreateStrData(str, dr);
            LoadDataTable.Rows.Add(dr);

        }
        private void CreateStrData(string str, DataRow dr)
        {

            GetValue(dr, "Open_Date", GetOpenDate(str));
            GetValue(dr, "Finance_Org", GetfieldData(str, "“", "”发放的"));
            GetValue(dr, "Currency", GetfieldData(str, "（", "）").ToString().TrimEnd("账户".ToCharArray()));
            GetValue(dr, "Account_Dw", GetfieldData(str, "业务号", "，"));
            var value = GetfieldData(str, "授信额度", "元");
            if (value == DBNull.Value)
            {
                GetValue(dr, "Credit_Limit_Amount", GetfieldData(str, "发放的", "元（"));
            }
            else
                GetValue(dr, "Credit_Limit_Amount", value.ToString().TrimStart("折合人民币".ToCharArray()));
            GetValue(dr, "Guarantee_Type", GetGuarantee_Type(str, "担保"));
            GetValue(dr, "Type_Dw", GetType_Dw(str));//贷款种类
            GetValue(dr, "State", GetfieldData(str, "账户状态为“", "”"));
            var values = GetfieldData(str, "截至", "，");
            if (values == DBNull.Value)
                GetValue(dr, "GetTime", DBNull.Value);
            else
                GetValue(dr, "GetTime", values.ToString().Replace("年", "-").Replace("月", "-").Replace("日", ""));
            var val = GetfieldData(str, "共享授信额度", "元");
            if (val == DBNull.Value)
                GetValue(dr, "Share_Credit_Limit_Amount", DBNull.Value);
            else
                GetValue(dr, "Share_Credit_Limit_Amount", val.ToString().TrimStart("折合人民币".ToCharArray()));

            GetValue(dr, "Payment_Cyc", GetLoanDes(str, "期，", "期，"));
            GetValue(dr, "Payment_Rating", GetLoanDes(str, "归还"));
            var res = GetLoanDes(str, "日到期", "日到期");
            if (res == DBNull.Value)
                GetValue(dr, "End_Date", DBNull.Value);
            else
                GetValue(dr, "End_Date", res.ToString().Replace("年", "-").Replace("月", "-"));
            GetValue(dr, "Cue", str);

        }
        object GetLoanDes(string str, string lastStr, string trimStr = null)
        {
            var result = GetGuarantee_Type(str, lastStr);
            if (result == DBNull.Value || trimStr == null)
                return result;
            else
            {
                var res = result.ToString();
                if (res.LastIndexOf(trimStr) != -1)
                    return res.Remove(res.LastIndexOf(trimStr));
                else
                    return DBNull.Value;
            }

        }
        object GetType_Dw(string str)
        {
            var value = GetfieldData(str, "）", "贷款");
            if (value == DBNull.Value)
            {
                return DBNull.Value;
            }
            else
            {
                return value + "贷款";
            }
        }
        object GetOpenDate(string str)
        {
            DateTime dt = DateTime.Now;
            var opendate = str.Substring(str.IndexOf('.') + 1, 11).Replace("年", "-").Replace("月", "-").Replace("日", "");
            if (DateTime.TryParse(opendate, out dt))
                return opendate;
            return DBNull.Value;


        }
        object GetGuarantee_Type(string str, string endUnitStr)
        {
            int index = str.IndexOf(endUnitStr);
            if (index == -1)
            {
                if (endUnitStr == "担保")
                {
                    index = str.IndexOf("保证");
                    if (index == -1)
                    {
                        index = str.IndexOf("保");
                        if (index == -1)
                            return DBNull.Value;
                        else
                            endUnitStr = "保";
                    }
                }
                else
                {
                    return DBNull.Value;
                }

            }
            int lastindex = str.Substring(0, index).LastIndexOf("，");
            return str.Substring(lastindex + 1, index - lastindex + endUnitStr.Length - 1);
        }


        void GetValue(DataRow dr, string columnName, object value)
        {
            if (LoadDataTable.Columns.Contains(columnName) && dr[columnName] == DBNull.Value)
            {
                if (value == DBNull.Value)
                {
                    dr[columnName] = DBNull.Value;
                }
                else
                {
                    dr[columnName] = DataTableOperator.SetDefaultValue(LoadDataTable.Columns[columnName].DataType, value.ToString());
                }
            }
        }
        object GetfieldData(string loadStr, string start, string end)
        {
            string value = Commonfunction.GetMidStr(loadStr, start, end);
            if (value == "")
                return DBNull.Value;
            return value;

        }

        private DataRow CreateDataRow()
        {
            DataRow dr = null;
            var eles = ele.GetElementsByTag("tbody")[0].Children;
            dr = LoadDataTable.NewRow();
            int num = LoadDataTable.Rows.Count + 1;
            dr["编号"] = num;//编号
            var titleEle = eles[0].Text();
            if (titleEle.EndsWith("逾期记录") || titleEle.EndsWith("透支记录") || titleEle.StartsWith("特殊交易"))//只包含逾期或者特殊交易
            {
                DealingCartSpecialAndTradeInfo(eles, num);
                return dr;
            }
            int[] dataIndex = { 1, 3 };//1,3行是数据列
            foreach (var index in dataIndex)
            {
                if (eles.Count - 1 < index)
                    break;
                int chIndex = 0;
                foreach (var eleItem in eles[index].Children)
                {

                    var eletxt = eles[index - 1].Children[chIndex].Text().Replace(" ", "");
                    if (eletxt.EndsWith("还款记录") || eletxt.EndsWith("缴费记录"))
                    {
                        dr["Payment_State"] = eles[index].Text().Replace(" ", "");//还款记录，单独处理。
                        break;
                    }
                    else if (cartNamefileDic.ContainsKey(eletxt))
                    {
                        dr[cartNamefileDic[eletxt]] = DataTableOperator.SetDefaultValue(LoadDataTable.Columns[cartNamefileDic[eletxt]].DataType, eleItem.Text().Replace(",", ""));
                    }
                    chIndex++;

                }


            }
            if (dr["Payment_State"] == DBNull.Value && eles.Count - 1 >= 5)
                dr["Payment_State"] = eles[5].Text().Replace(" ", "");//第5行数据是还款记录，单独处理。
            AddSpecailAndOveDueInfo(eles, num);//添加卡的逾期和特殊交易信息。
            return dr;
        }
        private void AddSpecailAndOveDueInfo(Elements eles, int num)
        {
            int recordIndex = 0;
            foreach (var item in eles)
            {
                if (item.Text().EndsWith("还款记录"))
                {
                    recordIndex++;
                    break;
                }
                recordIndex++;
            }
            if (recordIndex < eles.Count - 1)
            {
                Elements list = new Elements();
                for (int i = recordIndex + 1; i < eles.Count; i++)//获取除卡信息以外的信息
                {
                    list.Add(eles[i]);
                }
                DealingCartSpecialAndTradeInfo(list, num);
            }
        }
        private void GetCRD_AN_DSTINFO(Elements eles)
        {
            if (!ele.Text().Contains("本人声明"))
                return;
            List<Element> eleList = new List<Element>();
            int markIndex = 0;
            Element titleele = null;
            foreach (var item in eles)
            {
                if (item.Text().StartsWith("本人声明"))
                {
                    titleele = item;
                    markIndex++;
                    continue;
                }
                else if(markIndex==0)
                    continue;
                var title= item.Select("b").FirstOrDefault() ;
                if (markIndex != 0 && title== null)
                {
                    eleList.Add(item);
                }
                else if (title!=null)
                {
                    break; 
                }
            }
            if (eleList.Count != 0)
            {
                string[] FileNames = { "声明内容", "添加日期" };
                for (int i = 0; i < eleList.Count; i++)
                {
                    var dr = _ANCINFO.NewRow();
                    int index = 0;
                    foreach (var item in FileNames)
                    {
                        dr[ANCINFOfileDic[item]] = DataTableOperator.SetDefaultValue(_ANCINFO.Columns[ANCINFOfileDic[item]].DataType, eleList[i].Children[index].Text());
                        index++;
                    }
                    _ANCINFO.Rows.Add(dr);
                }
            }
            eles.RemoveAll(eleList);//移除本人声明元素；
            if (titleele != null)
                eles.Remove(titleele);
        }
   
        private void DealingCartSpecialAndTradeInfo(Elements eles, int num)
        {
            GetCRD_AN_DSTINFO(eles);//处理本人声明信息
            List<Element> specialList = new List<Element>();
            List<Element> ovedueList = new List<Element>();
            List<Element> diffList = new List<Element>();
            if (eles.Count == 0)
                return;
            var titleele = eles[0].Text();
            if (titleele.EndsWith("逾期记录") || titleele.EndsWith("透支记录"))//包含特殊类型的解析
            {
                eles.Remove(eles.First);//去掉标题
                int index = 0;
                int mark = 0;
                int diffMark = 0;
                foreach (Element item in eles)
                {
                    if (item.Text().StartsWith("特殊交易"))
                    {
                        mark = index;
                        specialList.Add(item);
                        continue;
                    }
                    if (item.Text().StartsWith("异议标注"))
                    {
                        diffMark = index;
                        continue;
                    }
                    if (diffMark != 0)
                    {
                        diffList.Add(item);
                        continue;
                    }
                    if (mark != 0)
                    {
                        specialList.Add(item);
                        continue;
                    }
                    ovedueList.Add(item);
                    index++;
                }

            }
            else if (titleele.StartsWith("特殊交易"))//只有特殊类型的解析
            {
                int diffMark = 0;
                foreach (var item in eles)
                {
                    if (item.Text().StartsWith("异议标注"))
                    {
                        diffMark = 1;
                        continue;
                    }
                    if (diffMark != 0)
                    {
                        diffList.Add(item);
                        continue;
                    }
                    specialList.Add(item);
                }

            }
            else if (titleele.StartsWith("异议标注"))
            {
                eles.Remove(eles[0]);
                diffList.AddRange(eles);
            }
            if (specialList.Count != 0)
                AddDataToTable(specialList, SpecialTable, specialNamefileDic, num);
            if (ovedueList.Count != 0)
                AddDataToTable(ovedueList, OvedueTable, oveDuenamefileDic, num);
            if (diffList.Count != 0)
            {
                string[] FileNames = { "标注内容", "添加日期" };
                for (int i = 0; i < diffList.Count; i++)
                {
                    var dr = DiffTable.NewRow();
                    int index = 0;
                    foreach (var item in FileNames)
                    {
                        dr[diffNamefileDic[item]] = DataTableOperator.SetDefaultValue(DiffTable.Columns[diffNamefileDic[item]].DataType, diffList[i].Children[index].Text());
                        index++;
                    }
                    DiffTable.Rows.Add(dr);
                }
            }

        }
        private void AddDataToTable(List<Element> eleList, DataTable table, Dictionary<string, string> columnDic, int num)
        {
            int colNum = eleList[0].Children.Count;
            if (colNum == 5)
            {
                SpecialTrade(eleList, table, columnDic, num);
            }
            else if (colNum == 6)
            {
                OveDueTrade(eleList, table, columnDic, num);
            }

        }
        private static void OveDueTrade(List<Element> eleList, DataTable table, Dictionary<string, string> columnDic, int num)
        {

            string[] colNameArr = new string[3];
            for (int i = 0; i < 3; i++)
            {
                var name = eleList[0].Children[i].Text();
                colNameArr[i] = name.Substring(name.Length - 2, 2);
            }

            for (int i = 1; i < eleList.Count; i++)
            {
                var dr = table.NewRow();
                var dr1 = table.NewRow();
                int index = 0;
                if (eleList[i].Children.Count != 6)
                    continue;
                foreach (var item in colNameArr)
                {
                    dr[columnDic[item]] = DataTableOperator.SetDefaultValue(table.Columns[columnDic[item]].DataType, eleList[i].Children[index].Text());
                    index++;
                }
                foreach (var item in colNameArr)
                {
                    dr1[columnDic[item]] = DataTableOperator.SetDefaultValue(table.Columns[columnDic[item]].DataType, eleList[i].Children[index].Text());
                    index++;
                }

                dr["编号"] = num;
                dr1["编号"] = num;
                table.Rows.Add(dr);
                table.Rows.Add(dr1);
            }
        }

        private static void SpecialTrade(List<Element> eleList, DataTable table, Dictionary<string, string> columnDic, int num)
        {

            for (int i = 1; i < eleList.Count; i++)
            {
                var dr = table.NewRow();
                int index = 0;
                foreach (var item in eleList[0].Children)
                {
                    var text = item.Text();
                    dr[columnDic[text]] = DataTableOperator.SetDefaultValue(table.Columns[columnDic[text]].DataType, eleList[i].Children[index].Text());
                    index++;
                }
                dr["编号"] = num;
                table.Rows.Add(dr);
            }
        }


        void CombineFiletoPayment_State(Element[] element)
        {
            int length = element.Length;
            for (int i = 0; i < length; i++)
            {
                if (i == length - 1)//以字符串处理
                {
                    ele = null;
                    GetDataFromStr(element[i]);
                }
                else
                {
                    var table = element[i + 1].GetElementsByTag(CommonData.table);
                    if (table != null && table.Count != 0)//是表格
                    {
                        ele = element[i + 1];
                        GetDataFromStr(element[i]);
                        i += 1;
                    }
                    else
                    {
                        ele = null;
                        GetDataFromStr(element[i]);
                    }
                }
            }
        }


    }
}
