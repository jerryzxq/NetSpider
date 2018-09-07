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
    class CombineField : ICreditExcelMap
    {
        DataTable CombineFiletoPayment_State(DataTable dt, bool isTELPNT = false)
        {
            if (isTELPNT)
            {
                dt.Columns.Add("Status24");
            }
            else
            {
                dt.Columns.Add("Payment_State");
            }
            int rownum = dt.Rows.Count;
            int colNum = dt.Columns.Count;
            string value = string.Empty;
            StringBuilder sb = new StringBuilder();
            List<string> combineColumnNameList = new List<string>();

            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName.Contains(CommonData.involveCombineStr))
                {
                    combineColumnNameList.Add(dc.ColumnName);
                }
            }
            for (int i = 0; i < rownum; i++)
            {
                combineColumnNameList.ForEach(item =>{
                    value= dt.Rows[i][item].ToString().Trim();
                    if (value==string.Empty)
                    {
                        value = "0";
                    }
                    sb.Append(value);
                
                });
                if (sb.ToString().All(ch => { return ch == '0'; }))//如果全都是0就给空值
                    dt.Rows[i][colNum - 1] =string.Empty;
                else
                    dt.Rows[i][colNum - 1] = sb.ToString();
                sb.Clear();
            }
            combineColumnNameList.ForEach(item => dt.Columns.Remove(item));

            return dt;

        }
        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, System.Data.DataTable> dic)
        {
            if(dic.Keys.Contains(CommonData.CRD_PI_TELPNT))
                dic[CommonData.CRD_PI_TELPNT] = CombineFiletoPayment_State(dic[CommonData.CRD_PI_TELPNT], true);
            if (dic.Keys.Contains(CommonData.CRD_CD_STNCARD))
                dic[CommonData.CRD_CD_STNCARD] = CombineFiletoPayment_State(dic[CommonData.CRD_CD_STNCARD]);
            if (dic.Keys.Contains(CommonData.CRD_CD_LND))
                dic[CommonData.CRD_CD_LND] = CombineFiletoPayment_State(dic[CommonData.CRD_CD_LND]);
            if (dic.Keys.Contains(CommonData.CRD_CD_LN))
                dic[CommonData.CRD_CD_LN] = CombineFiletoPayment_State(dic[CommonData.CRD_CD_LN]);
            return dic;
        }
    }
}
