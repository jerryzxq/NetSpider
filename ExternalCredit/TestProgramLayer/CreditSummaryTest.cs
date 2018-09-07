using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
//using Microsoft.Office.Interop.Excel;
//using DataTable = System.Data.DataTable;
using Vcredit.ExternalCredit.CrawlerLayer.CreditVariable;
using Aspose.Cells;

namespace TestProgramLayer
{
    class CreditSummaryTest
    {

        public static void GetCreditSummary()
        {
            string strConn;
            //strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + @"E:\1.xlsx" + ";Extended Properties='Excel 12.0;'";
            //OleDbConnection OleConn = new OleDbConnection(strConn);
            //OleConn.Open();
            //String sql = "SELECT * FROM  [sheet1$]";//可是更改Sheet名称，比如sheet2$，等等   

            //OleDbDataAdapter OleDaExcel = new OleDbDataAdapter(sql, OleConn);
            //DataSet OleDsExcle = new DataSet();
            //OleDaExcel.Fill(OleDsExcle, "sheet1");
            //OleConn.Close();
            //DataTable dt = OleDsExcle.Tables[0];

            //HttpHelper httpHelper = new HttpHelper();
            //HttpItem httpItem = null;
            //string postdata = string.Empty;

            string filepath = @"D:\1.xlsx";
            Workbook workbook = new Workbook(filepath);
            Worksheet sheet = workbook.Worksheets[0];
            ReportCaculation caculation = new ReportCaculation();
         
                for (int i = 1; i < 1000; i++)
                {
                  
                    try
                    {
                        VariableInfo info = caculation.GetByReportid(i);
                        if (info == null)
                        {
                            continue;
                        }
                        sheet.Cells[i + 1, 0].PutValue(info.Report_Id);
                        sheet.Cells[i + 1, 1].PutValue(info.Report_Sn);
                        sheet.Cells[i + 1, 2].PutValue(info.Name);
                        sheet.Cells[i + 1, 3].PutValue(info.Cert_No);
                        sheet.Cells[i + 1, 4].PutValue(info.IsCreditBlank);
                        sheet.Cells[i + 1, 5].PutValue(info.CRD_STAT_QR.M3ALLCNTTOTAL);
                        sheet.Cells[i + 1, 6].PutValue(info.CRD_STAT_QR.M3CREDITCNT);
                        sheet.Cells[i + 1, 7].PutValue(info.CRD_STAT_QR.M3LOANCNT);
                        sheet.Cells[i + 1, 8].PutValue(info.CRD_STAT_QR.L3M_ACCT_QRY_NUM);
                        sheet.Cells[i + 1, 9].PutValue(info.CRD_STAT_QR.L3M_LN_QRY_NUM);
                        sheet.Cells[i + 1, 10].PutValue(info.CRD_STAT_QR.NON_BNK_LN_QRY_CNT);
                        sheet.Cells[i + 1, 11].PutValue(info.CRD_STAT_LN.ALLLOANHOUSEDELAY90CNT);
                        sheet.Cells[i + 1, 12].PutValue(info.CRD_STAT_LN.ALLLOANHOUSEDELAYCNT);
                        sheet.Cells[i + 1, 13].PutValue(info.CRD_STAT_LN.ALLLOANHOUSEUNCLOSEDCNT);
                        sheet.Cells[i + 1, 14].PutValue(info.CRD_STAT_LN.ALLLOANOTHERDELAY90CNT);
                        sheet.Cells[i + 1, 15].PutValue(info.CRD_STAT_LN.ALLLOANOTHERDELAYCNT);
                        sheet.Cells[i + 1, 16].PutValue(info.CRD_STAT_LN.ALLLOANOTHERUNCLOSEDCNT);
                        sheet.Cells[i + 1, 17].PutValue(info.CRD_STAT_LN.LOANDELAY90CNT);
                        sheet.Cells[i + 1, 18].PutValue(info.CRD_STAT_LN.LOANAGEMONTH);
                        sheet.Cells[i + 1, 19].PutValue(info.CRD_STAT_LN.LOANDLQAMOUNT);
                        sheet.Cells[i + 1, 20].PutValue(info.CRD_STAT_LN.LOANPMTMONTHLY);
                        sheet.Cells[i + 1, 21].PutValue(info.CRD_STAT_LN.ALLLOANOTHERCNT);
                        sheet.Cells[i + 1, 22].PutValue(info.CRD_STAT_LN.SUMLOANLIMITAMOUNT);
                        sheet.Cells[i + 1, 23].PutValue(info.CRD_STAT_LN.SUMLOANBALANCE);
                        sheet.Cells[i + 1, 24].PutValue(info.CRD_STAT_LN.LOAN_HOUSE_DLQ_AMOUNT);
                        sheet.Cells[i + 1, 25].PutValue(info.CRD_STAT_LN.NORMALLOANAGEMONTH);
                        sheet.Cells[i + 1, 26].PutValue(info.CRD_STAT_LN.ln_housing_fund_amount);
                        sheet.Cells[i + 1, 27].PutValue(info.CRD_STAT_LN.ln_shopfront_amount);
                        sheet.Cells[i + 1, 28].PutValue(info.CRD_STAT_LN.ln_housing_mortgage_amount);
                        sheet.Cells[i + 1, 29].PutValue(info.CRD_STAT_LN.ALL_LOAN_DELAY_MONTH);
                        sheet.Cells[i + 1, 30].PutValue(info.CRD_STAT_LN.ln_Latest_6M_Used_Avg_Amount);
                        sheet.Cells[i + 1, 31].PutValue(info.CRD_STAT_LN.ln_normal_count);
                        sheet.Cells[i + 1, 32].PutValue(info.CRD_STAT_LN.L24M_OPE_NORM_ACCT_PCT);
                        sheet.Cells[i + 1, 33].PutValue(info.CRD_STAT_LN.ALL_LOAN_HOUSE_FOROTHERS_CNT);
                        sheet.Cells[i + 1, 34].PutValue(info.CRD_STAT_LN.ALL_LOAN_OTHER_FOROTHERS_CNT);
                        sheet.Cells[i + 1, 35].PutValue(info.CRD_STAT_LN.ALL_LOAN_HOUSE_CNT);
                        sheet.Cells[i + 1, 36].PutValue(info.CRD_STAT_LN.ACCT_NUM);
                        sheet.Cells[i + 1, 37].PutValue(info.CRD_STAT_LN.LST_LOAN_CLS_CNT);
                        sheet.Cells[i + 1, 38].PutValue(info.CRD_STAT_LN.LST_LOAN_OPE_CNT);
                        sheet.Cells[i + 1, 39].PutValue(info.CRD_STAT_LN.Monthly_Mortgage_Payment_Total);
                        sheet.Cells[i + 1, 40].PutValue(info.CRD_STAT_LN.Monthly_Mortgage_Payment_Max);
                        sheet.Cells[i + 1, 41].PutValue(info.CRD_STAT_LN.Monthly_Commercial_Mortgage_Payment_Total);
                        sheet.Cells[i + 1, 42].PutValue(info.CRD_STAT_LN.Monthly_Commercial_Mortgage_Payment_Max);
                        sheet.Cells[i + 1, 43].PutValue(info.CRD_STAT_LN.ln_other_amount);
                        sheet.Cells[i + 1, 44].PutValue(info.CRD_STAT_LN.assurerrepay_amount);
                        sheet.Cells[i + 1, 45].PutValue(info.CRD_STAT_LN.Monthly_Other_Mortgage_Payment_Total);
                        sheet.Cells[i + 1, 46].PutValue(info.CRD_STAT_LN.Max_AccumFund_Mon_Mort_Repay);
                        sheet.Cells[i + 1, 47].PutValue(info.CRD_STAT_LN.Max_AccumFund_All_Mort_Repay);
                        sheet.Cells[i + 1, 48].PutValue(info.CRD_STAT_LND.M9AVGLENDAMOUNT);
                        sheet.Cells[i + 1, 49].PutValue(info.CRD_STAT_LND.M9DELIVERCNT);
                        sheet.Cells[i + 1, 50].PutValue(info.CRD_STAT_LND.M9LENDAMOUNT);
                        sheet.Cells[i + 1, 51].PutValue(info.CRD_STAT_LND.ALLCREDITDELAY90CNT);
                        sheet.Cells[i + 1, 52].PutValue(info.CRD_STAT_LND.ALLCREDITDELAYCNT);
                        sheet.Cells[i + 1, 53].PutValue(info.CRD_STAT_LND.ALLCREDITUNCLOSEDCNT);
                        sheet.Cells[i + 1, 54].PutValue(info.CRD_STAT_LND.CREDITDELAYCNT);
                        sheet.Cells[i + 1, 55].PutValue(info.CRD_STAT_LND.SUMUSEDCREDITLIMITAMOUNT);
                        sheet.Cells[i + 1, 56].PutValue(info.CRD_STAT_LND.NORMALCARDNUM);
                        sheet.Cells[i + 1, 57].PutValue(info.CRD_STAT_LND.CARDAGEMONTH);
                        sheet.Cells[i + 1, 58].PutValue(info.CRD_STAT_LND.CREDITLIMITAMOUNTNORMMAX);
                        sheet.Cells[i + 1, 59].PutValue(info.CRD_STAT_LND.NORMALCREDITBALANCE);
                        sheet.Cells[i + 1, 60].PutValue(info.CRD_STAT_LND.NORMALUSEDRATE);
                        sheet.Cells[i + 1, 61].PutValue(info.CRD_STAT_LND.CREDITDLQAMOUNT);
                        sheet.Cells[i + 1, 62].PutValue(info.CRD_STAT_LND.NORMALUSEDMAX);
                        sheet.Cells[i + 1, 63].PutValue(info.CRD_STAT_LND.SUMNORMALLIMITAMOUNT);
                        sheet.Cells[i + 1, 64].PutValue(info.CRD_STAT_LND.NORMALCARDAGEMONTH);
                        sheet.Cells[i + 1, 65].PutValue(info.CRD_STAT_LND.ALL_CREDIT_DELAY_MONTH);
                        sheet.Cells[i + 1, 66].PutValue(info.CRD_STAT_LND.loand_Badrecord);
                        sheet.Cells[i + 1, 67].PutValue(info.CRD_STAT_LND.stncard_Badrecord);
                        sheet.Cells[i + 1, 68].PutValue(info.CRD_STAT_LND.StnCard_UseCreditLimit);
                        sheet.Cells[i + 1, 69].PutValue(info.CRD_STAT_LND.lnd_max_overdue_percent);
                        sheet.Cells[i + 1, 70].PutValue(info.CRD_STAT_LND.CRD_AGE_UNCLS_OLDEST);
                        sheet.Cells[i + 1, 71].PutValue(info.CRD_STAT_LND.lnd_max_normal_Age);
                        sheet.Cells[i + 1, 72].PutValue(info.CRD_STAT_LND.L12M_LOANACT_USED_MAX_R);
                        sheet.Cells[i + 1, 73].PutValue(info.CRD_STAT_LND.NORM_CDT_BAL_USED_PCT_AVG);
                        sheet.Cells[i + 1, 74].PutValue(info.CRD_STAT_LND.ALL_CREDIT_FOROTHERS_CNT);
                        sheet.Cells[i + 1, 75].PutValue(info.CRD_STAT_LND.ALL_CREDIT_CNT);
                        sheet.Cells[i + 1, 76].PutValue(info.CRD_STAT_LND.DLQ_5YR_CNT_MAX);
                        sheet.Cells[i + 1, 77].PutValue(info.CRD_STAT_LND.RCNT_CDT_LMT);
                        sheet.Cells[i + 1, 78].PutValue(info.CRD_STAT_LND.UNCLS_RCNT_CDT_LMT);
                        sheet.Cells[i + 1, 79].PutValue(info.CRD_STAT_LND.UNCLS_OLD_CDT_LMT);
                        sheet.Cells[i + 1, 80].PutValue(info.CRD_STAT_LND.DLQ_5YR_CNT_AVG);
                        sheet.Cells[i + 1, 81].PutValue(info.CRD_STAT_LND.NORM_CDT_LMT_MAX);
                        sheet.Cells[i + 1, 82].PutValue(info.CRD_STAT_LND.NORM_USED_SUM);
                        sheet.Cells[i + 1, 83].PutValue(info.CRD_STAT_LND.NORM_CDT_SUM);
                        sheet.Cells[i + 1, 84].PutValue(info.CRD_STAT_LND.NOW_NORM_UNI_ACCT_NUM);
                        sheet.Cells[i + 1, 85].PutValue(info.CRD_STAT_LND.CREDIT_CARD_AGE_OLDEST);
                        sheet.Cells[i + 1, 86].PutValue(info.CRD_STAT_LND.CardNum);

                        sheet.Cells[i + 1, 87].PutValue(info.CRD_STAT_LND.M24_DELIVER_CNT);
                        sheet.Cells[i + 1, 88].PutValue(info.CRD_STAT_LND.CREDIT_NORMAL_PAY_MAX);
                        sheet.Cells[i + 1, 89].PutValue(info.CRD_STAT_LND.AVG_NORMAL_LIMIT_AMOUNT);
                        sheet.Cells[i + 1, 90].PutValue(info.CRD_STAT_LND.M24_LEND_AMOUNT);
                        sheet.Cells[i + 1, 91].PutValue(info.CRD_STAT_LND.NORMALCREDITBALANCE);

                        Console.WriteLine(i);
                    }
                    catch {
                    
                    }
                }
                workbook.Save(filepath);
                 
              
        }
    }


}
