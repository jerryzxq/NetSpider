using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.BusinessLayer;
using Vcredit.ExternalCredit.CrawlerLayer.CreditVariable;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.CrawlerLayer.JinCityBank.VariableCaculater
{
   
    public class ReportCaculation
    {
        readonly CRD_HD_REPORTBusiness reportbus = new CRD_HD_REPORTBusiness();
        readonly CRD_CD_LNBusiness lnbus = new CRD_CD_LNBusiness();
        readonly CRD_CD_LNDBusiness lndbus = new CRD_CD_LNDBusiness();
        readonly CRD_QR_RECORDDTLBusiness recorddtlbus = new CRD_QR_RECORDDTLBusiness();
        readonly CRD_CD_STNCARDBusiness stncardbus = new CRD_CD_STNCARDBusiness();
        readonly CRD_QR_RECORDDTLINFOBusiness recordInfobus = new CRD_QR_RECORDDTLINFOBusiness();

        readonly VariableInfo vearableInfo = new VariableInfo();
        const string xiaohu = "销户";
        const string zhengchang = "正常";
        const string jieqing = "结清";

        DateTime? query_Time;
        List<CRD_CD_LNEntity> lnList = null;
        List<CRD_CD_LNDEntity> lndList = null;
        CRD_QR_RECORDDTLEntity recordCheck = null;
        List<CRD_CD_STNCARDEntity> stncardList=null ;
        CRD_HD_REPORTEntity reportentity = null;
        List<CRD_QR_RECORDDTLINFOEntity> recordInfoList = null;
        private  void InitialData(CRD_HD_REPORTEntity report)
        {
            lnList = lnbus.GetList(report.Report_Id);
            lndList = lndbus.GetList(report.Report_Id);
            recordCheck = recorddtlbus.Get(report.Report_Id);
            stncardList = stncardbus.GetList(report.Report_Id);
            recordInfoList = recordInfobus.GetList(report.Report_Id);
            reportentity = report;
            query_Time = GetQueryTime();
        }

        public VariableInfo GetByReportid(decimal reportid)
        {
            var info = reportbus.Get(reportid);
            if (info == null)
                return null;
            InitialData(info);
             GetVareableInfo();
             return vearableInfo;

        }
        public VariableInfo GetByReport_sn(string report_Sn)
        {
            var info = reportbus.Get(report_Sn);
            if (info == null)
                return null;
            InitialData(info);
             GetVareableInfo();
             return vearableInfo;
        }
        public VariableInfo GetByCert_No(string Cert_No)
        {
            var info = reportbus.GetBycertno(Cert_No);
            if (info == null)
                return null;
            InitialData(info);
            GetVareableInfo();
            return vearableInfo;
        }

        private void GetVareableInfo()
        {
            //_最早未销户卡龄
            GetCRD_AGE_UNCLS_OLDEST();
            GetL3M_ACCT_QRY_NUMAndL3M_LN_QRY_NUM();
            GetL24M_OPE_NORM_ACCT_PCT();
            GetL12M_LOANACT_USED_MAX_R();
            GetNORM_CDT_BAL_USED_PCT_AVG();
            GetACCT_NUM();
            GetDLQ_5YR_CNT_MAX();
            GetLST_LOAN_CLS_CHU_OPE();
            GetNON_BNK_LN_QRY_CNT();
            GetRCNT_CDT_LMT();
            GetUNCLS_RCNT_OLD_CDT_LMT_RNG();
            GetALL_LOAN_HOUSE_UNCLOSEACCOUNT_CNT();
            GetNORMAL_CARD_AGE_MONTH ();
            GetCREDIT_LIMIT_AMOUNT_NORM_MAX();
            GetNormal_Used_Credit_Limit_Amount();
            //Get();
            //Get();
            //Get();
            //Get();
            //Get();
            //Get();
            //Get();
            //Get();
            //Get();

        }

        #region  贷款统计信息

        /// <summary>
        /// 过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
        /// </summary>
        private void GetL12M_LOANACT_USED_MAX_R()
        {
            Func<int, int>[] funs = { LlsrNoInfo, LnHaveInfo,HaveQueryTime };
            int[] vals = { CommonData._9999999, CommonData._9999998,CommonData._9999997 };
            var val = GetSplecialValue(funs, vals);
            if(val==0)
            {
                decimal L12M_CNT = 0;
                decimal L12M_LOANACT_USED_MAX = 0;
                decimal L12M_LMT_ALL = 0;
                foreach (var ln in lnList)
                {
                    if (ln.Open_Date != null)
                    {

                        if ((int.Parse(query_Time.Value.ToString("yyyyMMdd")) - int.Parse((ln.Open_Date.Value).ToString("yyyyMMdd"))) <= 10000)
                        {
                            L12M_CNT++;
                            if (ln.Balance != null)
                                L12M_LOANACT_USED_MAX = L12M_LOANACT_USED_MAX > ln.Balance.Value ? L12M_LOANACT_USED_MAX : ln.Balance.Value;
                            L12M_LMT_ALL += (decimal)ln.Credit_Limit_Amount;
                        }
                    }
                }
                if (L12M_CNT == 0)
                {
                    vearableInfo.Stat_lnd.L12M_LOANACT_USED_MAX_R = CommonData._9999996;
                }
                else if (L12M_LMT_ALL == 0)
                {
                    vearableInfo.Stat_lnd.L12M_LOANACT_USED_MAX_R = CommonData._9999995;
                }
                else
                {
                    var L12M_LOANACT_USED_MAX_R = L12M_LOANACT_USED_MAX / (L12M_LMT_ALL / L12M_CNT);
                    vearableInfo.Stat_lnd.L12M_LOANACT_USED_MAX_R = decimal.Parse(L12M_LOANACT_USED_MAX_R.ToString("#0.00"));//过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
                }
            }
            else
            {
                vearableInfo.Stat_lnd.L12M_LOANACT_USED_MAX_R = val;
            }
        }
        /// <summary>
        /// 过去结清贷款与开户贷款数比
        /// </summary>
        private void GetLST_LOAN_CLS_CHU_OPE()
        {
            Func<int, int>[] funs = { LlsrNoInfo, LnHaveInfo,HaveReport };
            int[] vals = { CommonData._9999999, CommonData._9999998,CommonData._9999997 };
            var val = GetSplecialValue(funs, vals);
            if (val==0)
            {
                var CLS_LN_CNT = lnList.Count(x => x.State == jieqing);
                if (CLS_LN_CNT == 0)
                    vearableInfo.Stat_ln.LST_LOAN_CLS_CHU_OPE = CommonData._9999996;
                else
                    vearableInfo.Stat_ln.LST_LOAN_CLS_CHU_OPE = (decimal)(CLS_LN_CNT /(lnList.Count*1.00));
            }
            else
            {
                vearableInfo.Stat_ln.LST_LOAN_CLS_CHU_OPE = val;
            }

        }

        /// <summary>
        ///  [网络版征信]住房贷款未销户账户数(all)
        /// </summary>
        private void GetALL_LOAN_HOUSE_UNCLOSEACCOUNT_CNT()
        {
            Func<decimal> fun = () =>
            {
                string[] type_dwarr = { "个人商用房(包含商住两用)贷款", "个人住房按揭贷款", "个人住房贷款", "个人住房公积金贷款", "个人商用房（包含商住两用）贷款" };
                return  lnList.Where(x => x.State != xiaohu && type_dwarr.Contains(x.Type_Dw)).Count();
            };
            vearableInfo.Stat_ln.ALLLOANHOUSEUNCLOSEDCNT = (int)GetCommonFun(fun, LnHaveInfo);
        }

        /// <summary>
        ///  所有贷款的贷款本金总额 
        /// </summary>
        private void GetSUM_Loan_Amount()
        {
            Func<decimal> fun = () =>
            {
                return lnList.Sum(x => x.Credit_Limit_Amount).Value;
            };
            vearableInfo.Stat_ln.SUMLOANLIMITAMOUNT = GetCommonFun(fun, LnHaveInfo);
        }
        /// <summary>
        ///   [网络版征信]其他贷款未销户账户数(all) : 
        /// </summary>
        private void GetALL_LOAN_OTHER_UNCLOSED_CNT()
        {
            Func<decimal> fun = () =>
            {
                return lnList.Count(x => x.Type_Dw != "住房贷款" && x.State != xiaohu);
            };
            vearableInfo.Stat_ln.ALLLOANOTHERUNCLOSEDCNT = (int)GetCommonFun(fun, LnHaveInfo);
        }

        #endregion

        #region  贷记卡统计信息
        /// <summary>
        /// 最早未销户卡龄
        /// </summary>
        private void GetCRD_AGE_UNCLS_OLDEST()
        {
            Func<decimal> fun = () =>
            {
                decimal value = 0;
                var unclslndlist = lndList.Where(lnd => !lnd.State.Contains(xiaohu));
                if (query_Time != null)
                {
                    foreach (var lnd in unclslndlist)
                    {
                        if (lnd.Open_Date != null)
                        {
                            var CRD_AGE = query_Time.Value.Year * 12 + query_Time.Value.Month - (lnd.Open_Date.Value).Year * 12 - (lnd.Open_Date.Value).Month;
                            if (value < CRD_AGE)
                                value = CRD_AGE;
                        }
                    }
                }
                return value;
            };
            vearableInfo.Stat_lnd.CRD_AGE_UNCLS_OLDEST = (int)GetCommonFun(fun, LndHaveInfo);
          
        }
        /// <summary>
        /// 过去24个月内非共享信用卡账户开户数占当前正常账户比例 
        /// </summary>
        private void GetL24M_OPE_NORM_ACCT_PCT()
        {
            Func<int, int>[] funs = { LlsrNoInfo, LndHaveInfo, };
            int[] vals = { CommonData._9999999, CommonData._8888887 };
            var val = GetSplecialValue(funs, vals);
            if (val == 0)
            {
                decimal  value = GetNOW_NORM_UNI_ACCT_NUM();
                if (val == 0)
                {
                    vearableInfo.Stat_ln.L24M_OPE_NORM_ACCT_PCT = CommonData._9999998;
                }
                else
                {
                    var L24M_OPE_NORM_ACCT_PCT = GetL24M_OPE_ACCT_NUM() / value;
                    vearableInfo.Stat_ln.L24M_OPE_NORM_ACCT_PCT = decimal.Parse(L24M_OPE_NORM_ACCT_PCT.ToString("#0.00"));
                }
            }
            else
            {
                vearableInfo.Stat_ln.L24M_OPE_NORM_ACCT_PCT = val;
            }

        }
        private decimal GetL24M_OPE_ACCT_NUM()
        {
            decimal L24M_OPE_ACCT_NUM = 0;
            List<DateTime?> tempopenDate = new List<DateTime?>();
            foreach (var lnd in lndList)
            {
                if (string.IsNullOrEmpty(lnd.Currency))
                    continue;

                if (!tempopenDate.Contains(lnd.Open_Date))
                {
                    if (query_Time!=null && lnd.Open_Date != null)
                    {
                        var CRD_AGE = query_Time.Value.Year * 12 + query_Time.Value.Month - (lnd.Open_Date.Value).Year * 12 - (lnd.Open_Date.Value).Month;
                        if (0 < CRD_AGE && CRD_AGE < 24)
                        {
                            L24M_OPE_ACCT_NUM++;
                        }
                    }
                    tempopenDate.Add(lnd.Open_Date);
                }
            }
            return L24M_OPE_ACCT_NUM;
        }
        CRD_CD_LNDEntity[] normallndlist = null;
        List<CRD_CD_LNDEntity> filterlndlist = new List<CRD_CD_LNDEntity>();
        private decimal  GetNOW_NORM_UNI_ACCT_NUM()
        {
            lndList.Where(lnd => lnd.State==zhengchang).ToList().CopyTo(normallndlist);//注意测试
            foreach (CRD_CD_LNDEntity lnd in normallndlist)
            {
                var templnd = filterlndlist.Where(flnd => flnd.Open_Date == lnd.Open_Date).FirstOrDefault();
                if (templnd != null)
                {
                    if (lnd.Credit_Limit_Amount < templnd.Credit_Limit_Amount)
                    {
                        lnd.Credit_Limit_Amount = templnd.Credit_Limit_Amount;
                    }
                    decimal NORM_CDT_USED = (decimal)lnd.Used_Credit_Limit_Amount;
                    NORM_CDT_USED += (decimal)templnd.Used_Credit_Limit_Amount;
                    lnd.Used_Credit_Limit_Amount = NORM_CDT_USED;//在修改UserCreditLimitAmount时会将lndList中的此字段修改（原因？？？）
                    filterlndlist.Remove(templnd);
                    filterlndlist.Add(lnd);
                }
                else
                {
                    filterlndlist.Add(lnd);
                }
            }
            return  filterlndlist.Count();
        }
        /// <summary>
        /// 当前正常的信用卡账户最大负债额与透支余额之比的均值
        /// </summary>
        private void GetNORM_CDT_BAL_USED_PCT_AVG()
        {
            Func<int, int>[] funs = { LlsrNoInfo, LndHaveInfo, };
            int[] vals = { CommonData._9999999, CommonData._8888887 };
            var val = GetSplecialValue(funs, vals);
            if(val==0)
            {
                List<decimal> LIST_NORM_CDT_BAL_USED_PCT = new List<decimal>();
                foreach (var lnd in filterlndlist)
                {
                    decimal CDT_LMT_SUM =lnd.Credit_Limit_Amount.Value;
                    decimal NORM_CDT_USED = lnd.Used_Credit_Limit_Amount.Value;
                    decimal SUM_NORM_CDT_USED = CDT_LMT_SUM - NORM_CDT_USED;
                    decimal NORM_CDT_BAL_USED_PCT = 0;
                    if (SUM_NORM_CDT_USED != 0)
                    {
                        NORM_CDT_BAL_USED_PCT = NORM_CDT_USED / SUM_NORM_CDT_USED;
                        LIST_NORM_CDT_BAL_USED_PCT.Add(NORM_CDT_BAL_USED_PCT);
                    }
                }
            
                decimal NORM_CDT_BAL_USED_PCT_AVG = 0;
                if (normallndlist.Count() == 0 || LIST_NORM_CDT_BAL_USED_PCT.Count == 0)
                {
                   vearableInfo.Stat_lnd.NORM_CDT_BAL_USED_PCT_AVG = CommonData._9999998;
                }
                else
                {
                    NORM_CDT_BAL_USED_PCT_AVG = LIST_NORM_CDT_BAL_USED_PCT.Sum() / LIST_NORM_CDT_BAL_USED_PCT.Count;
                    vearableInfo.Stat_lnd.NORM_CDT_BAL_USED_PCT_AVG = decimal.Parse(NORM_CDT_BAL_USED_PCT_AVG.ToString("#0.00"));//当前正常的信用卡账户最大负债额与透支余额之比的均值
                }

            }
            else
            {
                vearableInfo.Stat_lnd.NORM_CDT_BAL_USED_PCT_AVG = val;
            }
        }
        /// <summary>
        /// [征信]信贷账户数:
        /// </summary>
        private void GetACCT_NUM()
        {
            Func<int, int>[] funs = { LlsrNoInfo };
            int[] vals = { CommonData._9999999 };
            var val = GetSplecialValue(funs, vals);
            if (val == 0)
                vearableInfo.Stat_ln.ACCT_NUM = lndList.Select(e => e.Finance_Org + e.Open_Date).Distinct().Count() + lnList.Count;
            else
                vearableInfo.Stat_ln.ACCT_NUM = val;
        }
        /// <summary>
        /// 五年内逾期最大次数:
        /// </summary>
        private void GetDLQ_5YR_CNT_MAX()
        {
            Func<decimal> fun = () =>
            {
               return  (decimal)(lndList.Max(x => x.Overdue_Cyc) * 2.5);
            };
            vearableInfo.Stat_lnd.DLQ_5YR_CNT_MAX = (int)GetCommonFun(fun, LndHaveInfo);

        }
        /// <summary>
        /// 最近授信额度
        /// </summary>
        private void GetRCNT_CDT_LMT()
        {
            Func<decimal> fun = () =>
            {
                var CreditLimitRencent = lndList.Where(o => o.State == zhengchang).OrderByDescending(t => t.Open_Date).ThenByDescending(d => d.Credit_Limit_Amount).ToList();
                return CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].Credit_Limit_Amount.Value : decimal.Zero;
            };
            vearableInfo.Stat_lnd.RCNT_CDT_LMT = (int)GetCommonFun(fun, LndHaveInfo);
        }
        /// <summary>
        /// 未销户最早最近授信额度差
        /// </summary>
        private void GetUNCLS_RCNT_OLD_CDT_LMT_RNG()
        {
            Func<decimal> fun = () =>
            {
                decimal value = 0;
                var CreditLimitRencent = lndList.Where(o => o.State != xiaohu).OrderByDescending(t => t.Open_Date).ThenByDescending(d => d.Credit_Limit_Amount).ToList();
                if (CreditLimitRencent.Count == 0)
                {
                    value = CommonData._9999997;
                }
                else
                {
                    value = (CreditLimitRencent.Last().Credit_Limit_Amount - CreditLimitRencent.First().Credit_Limit_Amount).Value;
                }
                return value;
            };
            vearableInfo.Stat_lnd.UNCLS_RCNT_OLD_CDT_LMT_RNG = (int)GetCommonFun(fun, LndHaveInfo);
        }

        /// <summary>
        /// 正常状态最早卡龄
        /// </summary>
        private void GetNORMAL_CARD_AGE_MONTH()
        {
            Func<decimal> fun = () =>
            {
                decimal value = 0;
                var oldestentity = lndList.Where(o => o.State == zhengchang).OrderBy(t => t.Open_Date).ThenByDescending(d => d.Credit_Limit_Amount).FirstOrDefault();
                if (oldestentity != null)//如果为null 该如何赋值 
                {
                    value = (query_Time.Value.Year - oldestentity.Open_Date.Value.Year) * 12 + (query_Time.Value.Month - oldestentity.Open_Date.Value.Month);
                }
                return value;
            };
            vearableInfo.Stat_lnd.NORMALCARDAGEMONTH = (int)GetCommonFun(fun, LndHaveInfo);
        }


        /// <summary>
        /// 正常信用卡最大额度
        /// </summary>
        private void GetCREDIT_LIMIT_AMOUNT_NORM_MAX()
        {
            Func<decimal> fun = () =>
            {
                decimal value = 0;
                var zclist = lndList.Where(o => o.State == zhengchang);
                if (zclist.Count() != 0)//如果为0该如何赋值 
                {
                   value = zclist.Max(x => x.Credit_Limit_Amount).Value;
                }
                return value;
            };
            vearableInfo.Stat_lnd.CREDITLIMITAMOUNTNORMMAX = GetCommonFun(fun, LndHaveInfo);
        }
        /// <summary>
        /// 正常信用卡中的最大已使用额度
        /// </summary>
        private void GetNormal_Used_Credit_Limit_Amount()
        {
            Func<decimal> fun = () =>
            {
                decimal value = 0;
                var zclist = lndList.Where(o => o.State == zhengchang);
                if (zclist.Count() != 0)//如果为0该如何赋值 
                {
                    value = zclist.Max(x => x.Used_Credit_Limit_Amount).Value;
                }
                return value;
            };
            vearableInfo.Stat_lnd.NORMALUSEDMAX = GetCommonFun(fun, LndHaveInfo);
        }

        /// <summary>
        /// 信用卡已使用总额度
        /// </summary>
        private void GetSUM_USED_CREDIT_LIMIT_AMOUNT()
        {
            Func<decimal> fun = () =>
            {
                return lndList.Sum(x => x.Used_Credit_Limit_Amount).Value;
            };
            vearableInfo.Stat_lnd.SUMUSEDCREDITLIMITAMOUNT = GetCommonFun(fun, LndHaveInfo);
        }
        /// <summary>
        /// 信用卡未销户账户数(all)
        /// </summary>
        private void GetALL_CREDIT_UNCLOSED_CNT()
        {
            Func<decimal> fun = () =>
            {
                return lndList.Where(x => x.State != xiaohu).Select(x => x.Open_Date).Distinct().Count();
            };
            vearableInfo.Stat_lnd.ALLCREDITUNCLOSEDCNT = (int)GetCommonFun(fun, LndHaveInfo);

        }

        /// <summary>
        /// 贷记卡最大逾期次数占比 : 
        /// </summary>
        private void Getlnd_max_overdue_percent()
        {
            Func<decimal> fun = () =>
            {
                int usemonth = 0;
                decimal tempoverdue = 0;
                decimal lnd_max_overdue_percent = 0;
                foreach (var item in lndList)
                {
                    if (item.Overdue_Cyc > 0 && item.Open_Date != null)
                    {
                        usemonth = CommonFun.GetIntervalOf2DateTime(query_Time.Value, (DateTime)item.Open_Date.Value, "M");
                        if (usemonth > 60)
                        {
                            usemonth = 60;
                        }
                        if (usemonth == 0)
                        {
                            continue;
                        }
                        tempoverdue = item.Overdue_Cyc.Value / usemonth;
                        if (lnd_max_overdue_percent < tempoverdue)
                        {
                            lnd_max_overdue_percent = tempoverdue;
                        }
                    }
                }
                return lnd_max_overdue_percent;
            };
            vearableInfo.Stat_lnd.lnd_max_overdue_percent = GetCommonFun(fun, LndHaveInfo);
        }
     

   
        #endregion

        #region  查询信息和其他信息
      
        /// <summary>
        /// 评分_过去3个月信用卡审批查询次数,评分_过去3个月贷款审批查询次数 
        /// </summary>
        private void GetL3M_ACCT_QRY_NUMAndL3M_LN_QRY_NUM()
        {

            Func<int, int>[] funs = { LlsrNoInfo, RecordcheckHaveInfo};
            int[] vals = { CommonData._9999999, CommonData._8888888 };
            var val= GetSplecialValue(funs, vals);
            if(val==0)
            {
                vearableInfo.Stat_record.L3M_ACCT_QRY_NUM = recordCheck.COUNT_CARD_IN3M.Value;
                vearableInfo.Stat_record.L3M_LN_QRY_NUM = recordCheck.COUNT_loan_IN3M.Value;
            }
            else
            {
                vearableInfo.Stat_record.L3M_ACCT_QRY_NUM = val;
                vearableInfo.Stat_record.L3M_LN_QRY_NUM = val;
            }
        }
        /// <summary>
        /// 非银行类机构贷款查询次数
        /// </summary>
        private void GetNON_BNK_LN_QRY_CNT()
        {
            Func<int, int>[] funs = { LlsrNoInfo };
            int[] vals = { CommonData._9999999 };
            var val = GetSplecialValue(funs, vals);
            if(val==0)
            {
                vearableInfo.Stat_record.NON_BNK_LN_QRY_CNT =recordInfoList.Count(o => o.Query_Reason == "贷款审批" && (o.Querier.IndexOf("银行") == -1 && o.Querier.IndexOf("花旗") == -1));
            }
            else
            {
                vearableInfo.Stat_record.NON_BNK_LN_QRY_CNT = val;
            }
        }
        #endregion

        #region  公共私有方法

        private decimal  GetCommonFun(Func<decimal> funval, Func<int, int> fun) 
        {
            decimal   property = 0;
            Func<int, int>[] funs = { LlsrNoInfo ,fun};
            int[] vals = { CommonData._9999999,CommonData._8888888 };
          
            var val = GetSplecialValue(funs,vals);
            if (val == 0)
            {
                property = funval();
            }
            else
            {
                property = val;
            }
            return property;
        }
        private int GetSplecialValue(Func<int,int>[]  funs,int[] datas)
        {
            int index=0;
            int  specialvalue=0;
            foreach(var fun in funs)
            {
               if( fun(datas[index])!=0)
               {
                   specialvalue=datas[index];
                   break;
               }     
               index++;
            }
            return specialvalue;
        }
        private DateTime? GetQueryTime()
        {
            DateTime? dt=null;
            if (reportentity.Query_Time != null)
                dt = reportentity.Query_Time;
            else if (reportentity.Report_Create_Time != null)
                dt = reportentity.Report_Create_Time;
            return dt;
        }
    
        private  int commonFun(Func<bool> fun,int value)
        {
            if (fun())
            {
                return value;
            }
            return 0;
        }
        /// <summary>
        /// 有征信报告号(REPORT_ID)，但无贷款信息即CRD_CD_LN中无，无贷记卡信息即CRD_CD_LND中无，
        /// 无准贷记卡信息即CRD_CD_STNCARD中无，无查询信息即CRD_QR_RECORDDT
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private int LlsrNoInfo(int val)
        {
            return commonFun(() => lnList.Count == 0 && lndList.Count == 0 && stncardList.Count == 0 && recordCheck == null, val);
        }
        private int LndHaveInfo(int val)
        {
            return commonFun(() => lndList.Count == 0, val);
        }
        private int RecordcheckHaveInfo(int val)
        {
            return commonFun(()=>recordInfoList.Count==0,val);
        }
        private int LnHaveInfo(int val)
        {
            return commonFun(() => lnList.Count ==0, val);
        }
        private int HaveQueryTime(int val)
        {
            return commonFun(() =>query_Time== null, val);
        }
        private int HaveReport(int val)
        {
            return commonFun(() => reportentity == null, val);
        }
        #endregion


    }
}
