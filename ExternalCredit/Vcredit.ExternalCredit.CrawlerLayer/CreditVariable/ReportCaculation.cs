using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.Common.Constants;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.BusinessLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.Common.Ext;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Reflection;
namespace Vcredit.ExternalCredit.CrawlerLayer.CreditVariable
{
   
    public class ReportCaculation
    {

        readonly CRD_HD_REPORTBusiness reportbus = new CRD_HD_REPORTBusiness();
        readonly VariableInfo vearableInfo = new VariableInfo();
        const string xiaohu = "销户";
        const string zhengchang = "正常";
        const string jieqing = "结清";
        const string rmb = "人民币";
        DateTime? query_Time;
        List<CRD_CD_LNEntity> lnList = null;
        List<CRD_CD_LNDEntity> lndList = null;
        List<CRD_CD_STNCARDEntity> stncardList=null ;
        CRD_HD_REPORTEntity reportentity = null;
        List<CRD_QR_RECORDDTLINFOEntity> recordInfoList = null;
     
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
        private void InitialData(CRD_HD_REPORTEntity report)
        {

            CRD_CD_LNBusiness lnbus = new CRD_CD_LNBusiness();
            CRD_CD_LNDBusiness lndbus = new CRD_CD_LNDBusiness();
            CRD_CD_STNCARDBusiness stncardbus = new CRD_CD_STNCARDBusiness();
            CRD_QR_RECORDDTLINFOBusiness recordInfobus = new CRD_QR_RECORDDTLINFOBusiness();
            lnList = lnbus.GetList(report.Report_Id);
            lnList.ForEach(x => x.Overdue_Cyc = VariableCommonFun.GetOverdue_Cyc(x.Payment_State));
            lndList = lndbus.GetList(report.Report_Id);
            lndList.ForEach(x => x.Overdue_Cyc = VariableCommonFun.GetOverdue_Cyc(x.Payment_State));
            stncardList = stncardbus.GetList(report.Report_Id);
            stncardList.ForEach(x => x.Overdue_Cyc = VariableCommonFun.GetOverdue_Cyc(x.Payment_State));
            recordInfoList = recordInfobus.GetList(report.Report_Id);
            reportentity = report;
            query_Time = GetQueryTime();
            vearableInfo.Cert_No = report.Cert_No;
            vearableInfo.Cert_Type = report.Cert_Type;
            vearableInfo.Report_Create_Time = report.Report_Create_Time;
            vearableInfo.Report_Id = report.Report_Id;
            vearableInfo.Report_Sn = report.Report_Sn;
            vearableInfo.Query_Time = report.Query_Time;
            vearableInfo.Name = report.Name;
        }

        private DateTime? GetQueryTime()
        {
            DateTime? dt = null;
            if (reportentity.Query_Time != null)
                dt = reportentity.Query_Time;
            else if (reportentity.Report_Create_Time != null)
                dt = reportentity.Report_Create_Time;
            return dt;
        }
        private void GetVareableInfo()
        {
            CRD_STAT_LNEntity statln = new CRD_STAT_LNEntity() { ReportId=(int)reportentity.Report_Id};//贷款统计表
            CRD_STAT_LNDEntity statlnd = new CRD_STAT_LNDEntity()  { ReportId = (int)reportentity.Report_Id } ;//贷记卡统计表
            CRD_STAT_QREntity statqr = new CRD_STAT_QREntity() { ReportId = (int)reportentity.Report_Id };//查询统计表
            vearableInfo.CRD_STAT_LN = statln;
            vearableInfo.CRD_STAT_LND = statlnd;
            vearableInfo.CRD_STAT_QR = statqr;

            #region 原入库变量计算
            decimal CREDIT_LIMIT_AMOUNT_NORM_MAX = 0;//正常信用卡最大额度
            //decimal NORMAL_CREDIT_BALANCE = 0;//正常信用卡未用额度
            decimal SUM_NORMAL_LIMIT_AMOUNT = 0;//正常信用卡总额度
            decimal SUM_NORMAL_USE_LIMIT_AMOUNT = 0;//正常信用卡使用额度
            decimal credit_dlq_amount = 0;//信用卡逾期金额
            decimal M9_LEND_AMOUNT = 0;//最近九个月发放贷记卡总金额
            int M9_DELIVER_CNT = 0;//最近九个月发放贷记卡数量
            int NORMAL_CARDNUM = 0;//正常卡数据量
            int CREDIT_DELAY_CNT = 0;
            decimal NORMAL_USED_MAX = 0;//正常信用卡最大已使用额度

            decimal loan_dlq_amount = 0;//贷款逾期金额
            decimal loan_pmt_monthly = 0;//贷款每月还款本金金额
            decimal loan_house_pmt_monthly = 0;//贷款每月还款本金金额
            decimal SUM_LOAN_BALANCE = 0;//贷款总余额
            decimal SUM_LOAN_LIMIT_AMOUNT = 0;//贷款本金总额
            decimal loan_Balance = 0;

            DateTime NowDate = query_Time.Value;//当前日期
            DateTime CreditOpenTime = query_Time.Value;//信用卡最早开卡时间
            DateTime LoanOpenTime = query_Time.Value;//最早贷款时间
            DateTime NormalCreditOpenTime = query_Time.Value;//正常信用卡最早开卡时间
            DateTime NormalLoanOpenTime = query_Time.Value;//未还完最早贷款时间
            int specialValue = 0;//贷记卡获取特殊值

            #region 基本信息、信息概要、报告查询统计(核对一下算法)
            var creditcue = new CRD_IS_CREDITCUEBusiness().GetEntity(reportentity.Report_Id);
            if (creditcue != null)
            {
                statln.ALL_LOAN_HOUSE_CNT = (int)creditcue.House_Loan_Count;//住房贷款账户数
                statln.ALLLOANOTHERCNT = (int)creditcue.Other_Loan_Count;//其他贷款账户数
                statlnd.ALL_CREDIT_CNT = (int)creditcue.Loancard_Count;//信用卡账户数
            }
            if (lnList != null)
            {
                string[] type_dwarr = { "个人商用房(包含商住两用)贷款", "个人住房按揭贷款", "个人住房贷款", "个人住房公积金贷款", "个人商用房（包含商住两用）贷款" };
                statln.ALLLOANHOUSEUNCLOSEDCNT = lnList.Where(x => x.State != jieqing && type_dwarr.Contains(x.Type_Dw)).Count();//住房贷款未销户账户数
                specialValue = GetSpecialValue(true);
                if (specialValue != 0)
                    statln.ALLLOANOTHERUNCLOSEDCNT = specialValue;
                else
                    statln.ALLLOANOTHERUNCLOSEDCNT = lnList.Where(x => x.State != jieqing && !type_dwarr.Contains(x.Type_Dw)).Count();//其他贷款未销户账户数(如果修改这个逻辑记得修改其相对应的逻辑)

                statln.ALLLOANHOUSEDELAYCNT = lnList.Where(x => x.Overdue_Cyc > 0 && type_dwarr.Contains(x.Type_Dw)).Count();//住房贷款发生过逾期的账户数
                statln.ALLLOANOTHERDELAYCNT = lnList.Where(x => x.Overdue_Cyc > 0 && !type_dwarr.Contains(x.Type_Dw)).Count();//其他贷款发生过逾期的账户数

                statln.ALLLOANHOUSEDELAY90CNT = lnList.Where(x => x.LnoverList.Count(y => y.Last_Months >= 3) > 0 && type_dwarr.Contains(x.Type_Dw)).Count();//住房贷款发生过90天以上逾期的账户数
                statln.ALLLOANOTHERDELAY90CNT = lnList.Where(x => x.LnoverList.Count(y => y.Last_Months >= 3) > 0 && !type_dwarr.Contains(x.Type_Dw)).Count();//其他贷款发生过90天以上逾期的账户数
                statln.LOANDELAY90CNT = statln.ALLLOANHOUSEDELAY90CNT + statln.ALLLOANOTHERDELAY90CNT;//贷款逾期90天以上账户数
                //后续添加上
                //statln.ALL_LOAN_HOUSE_FOROTHERS_CNT = tRows1[2].ToInt();//为他人担保笔数住房贷款发生过逾期的账户数
                //statln.ALL_LOAN_OTHER_FOROTHERS_CNT = tRows1[3].ToInt();//为他人担保笔数其他贷款发生过逾期的账户数
            }
            if (lndList != null)
            {
                specialValue = GetSpecialValue(false);
                if (specialValue != 0)
                    statlnd.ALLCREDITUNCLOSEDCNT = specialValue;
                else
                    statlnd.ALLCREDITUNCLOSEDCNT = lndList.Where(x => x.State != xiaohu).Select(x => x.Open_Date).Distinct().Count();//信用卡未销户账户数
                statlnd.ALLCREDITDELAYCNT = lndList.Count(x => x.LndoverList.Count > 0);//信用卡发生过逾期的账户数

                statlnd.ALLCREDITDELAY90CNT = lndList.Count(x => x.LndoverList.Count(y => y.Last_Months >= 3) > 0);//信用卡发生过90天以上逾期的账户数
                //后续添加上
                // statlnd.ALL_CREDIT_FOROTHERS_CNT = tRows1[1].ToInt();//为他人担保笔数信用卡发生过逾期的账户数

            }

            //查询记录统计
            statqr.M3ALLCNTTOTAL = recordInfoList.Where(o => (query_Time.Value.Year - o.Query_Date.Value.Year) * 12 + (query_Time.Value.Month - o.Query_Date.Value.Month) 
               <3 && (o.Query_Reason == "信用卡审批" || o.Query_Reason == "贷款审批")).Count();
            statqr.M3CREDITCNT = recordInfoList.Where(o => o.Query_Date >= query_Time.Value.AddMonths(-3) && (o.Query_Reason == "信用卡审批")).Count();
            statqr.M3LOANCNT = recordInfoList.Where(o => o.Query_Date >= query_Time.Value.AddMonths(-3) && o.Query_Reason == "贷款审批").Count();

            #endregion


            #region 信用卡、贷款、担保贷款明细


            #region 准贷记卡
            foreach (var crdstncard in stncardList)
            {
                if (crdstncard.Curr_Overdue_Amount != null)
                {
                    credit_dlq_amount += (decimal)crdstncard.Curr_Overdue_Amount;//逾期金额
                }
           
                //最近九个月发放贷记卡统计
                if (crdstncard.Open_Date > NowDate.AddMonths(-9) && crdstncard.Currency == rmb)
                {
                    M9_LEND_AMOUNT += (decimal)crdstncard.Credit_Limit_Amount;
                    M9_DELIVER_CNT++;
                }

                if (crdstncard.State == zhengchang)
                    NORMAL_CARDNUM++;
                else
                    continue;
                //正常信用卡未用额度
                // NORMAL_CREDIT_BALANCE += crdstncard.Credit_Limit_Amount ?? 0 - crdstncard.Used_Credit_Limit_Amount ?? 0;
                if (crdstncard.Currency == rmb)
                {
                    //正常信用卡最大额度
                    if (crdstncard.Credit_Limit_Amount > CREDIT_LIMIT_AMOUNT_NORM_MAX)
                    {
                        CREDIT_LIMIT_AMOUNT_NORM_MAX = (decimal)crdstncard.Credit_Limit_Amount;
                    }
                    SUM_NORMAL_LIMIT_AMOUNT += (decimal)crdstncard.Credit_Limit_Amount;//正常信用卡总额度,不包括美元账户
                }
                //正常信用卡使用额度
                SUM_NORMAL_USE_LIMIT_AMOUNT += (decimal)crdstncard.Used_Credit_Limit_Amount;
                //正常信用卡最大已使用额度
                if (NORMAL_USED_MAX < (decimal)crdstncard.Used_Credit_Limit_Amount)
                {
                    NORMAL_USED_MAX = (decimal)crdstncard.Used_Credit_Limit_Amount;
                }
                if (NormalCreditOpenTime > crdstncard.Open_Date)
                {
                    NormalCreditOpenTime = (DateTime)crdstncard.Open_Date;
                }

            }

            #endregion

            #region 贷记卡
            foreach (var crdlnd in lndList)
            {
                credit_dlq_amount += crdlnd.Curr_Overdue_Amount ?? 0;//逾期金额
                if (CreditOpenTime > crdlnd.Open_Date)
                {
                    CreditOpenTime = (DateTime)crdlnd.Open_Date;
                }
                //最近九个月发放贷记卡统计
                if (crdlnd.Open_Date > NowDate.AddMonths(-9) && crdlnd.Currency == rmb)
                {
                    M9_LEND_AMOUNT += (decimal)crdlnd.Credit_Limit_Amount;
                    M9_DELIVER_CNT++;
                }
                //逾期账户数
                if (crdlnd.Curr_Overdue_Amount > 0)
                {
                    CREDIT_DELAY_CNT++;
                }

                if (crdlnd.State == zhengchang)
                    NORMAL_CARDNUM++;
                else
                    continue;
                //正常信用卡未用额度
                //NORMAL_CREDIT_BALANCE += crdlnd.Credit_Limit_Amount ?? 0 - crdlnd.Used_Credit_Limit_Amount ?? 0;

                if (crdlnd.Currency == rmb)
                {
                    //正常信用卡最大额度
                    if (crdlnd.Credit_Limit_Amount > CREDIT_LIMIT_AMOUNT_NORM_MAX)
                    {
                        CREDIT_LIMIT_AMOUNT_NORM_MAX = (decimal)crdlnd.Credit_Limit_Amount;
                    }
                    SUM_NORMAL_LIMIT_AMOUNT += (decimal)crdlnd.Credit_Limit_Amount; //正常信用卡总额度,不包括美元账户
                }
                if (crdlnd.Used_Credit_Limit_Amount != null && crdlnd.Used_Credit_Limit_Amount > 0)
                {
                    //正常信用卡使用额度
                    SUM_NORMAL_USE_LIMIT_AMOUNT += (decimal)crdlnd.Used_Credit_Limit_Amount;
                    //正常信用卡最大已使用额度
                    if (NORMAL_USED_MAX < (decimal)crdlnd.Used_Credit_Limit_Amount)
                    {
                        NORMAL_USED_MAX = (decimal)crdlnd.Used_Credit_Limit_Amount;
                    }
                }

                if (NormalCreditOpenTime > crdlnd.Open_Date)
                {
                    NormalCreditOpenTime = (DateTime)crdlnd.Open_Date;
                }

            }

            #endregion


            #region 贷款
            foreach (var crdln in lnList)
            {
                loan_dlq_amount += crdln.Curr_Overdue_Amount ?? 0;
                //比较是否最早贷款
                if (LoanOpenTime > crdln.Open_Date)
                {
                    LoanOpenTime = (DateTime)crdln.Open_Date;
                }
                SUM_LOAN_LIMIT_AMOUNT += (decimal)crdln.Credit_Limit_Amount;//贷款总额

                if (crdln.Balance != null)
                {
                    SUM_LOAN_BALANCE += loan_Balance;
                }
                if (crdln.State != zhengchang)
                    continue;
                int surplusMonth = 0;//剩余期数
                int intervalMonth = 0;
                if (crdln.End_Date != null)
                {
                    intervalMonth = CommonFun.GetIntervalOf2DateTime((DateTime)crdln.Open_Date, (DateTime)crdln.Open_Date, "M");
                    if (crdln.GetTime != null)
                    {
                        surplusMonth = CommonFun.GetIntervalOf2DateTime(crdln.End_Date.Value.ToString(Consts.DateFormatString8).ToDateTime().Value, crdln.GetTime.Value, "M");
                        surplusMonth++;
                    }

                }
                //期数大于0且余额大于0
                if (intervalMonth > 0 && crdln.Balance > 0)
                {
                    #region 判断每月还息，最后还本的情况 2016-04-14

                    if (surplusMonth < intervalMonth && crdln.Balance == crdln.Credit_Limit_Amount)//每月还息，最后一次还本
                    {
                        loan_pmt_monthly += 0;
                        loan_house_pmt_monthly += 0;
                    }
                    else
                    {
                        loan_pmt_monthly += (decimal)(crdln.Credit_Limit_Amount / intervalMonth);//贷款每月还款金额
                        //住房贷款每月还款金额
                        if (crdln.Type_Dw == "住房贷款")
                        {
                            loan_house_pmt_monthly += (decimal)(crdln.Credit_Limit_Amount / intervalMonth);
                        }
                    }
                    #endregion
                }
                //比较是否未还清最早贷款
                if (NormalLoanOpenTime > crdln.Open_Date)
                {
                    NormalLoanOpenTime = (DateTime)crdln.Open_Date;
                }

            }
            #endregion


            #endregion

            #region 信用卡相关
            statlnd.CREDITDLQAMOUNT = credit_dlq_amount;//信用卡逾期金额
            statlnd.NORMALCARDNUM = NORMAL_CARDNUM;//正常卡数据量
            statlnd.NORMALCREDITBALANCE = SUM_NORMAL_LIMIT_AMOUNT - SUM_NORMAL_USE_LIMIT_AMOUNT;//正常信用卡未用额度
            statlnd.SUMNORMALLIMITAMOUNT = SUM_NORMAL_LIMIT_AMOUNT;//正常信用卡总信用额度
            statlnd.NORMALUSEDMAX = NORMAL_USED_MAX;//正常信用卡最大已使用额度
            //正常信用卡使用率
            if (SUM_NORMAL_LIMIT_AMOUNT > 0)
            {
                statlnd.NORMALUSEDRATE = SUM_NORMAL_USE_LIMIT_AMOUNT / SUM_NORMAL_LIMIT_AMOUNT;
            }
            else
            {
                statlnd.NORMALUSEDRATE = CommonData._9999998;
            }
            statlnd.CREDITLIMITAMOUNTNORMMAX = CREDIT_LIMIT_AMOUNT_NORM_MAX;//正常信用卡最大额度
            statlnd.CARDAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, CreditOpenTime, "M");//最早卡龄
            statlnd.NORMALCARDAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, NormalCreditOpenTime, "M");//正常卡最早卡龄
            statlnd.SUMUSEDCREDITLIMITAMOUNT = SUM_NORMAL_USE_LIMIT_AMOUNT;//信用卡已使用总额度

            statlnd.M9LENDAMOUNT = M9_LEND_AMOUNT;//最近九个月发放贷记卡数量
            statlnd.M9DELIVERCNT = M9_DELIVER_CNT;//最近九个月发放贷记卡总金额
            //最近九个月发放贷记卡平均金额
            if (M9_DELIVER_CNT > 0)
            {
                statlnd.M9AVGLENDAMOUNT = M9_LEND_AMOUNT / M9_DELIVER_CNT;
            }
            else
            {
                statlnd.M9AVGLENDAMOUNT = 0;
            }
            statlnd.CREDITDELAYCNT = CREDIT_DELAY_CNT;//逾期账户数

            //判断是否有信用 卡,没有的话赋默认值
            if (lndList.Count == 0 && stncardList.Count == 0)
            {
                statlnd.M9AVGLENDAMOUNT = CommonData._9999999;
                statlnd.M9DELIVERCNT = CommonData._9999999;
                statlnd.M9LENDAMOUNT = CommonData._9999999;
                statlnd.SUMUSEDCREDITLIMITAMOUNT = CommonData._9999999;
                statlnd.CARDAGEMONTH = CommonData._9999999;
                statlnd.NORMALCREDITBALANCE = CommonData._9999999;
                statlnd.NORMALUSEDRATE = CommonData._9999999;
            }
            #endregion

            #region 贷款相关
            statln.LOANAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, LoanOpenTime, "M");//最早贷龄
            statln.NORMALLOANAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, NormalLoanOpenTime, "M");//未还清的最早贷龄
            statln.LOANDLQAMOUNT = loan_dlq_amount;//贷款逾期金额
            statln.LOANPMTMONTHLY = loan_pmt_monthly;//贷款每月还款本金金额
            statln.LOAN_HOUSE_DLQ_AMOUNT = loan_house_pmt_monthly;//房贷每月还款本金金额
            statln.SUMLOANBALANCE = SUM_LOAN_BALANCE;//贷款总余额
            statln.SUMLOANLIMITAMOUNT = SUM_LOAN_LIMIT_AMOUNT;//贷款本金总额

            //判断是否有贷款,没有的话赋默认值
            if (lnList.Count == 0)
            {
                statln.LOANAGEMONTH = CommonData._9999999;
                statln.LOANDLQAMOUNT = CommonData._9999999;
                statln.LOANPMTMONTHLY = CommonData._9999999;
            }
            #endregion

            #endregion

            #region 未入库变量计算

            #region 贷款统计信息
            int ALL_LOAN_DELAY_MONTH = 0;//贷款5年逾期的月数
            decimal ln_housing_fund_amount = 0;//个人住房公积金贷款额
            decimal ln_shopfront_amount = 0;//个人住房商铺贷款额
            decimal ln_housing_mortgage_amount = 0;//个人住房按揭贷款额
            int ln_normal_count = 0;//状态为正常的贷款笔数

            if (statln != null)
            {
                //需要优化
                ALL_LOAN_DELAY_MONTH = lnList.Where(o => o.Overdue_Cyc > 0).Select(o => (int)o.Overdue_Cyc).Sum();
                ln_housing_fund_amount = lnList.Where(o => o.Type_Dw == "个人住房公积金贷款" && o.State == zhengchang).Select(o => (decimal)o.Credit_Limit_Amount).Sum();
                ln_shopfront_amount = lnList.Where(o => o.Type_Dw == "个人住房商铺贷款" && o.State == zhengchang).Select(o => (decimal)o.Credit_Limit_Amount).Sum();
                ln_housing_mortgage_amount = lnList.Where(o => o.Type_Dw == "住房贷款" && o.State == zhengchang).Select(o => (decimal)o.Credit_Limit_Amount).Sum();
                ln_normal_count = lnList.Where(o => o.State == zhengchang).Count();

                statln.ALL_LOAN_DELAY_MONTH = ALL_LOAN_DELAY_MONTH;
                statln.ln_housing_fund_amount = ln_housing_fund_amount;
                statln.ln_shopfront_amount = ln_shopfront_amount;
                statln.ln_housing_mortgage_amount = ln_housing_mortgage_amount;
                statln.ln_normal_count = ln_normal_count;
            }

            #endregion

            #region 贷记卡统计信息

            int ALL_CREDIT_DELAY_MONTH = 0;//贷记卡5年逾期的月数
            int loand_Badrecord = 0;//贷记卡呆账数
            int stncard_Badrecord = 0;//准贷记卡呆账数
            decimal StnCard_UseCreditLimit = 0;//准贷记卡透支余额
            decimal lnd_max_overdue_percent = 0;//贷记卡最大逾期次数占比
            int lnd_max_normal_Age = 0;//正常状态最大授信额度卡的卡龄

            ALL_CREDIT_DELAY_MONTH = (int)lndList.Where(o => o.Overdue_Cyc > 0).Select(o => (int)o.Overdue_Cyc).Sum();
            loand_Badrecord = (int)lndList.Where(o => o.State == "呆账").Count();
            stncard_Badrecord = (int)stncardList.Where(o => o.State == "呆账").Count();
            StnCard_UseCreditLimit = (int)stncardList.Where(o => o.Used_Credit_Limit_Amount > 0).Select(o => o.Used_Credit_Limit_Amount).Sum();

            int usemonth = 0;
            DateTime nowdate = DateTime.Now;
            decimal tempoverdue = 0;
            foreach (var item in lndList)
            {
                if (item.Overdue_Cyc > 0 && item.Open_Date != null)
                {
                    usemonth = CommonFun.GetIntervalOf2DateTime(nowdate, (DateTime)item.Open_Date, "M");
                    if (usemonth > 60)
                    {
                        usemonth = 60;
                    }
                    if (usemonth == 0)
                    {
                        continue;
                    }
                    tempoverdue = (decimal)item.Overdue_Cyc / usemonth;
                    if (lnd_max_overdue_percent < tempoverdue)
                    {
                        lnd_max_overdue_percent = tempoverdue;
                    }
                }
            }
            //正常状态最大授信额度卡的卡龄
            if (statlnd.CREDITLIMITAMOUNTNORMMAX > 0)
            {
                var templnd = lndList.Where(o => o.Credit_Limit_Amount == statlnd.CREDITLIMITAMOUNTNORMMAX).FirstOrDefault();
                if (templnd != null)
                {
                    lnd_max_normal_Age = CommonFun.GetIntervalOf2DateTime((DateTime)query_Time, (DateTime)templnd.Open_Date, "M");
                }
            }

            statlnd.ALL_CREDIT_DELAY_MONTH = ALL_CREDIT_DELAY_MONTH;
            statlnd.loand_Badrecord = loand_Badrecord;
            statlnd.stncard_Badrecord = stncard_Badrecord;
            statlnd.StnCard_UseCreditLimit = StnCard_UseCreditLimit;
            statlnd.lnd_max_overdue_percent = lnd_max_overdue_percent;
            statlnd.lnd_max_normal_Age = lnd_max_normal_Age;
            statlnd.CardNum = lndList.Select(e => e.Open_Date).Distinct().Count();
            #endregion



            #region 卡卡贷网络版征信评分字段决策配置(hering)

            //银行信贷涉及的账户数
            statln.ACCT_NUM = lndList.Select(e => e.Finance_Org + e.Open_Date).Distinct().Count() + lnList.Count;
            //五年内最大逾期次数
            var DLQ_5YR_CNT_MAX = lndList.OrderByDescending(o => o.Overdue_Cyc).FirstOrDefault();
            statlnd.DLQ_5YR_CNT_MAX = DLQ_5YR_CNT_MAX != null ? DLQ_5YR_CNT_MAX.Overdue_Cyc * (2.5) : 0;

            //过去贷款结清数
            statln.LST_LOAN_CLS_CNT = lnList.Count(o => o.State == jieqing);
            //过去贷款开户数
            statln.LST_LOAN_OPE_CNT = lnList.Count;
            //非银行机构的贷款审批查询次数
            statqr.NON_BNK_LN_QRY_CNT = recordInfoList.Count(o => o.Query_Reason == "贷款审批" && (o.Querier.IndexOf("银行") == -1 && o.Querier.IndexOf("花旗") == -1));
            //最近正常贷记卡授信额度
            var CreditLimitRencent = lndList.Where(o => o.State == zhengchang).OrderByDescending(t => t.Open_Date).ThenByDescending(d => d.Credit_Limit_Amount).ToList();
            statlnd.RCNT_CDT_LMT = CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].Credit_Limit_Amount : decimal.Zero;
            //最近未销户贷记卡额度
            CreditLimitRencent = lndList.Where(o => o.State != xiaohu).OrderByDescending(t => t.Open_Date).ThenByDescending(d => d.Credit_Limit_Amount).ToList();
            statlnd.UNCLS_RCNT_CDT_LMT = CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].Credit_Limit_Amount : decimal.Zero;
            //最早未销户贷记卡额度
            CreditLimitRencent = lndList.Where(o => o.State != xiaohu).OrderBy(f => f.Open_Date).ThenByDescending(d => d.Credit_Limit_Amount).ToList();
            statlnd.UNCLS_OLD_CDT_LMT = CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].Credit_Limit_Amount : decimal.Zero;

            #endregion

            #region 新增需求(Kerry)
            if (lnList.Count == 0 && lndList.Count == 0 && stncardList.Count == 0 && recordInfoList.Count == 0)
            {
                statlnd.L12M_LOANACT_USED_MAX_R = CommonData._9999999;
                statln.L24M_OPE_NORM_ACCT_PCT = CommonData._9999999;
                statlnd.NORM_CDT_BAL_USED_PCT_AVG = CommonData._9999999;
                statlnd.CRD_AGE_UNCLS_OLDEST = CommonData._9999999;
                statqr.L3M_ACCT_QRY_NUM = CommonData._9999999;
                statqr.L3M_LN_QRY_NUM = CommonData._9999999;
            }
            else
            {

                DateTime query_time = new DateTime();
                query_time = query_Time.Value;

                var L3M_ACCT_QRY_NUM = 0;
                var L3M_LN_QRY_NUM = 0;
                foreach (var record in recordInfoList)
                {
                    if (record.Query_Reason != null)
                    {
                        var time = query_time.Year * 12 + query_time.Month - ((DateTime)record.Query_Date).Year * 12 - ((DateTime)record.Query_Date).Month;
                        //var time = query_time.Year * 1200 + query_time.Month * 100 + query_time.Day - ((DateTime)record.QueryDate).Year * 1200 - ((DateTime)record.QueryDate).Month * 100 - ((DateTime)record.QueryDate).Day;
                        //if (time < 400)
                        if (time < 3)
                        {
                            if (record.Query_Reason == "信用卡审批")
                                L3M_ACCT_QRY_NUM++;
                            if (record.Query_Reason == "贷款审批")
                                L3M_LN_QRY_NUM++;
                        }
                    }
                }
                statqr.L3M_ACCT_QRY_NUM = L3M_ACCT_QRY_NUM;//过去3个月信用卡审批查询次数
                statqr.L3M_LN_QRY_NUM = L3M_LN_QRY_NUM;//过去3个月贷款审批查询次数
                //entity.L3M_ACCT_QRY_NUM = statqr.M3CREDITCNT == null ? 0 : (int)statqr.M3CREDITCNT;//过去3个月信用卡审批查询次数
                //entity.L3M_LN_QRY_NUM = statqr.M3LOANCNT == null ? 0 : (int)statqr.M3LOANCNT;//过去3个月贷款审批查询次数

                // L12M_LOANACT_USED_MAX_R
                #region L12M_LOANACT_USED_MAX_R
                if (lnList.Count == 0)
                {
                    statlnd.L12M_LOANACT_USED_MAX_R = CommonData._9999998;
                }
                else if (query_Time == null)
                {
                    statlnd.L12M_LOANACT_USED_MAX_R = CommonData._9999997;
                }
                else
                {
                    decimal L12M_CNT = 0;
                    decimal L12M_LOANACT_USED_MAX = 0;
                    decimal L12M_LMT_ALL = 0;
                    foreach (var ln in lnList)
                    {
                        if (ln.Open_Date != null)
                        {
                            if ((int.Parse(query_time.ToString("yyyyMMdd")) - int.Parse(((DateTime)ln.Open_Date).ToString("yyyyMMdd"))) <= 10000)
                            {
                                L12M_CNT++;
                                if (ln.Balance != null)
                                    L12M_LOANACT_USED_MAX = L12M_LOANACT_USED_MAX > (decimal)ln.Balance ? L12M_LOANACT_USED_MAX : (decimal)ln.Balance;
                                L12M_LMT_ALL += (decimal)ln.Credit_Limit_Amount;
                            }
                        }
                    }
                    if (L12M_CNT == 0)
                    {
                        statlnd.L12M_LOANACT_USED_MAX_R = CommonData._9999996;
                    }
                    else if (L12M_LMT_ALL == 0)
                    {
                        statlnd.L12M_LOANACT_USED_MAX_R = CommonData._9999995;
                    }
                    else
                    {
                        var L12M_LOANACT_USED_MAX_R = L12M_LOANACT_USED_MAX / (L12M_LMT_ALL / L12M_CNT);
                        statlnd.L12M_LOANACT_USED_MAX_R = decimal.Parse(L12M_LOANACT_USED_MAX_R.ToString("#0.00"));//过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
                        //entity.L12M_LOANACT_USED_MAX_R = L12M_LOANACT_USED_MAX == 0 ? 0 : decimal.Parse(L12M_LOANACT_USED_MAX_R.ToString("#0.00").TrimEnd(new char[] { '0', '.' }));//过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
                    }
                }
                #endregion
                if (lndList.Count == 0)
                {
                    statln.L24M_OPE_NORM_ACCT_PCT = CommonData._8888887;
                    statlnd.NORM_CDT_BAL_USED_PCT_AVG = CommonData._8888887;
                    statlnd.CRD_AGE_UNCLS_OLDEST = CommonData._8888888;
                }
                else
                {
                    // CRD_AGE_UNCLS_OLDEST
                    #region CRD_AGE_UNCLS_OLDEST
                    var unclslndlist = lndList.Where(lnd => !lnd.State.Contains(xiaohu));
                    int CRD_AGE_UNCLS_OLDEST = 0;
                    if (query_Time != null)
                    {
                        foreach (var lnd in unclslndlist)
                        {
                            if (lnd.Open_Date != null)
                            {
                                var CRD_AGE = query_time.Year * 12 + query_time.Month - ((DateTime)lnd.Open_Date).Year * 12 - ((DateTime)lnd.Open_Date).Month;
                                if (CRD_AGE_UNCLS_OLDEST < CRD_AGE)
                                    CRD_AGE_UNCLS_OLDEST = CRD_AGE;
                            }
                        }
                    }
                    statlnd.CRD_AGE_UNCLS_OLDEST = CRD_AGE_UNCLS_OLDEST;//最早未销户卡龄
                    #endregion
                    // L24M_OPE_NORM_ACCT_PCT & NORM_CDT_BAL_USED_PCT_AVG
                    #region L24M_OPE_NORM_ACCT_PCT & NORM_CDT_BAL_USED_PCT_AVG（这里出了问题 ？)
                    var normallndlist = lndList.Where(lnd => lnd.State.Contains(zhengchang));
                    var filterlndlist = new List<CRD_CD_LNDEntity>();
                    foreach (var lnd in normallndlist)
                    {
                        var templnd = filterlndlist.Where(flnd => flnd.Open_Date == lnd.Open_Date).FirstOrDefault();
                        if (templnd != null)
                        {
                            var tlnd = DeepCopy(lnd);
                            if (lnd.Credit_Limit_Amount < templnd.Credit_Limit_Amount)
                            {
                                tlnd.Credit_Limit_Amount = templnd.Credit_Limit_Amount;
                            }
                            decimal NORM_CDT_USED = (decimal)lnd.Used_Credit_Limit_Amount;
                            NORM_CDT_USED += (decimal)templnd.Used_Credit_Limit_Amount;
                            tlnd.Used_Credit_Limit_Amount = NORM_CDT_USED;//在修改UserCreditLimitAmount时会将lndList中的此字段修改（原因？？？）
                            filterlndlist.Remove(templnd);
                            filterlndlist.Add(tlnd);
                        }
                        else
                        {
                            filterlndlist.Add(lnd);
                        }
                    }
                    decimal NOW_NORM_UNI_ACCT_NUM = filterlndlist.Count();
                    decimal L24M_OPE_ACCT_NUM = 0;
                    List<decimal> LIST_NORM_CDT_BAL_USED_PCT = new List<decimal>();
                    var templndlist = new List<CRD_CD_LNDEntity>();
                    foreach (var lnd in lndList)
                    {
                        if (string.IsNullOrEmpty(lnd.Currency))
                            continue;
                        var templnd = templndlist.Where(flnd => flnd.Open_Date == lnd.Open_Date).FirstOrDefault();
                        if (templnd == null)
                        {
                            if (query_Time != null && lnd.Open_Date != null)
                            {
                                var CRD_AGE = query_time.Year * 12 + query_time.Month - ((DateTime)lnd.Open_Date).Year * 12 - ((DateTime)lnd.Open_Date).Month;
                                if (0 < CRD_AGE && CRD_AGE < 24)
                                {
                                    L24M_OPE_ACCT_NUM++;
                                }
                            }
                            templndlist.Add(lnd);
                        }
                    }
                    foreach (var lnd in filterlndlist)
                    {
                        decimal CDT_LMT_SUM = (decimal)lnd.Credit_Limit_Amount;
                        decimal NORM_CDT_USED = (decimal)lnd.Used_Credit_Limit_Amount;
                        decimal SUM_NORM_CDT_USED = CDT_LMT_SUM - NORM_CDT_USED;
                        decimal NORM_CDT_BAL_USED_PCT = 0;
                        if (SUM_NORM_CDT_USED != 0)
                        {
                            NORM_CDT_BAL_USED_PCT = NORM_CDT_USED / SUM_NORM_CDT_USED;
                            LIST_NORM_CDT_BAL_USED_PCT.Add(NORM_CDT_BAL_USED_PCT);
                        }
                    }
                    if (NOW_NORM_UNI_ACCT_NUM == 0)
                    {
                        statln.L24M_OPE_NORM_ACCT_PCT = CommonData._9999998;
                    }
                    else
                    {
                        var L24M_OPE_NORM_ACCT_PCT = L24M_OPE_ACCT_NUM / NOW_NORM_UNI_ACCT_NUM;
                        statln.L24M_OPE_NORM_ACCT_PCT = decimal.Parse(L24M_OPE_NORM_ACCT_PCT.ToString("#0.00"));//过去24个月内非共享信用卡账户开户数占当前正常账户比例
                        //entity.L24M_OPE_NORM_ACCT_PCT = L24M_OPE_NORM_ACCT_PCT == 0 ? 0 : decimal.Parse(L24M_OPE_NORM_ACCT_PCT.ToString("#0.00").TrimEnd(new char[] { '0', '.' }));//过去24个月内非共享信用卡账户开户数占当前正常账户比例
                    }
                    decimal NORM_CDT_BAL_USED_PCT_AVG = 0;
                    if (normallndlist.Count() == 0 || LIST_NORM_CDT_BAL_USED_PCT.Count == 0)
                    {
                        statlnd.NORM_CDT_BAL_USED_PCT_AVG = CommonData._9999998;
                    }
                    else
                    {
                        NORM_CDT_BAL_USED_PCT_AVG = LIST_NORM_CDT_BAL_USED_PCT.Sum() / LIST_NORM_CDT_BAL_USED_PCT.Count;

                        statlnd.NORM_CDT_BAL_USED_PCT_AVG = decimal.Parse(NORM_CDT_BAL_USED_PCT_AVG.ToString("#0.00"));//当前正常的信用卡账户最大负债额与透支余额之比的均值
                        //entity.NORM_CDT_BAL_USED_PCT_AVG = NORM_CDT_BAL_USED_PCT_AVG == 0 ? 0 : decimal.Parse(NORM_CDT_BAL_USED_PCT_AVG.ToString("#0.00").TrimEnd(new char[] { '0', '.' }));//当前正常的信用卡账户最大负债额与透支余额之比的均值
                    }

                    #endregion
                }
            }
            #endregion

            #region statln新增变量6个（Kerry 20160524）
            //房屋贷款年化率为4%
            //住房贷款
            var ln_Mortgage = lnList.Where(o => o.Type_Dw == "住房贷款" && o.State == zhengchang);
            decimal Monthly_Mortgage_Payment = 0;
            decimal Monthly_Mortgage_Payment_Total = 0;
            statln.Monthly_Mortgage_Payment_Max = 0;
            foreach (CRD_CD_LNEntity ln in ln_Mortgage)
            {
                if (ln.Payment_Rating != "按月归还")
                {
                    continue;
                }
                int Mortgage_Month = (((DateTime)ln.End_Date).Year - ((DateTime)ln.Open_Date).Year) * 12 + (((DateTime)ln.End_Date).Month - ((DateTime)ln.Open_Date).Month);

                if (Mortgage_Month == 0)
                {
                    Mortgage_Month++;
                }
                decimal month_rate = ((decimal)0.04) / 12;
                Monthly_Mortgage_Payment = (decimal)(ln.Credit_Limit_Amount * month_rate * factorial((1 + month_rate), Mortgage_Month)) / (factorial((1 + month_rate), Mortgage_Month) - 1);//等额本息月款额=[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]，月利率=年利率/12
                if (statln.Monthly_Mortgage_Payment_Max < Monthly_Mortgage_Payment)
                {
                    statln.Monthly_Mortgage_Payment_Max = Monthly_Mortgage_Payment;//最大月按揭还款额
                }
                Monthly_Mortgage_Payment_Total += Monthly_Mortgage_Payment;
            }
            statln.Monthly_Mortgage_Payment_Total = Monthly_Mortgage_Payment_Total;//月按揭还款总额

            //个人商用房
            var ln_Commercial = lnList.Where(o => o.Type_Dw == "个人商用房" && o.State == zhengchang);
            decimal Monthly_Commercial_Mortgage_Payment = 0;
            decimal Monthly_Commercial_Mortgage_Payment_Total = 0;
            statln.Monthly_Commercial_Mortgage_Payment_Max = 0;
            foreach (CRD_CD_LNEntity ln in ln_Commercial)
            {
                if (ln.Payment_Rating != "按月归还")
                {
                    continue;
                }
                int Commercial_Month = (((DateTime)ln.End_Date).Year - ((DateTime)ln.Open_Date).Year) * 12 + (((DateTime)ln.End_Date).Month - ((DateTime)ln.Open_Date).Month);
                if (Commercial_Month == 0)
                {
                    Commercial_Month++;
                }
                decimal month_rate = ((decimal)0.04) / 12;
                Monthly_Commercial_Mortgage_Payment = (decimal)(ln.Credit_Limit_Amount * month_rate * factorial((1 + month_rate), Commercial_Month)) / (factorial((1 + month_rate), Commercial_Month) - 1);//等额本息月款额=[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]，月利率=年利率/12
                if (statln.Monthly_Commercial_Mortgage_Payment_Max < Monthly_Commercial_Mortgage_Payment)
                {
                    statln.Monthly_Commercial_Mortgage_Payment_Max = Monthly_Commercial_Mortgage_Payment;//最大商用房月按揭还款额
                }
                Monthly_Commercial_Mortgage_Payment_Total += Monthly_Commercial_Mortgage_Payment;
            }
            statln.Monthly_Commercial_Mortgage_Payment_Total = Monthly_Commercial_Mortgage_Payment_Total;//商用房月按揭还款总额

            //其他贷款
            decimal ln_other_amount = 0;
            decimal Monthly_Other_Mortgage_Payment = 0;
            decimal Monthly_Other_Mortgage_Payment_Total = 0;
            var ln_other = lnList.Where(o => o.Type_Dw != "个人商用房" && o.Type_Dw != "住房贷款" && o.Type_Dw != "个人住房公积金贷款" && (o.State == zhengchang));
            foreach (CRD_CD_LNEntity ln in ln_other)
            {
                if (ln.Payment_Rating != "按月归还")
                {
                    continue;
                }
                if ((decimal)ln.Balance == 0)//若余额为0（报告滞后，应算作结清，不予统计）
                {
                    continue;
                }
                //if (ln.Cue.Contains("截至"))//描述中是否存在“截至”，即是否有截至年月
                //{
                //    try
                //    {
                //        string CalEndMonth_Str = CommonFun.GetMidStr(ln.Cue, "截至", "，");//截至年月字符串
                //        if (!string.IsNullOrEmpty(CalEndMonth_Str))//截至年月是否为空
                //        {
                //            int CalEndMonth = int.Parse(CommonFun.GetMidStr(CalEndMonth_Str, "", "年")) * 100 + int.Parse(CommonFun.GetMidStr(CalEndMonth_Str, "年", "月"));//截至年月
                //            if (CalEndMonth == int.Parse(((DateTime)ln.End_Date).ToString("yyyyMM")) && (decimal)ln.Credit_Limit_Amount == (decimal)ln.Balance)//到期月=截止月，余额等于本金，全额计入
                //            {
                //                Monthly_Other_Mortgage_Payment_Total += (decimal)ln.Credit_Limit_Amount;//全额计入
                //                ln_other_amount += (decimal)ln.Credit_Limit_Amount;
                //                continue;
                //            }
                //            else if (CalEndMonth > int.Parse(((DateTime)ln.Open_Date).AddMonths(1).ToString("yyyyMM")) && (decimal)ln.Credit_Limit_Amount == (decimal)ln.Balance)//贷款到期月不等于截止月，且截止月>=2+贷款发放月，并且发放的贷款本金=余额，则此类贷款记0
                //            {
                //                ln_other_amount += (decimal)ln.Credit_Limit_Amount;
                //                continue;
                //            }
                //        }
                //        else
                //        {
                //            continue;
                //        }
                //    }
                //    catch
                //    {
                //        continue;
                //    }
                //}
                int Mortgage_Month = (((DateTime)ln.End_Date).Year - ((DateTime)ln.Open_Date).Year) * 12 + (((DateTime)ln.End_Date).Month - ((DateTime)ln.Open_Date).Month);
                if (Mortgage_Month == 0)
                {
                    Mortgage_Month++;
                }
                decimal month_rate = ((decimal)0.06) / 12;
                ln_other_amount += (decimal)ln.Credit_Limit_Amount;
                //try
                //{
                Monthly_Other_Mortgage_Payment = (decimal)(ln.Credit_Limit_Amount * month_rate * factorial((1 + month_rate), Mortgage_Month)) / (factorial((1 + month_rate), Mortgage_Month) - 1);
                //}
                //catch
                //{
                //    Monthly_Other_Mortgage_Payment_Total = 9999999;
                //    break;
                //}
                Monthly_Other_Mortgage_Payment_Total += Monthly_Other_Mortgage_Payment;
            }
            statln.ln_other_amount = ln_other_amount;//其他贷款额
            statln.Monthly_Other_Mortgage_Payment_Total = Monthly_Other_Mortgage_Payment_Total;

            #endregion

            #region 保证人代偿
            var asrrepayList = new CRD_CD_ASRREPAYBusiness().GetList(reportentity.Report_Id);
            statln.assurerrepay_amount = (decimal)asrrepayList.Where(o => o.Money != null).Sum(o => o.Money);
            #endregion

            #region statln新增变量2个（Kerry 20160617）
            var ln_AccumFund = lnList.Where(o => o.Type_Dw == "个人住房公积金贷款" && o.State != jieqing);
            decimal AccumFund_Mon_Mort_Repay = 0;
            decimal Max_AccumFund_All_Mort_Repay = 0;
            statln.Max_AccumFund_Mon_Mort_Repay = 0;
            foreach (CRD_CD_LNEntity ln in ln_AccumFund)
            {
                if (ln.Payment_Rating != "按月归还")
                {
                    continue;
                }
                int AccumFund_Month = (((DateTime)ln.End_Date).Year - ((DateTime)ln.Open_Date).Year) * 12 + (((DateTime)ln.End_Date).Month - ((DateTime)ln.Open_Date).Month);

                if (AccumFund_Month == 0)
                {
                    AccumFund_Month++;
                }
                decimal month_rate = ((decimal)0.03) / 12;
                AccumFund_Mon_Mort_Repay = (decimal)(ln.Credit_Limit_Amount * month_rate * factorial((1 + month_rate), AccumFund_Month)) / (factorial((1 + month_rate), AccumFund_Month) - 1);//等额本息月款额=[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]，月利率=年利率/12
                if (statln.Max_AccumFund_Mon_Mort_Repay < AccumFund_Mon_Mort_Repay)
                {
                    statln.Max_AccumFund_Mon_Mort_Repay = AccumFund_Mon_Mort_Repay;//最大月按揭还款额
                }
                Max_AccumFund_All_Mort_Repay += AccumFund_Mon_Mort_Repay;
            }
            statln.Max_AccumFund_All_Mort_Repay = Max_AccumFund_All_Mort_Repay;//月按揭还款总额


            #endregion

            #region 豆豆钱新增变量6个（Kerry 20160624）
            if (lnList.Count == 0 && lndList.Count == 0 && stncardList.Count == 0 && recordInfoList.Count == 0)
            {
                statlnd.DLQ_5YR_CNT_AVG = CommonData._9999999;
                statlnd.NOW_NORM_UNI_ACCT_NUM = CommonData._9999999;
                statlnd.NORM_CDT_LMT_MAX = CommonData._9999999;
                statlnd.NORM_USED_SUM = CommonData._9999999;
                statlnd.NORM_CDT_SUM = CommonData._9999999;
                statlnd.CREDIT_CARD_AGE_OLDEST = CommonData._9999999;
            }
            else
            {
                if (lndList.Count == 0)
                {
                    statlnd.DLQ_5YR_CNT_AVG = CommonData._8888888;
                    statlnd.NOW_NORM_UNI_ACCT_NUM = CommonData._8888888;
                    statlnd.NORM_CDT_LMT_MAX = CommonData._8888888;
                    statlnd.NORM_USED_SUM = CommonData._8888888;
                    statlnd.NORM_CDT_SUM = CommonData._8888888;
                    statlnd.CREDIT_CARD_AGE_OLDEST = CommonData._8888888;
                }
                else
                {
                    if (lndList.Where(e => e.State == zhengchang).Count() == 0)
                    {
                        statlnd.NORM_CDT_LMT_MAX = CommonData._9999997;
                        statlnd.NOW_NORM_UNI_ACCT_NUM = 0;
                        statlnd.NORM_USED_SUM = CommonData._8888886;
                        statlnd.NORM_CDT_SUM = CommonData._8888886;
                    }
                    else
                    {
                        statlnd.NORM_CDT_LMT_MAX = lndList.Where(e => e.State == zhengchang).Max(e => e.Credit_Limit_Amount);

                        List<CRD_CD_LNDEntity> normallndlist_used = lndList.Where(e => e.State == zhengchang && e.Used_Credit_Limit_Amount > 0).ToList();
                        List<CRD_CD_LNDEntity> filterlndlist_used = new List<CRD_CD_LNDEntity>();//已使用的正常非共享信用卡列表
                        foreach (CRD_CD_LNDEntity lnd in normallndlist_used)
                        {
                            CRD_CD_LNDEntity old_lnd = filterlndlist_used.Where(e => e.Open_Date == lnd.Open_Date).FirstOrDefault();
                            if (old_lnd == null)
                            {
                                filterlndlist_used.Add(lnd);
                            }
                            else
                            {
                                if (lnd.Credit_Limit_Amount > old_lnd.Credit_Limit_Amount)
                                {
                                    filterlndlist_used.Remove(old_lnd);
                                    filterlndlist_used.Add(lnd);
                                }
                            }
                        }
                        List<CRD_CD_LNDEntity> normallndlist_all_normal = lndList.Where(e => e.State == "正常").ToList();
                        List<CRD_CD_LNDEntity> filterlndlist_all_normal = new List<CRD_CD_LNDEntity>();//正常非共享信用卡列表
                        foreach (CRD_CD_LNDEntity lnd in normallndlist_all_normal)
                        {
                            CRD_CD_LNDEntity old_lnd = filterlndlist_all_normal.Where(e => e.Open_Date == lnd.Open_Date).FirstOrDefault();
                            if (old_lnd == null)
                            {
                                filterlndlist_all_normal.Add(lnd);
                            }
                            else
                            {
                                if (lnd.Credit_Limit_Amount > old_lnd.Credit_Limit_Amount)
                                {
                                    filterlndlist_all_normal.Remove(old_lnd);
                                    filterlndlist_all_normal.Add(lnd);
                                }
                            }
                        }

                        statlnd.NOW_NORM_UNI_ACCT_NUM = filterlndlist_all_normal.Count();
                        statlnd.NORM_USED_SUM = normallndlist_used.Sum(e => e.Used_Credit_Limit_Amount) != null ? normallndlist_used.Sum(e => e.Used_Credit_Limit_Amount) : 0;
                        if (filterlndlist_used.Count != 0)
                        {
                            statlnd.NORM_CDT_SUM = filterlndlist_used.Sum(e => e.Credit_Limit_Amount);
                        }
                        else
                        {
                            statlnd.NORM_CDT_SUM = 0;
                        }

                    }
                    statlnd.DLQ_5YR_CNT_AVG = decimal.Parse(((decimal)lndList.Average(e => e.Overdue_Cyc)).ToString("#0.000"));
                    statlnd.CREDIT_CARD_AGE_OLDEST = CommonFun.GetIntervalOf2DateTime(query_Time.Value, (DateTime)(lndList.Min(e => e.Open_Date)), "M");
                }
            }
            #endregion

            #endregion

            #region 公共信息变量
            CRD_PI_ADMINPNSHMBusiness CRD_PI_ADMINPNSHMBll = new CRD_PI_ADMINPNSHMBusiness();
            CRD_PI_CIVILJDGMBusiness CRD_PI_CIVILJDGMBll = new CRD_PI_CIVILJDGMBusiness();
            CRD_PI_FORCEEXCTNBusiness CRD_PI_FORCEEXCTNBll = new CRD_PI_FORCEEXCTNBusiness();
            CRD_PI_TAXARREARBusiness CRD_PI_TAXARREARBll = new CRD_PI_TAXARREARBusiness();
            CRD_PI_TELPNTBusiness CRD_PI_TELPNTBll = new CRD_PI_TELPNTBusiness();
            vearableInfo.PubInfoSummary.AdminpnshmCount = CRD_PI_ADMINPNSHMBll.GetList((int)reportentity.Report_Id).Count;
            vearableInfo.PubInfoSummary.CiviljdgmCount = CRD_PI_CIVILJDGMBll.GetList((int)reportentity.Report_Id).Count;
            vearableInfo.PubInfoSummary.ForceexctnCount = CRD_PI_FORCEEXCTNBll.GetList((int)reportentity.Report_Id).Count;
            vearableInfo.PubInfoSummary.TaxarrearCount = CRD_PI_TAXARREARBll.GetList((int)reportentity.Report_Id).Count;
            vearableInfo.PubInfoSummary.TelpntCount = CRD_PI_TELPNTBll.GetList((int)reportentity.Report_Id).Count;
            #endregion

            #region 映射网络版征信新增变量
             specialValue= GetSpecialValue(false);             
            if (specialValue != 0)
            {
                statlnd.M24_DELIVER_CNT = specialValue;
                statlnd.M24_LEND_AMOUNT = specialValue;
                statlnd.AVG_NORMAL_LIMIT_AMOUNT = specialValue;
                statlnd.CREDIT_NORMAL_PAY_MAX = specialValue;
                statlnd.NORMALCREDITBALANCE = specialValue;
                return;
            }
            List<CRD_CD_LNDEntity> trimRepeatlndList=new List<CRD_CD_LNDEntity> ();//保留观测的数据
            foreach (var  item in lndList.GroupBy(x=>x.Open_Date))
            {
                trimRepeatlndList.Add (item.OrderByDescending(x=>x.Credit_Limit_Amount).FirstOrDefault());
            }
             
            List<CRD_CD_LNDEntity> trimRepeatNomallndList = new List<CRD_CD_LNDEntity>();//保留观测的数据
            foreach (var item in lndList.Where(x=>x.State==zhengchang).GroupBy(x => x.Open_Date))
            {
                trimRepeatNomallndList.Add(item.OrderByDescending(x => x.Credit_Limit_Amount).FirstOrDefault());
            }

            decimal  zcLndNum = 0;
            decimal  totalCreditlimitAmount = 0;
            int M24_DELIVER_CNT = 0;

            decimal M24_LEND_AMOUNT = 0;
            decimal NORMAL_CREDIT_BALANCE = 0;
            foreach (var o in  trimRepeatlndList)
            {
               var spanMonths= (query_Time.Value.Year - o.Open_Date.Value.Year) * 12 + (query_Time.Value.Month - o.Open_Date.Value.Month);
                if(spanMonths>0&&spanMonths<24)
                {
                    //近24个月发放信用卡次数
                     M24_DELIVER_CNT++;
                    //近24个月发放信用卡总额度
                    M24_LEND_AMOUNT += o.Credit_Limit_Amount??0;
                }
            }

            foreach (var item in trimRepeatNomallndList)
            {
                zcLndNum++;
                totalCreditlimitAmount += item.Credit_Limit_Amount ?? 0;
                //正常信用卡未用额度汇总
                NORMAL_CREDIT_BALANCE += (item.Credit_Limit_Amount ?? 0 - item.Used_Credit_Limit_Amount ?? 0);
            }
            statlnd.M24_DELIVER_CNT = M24_DELIVER_CNT;
            statlnd.M24_LEND_AMOUNT = M24_LEND_AMOUNT;
            statlnd.NORMALCREDITBALANCE=NORMAL_CREDIT_BALANCE;
            if(zcLndNum!=0)
            {
                //正常信用卡平均额度
                statlnd.AVG_NORMAL_LIMIT_AMOUNT = Math.Round((totalCreditlimitAmount / zcLndNum),2);
            }
            else
            {
                statlnd.AVG_NORMAL_LIMIT_AMOUNT = CommonData._8888887;
            }

            int CREDIT_NORMAL_PAY_MAX=0;
            foreach (var item in lndList)
            {
                if(string.IsNullOrEmpty(item.Payment_State))
                {
                    continue;
                }
                var countN = item.Payment_State.ToCharArray().Count(x => x == 'N');
                if (CREDIT_NORMAL_PAY_MAX < countN)
                    CREDIT_NORMAL_PAY_MAX = countN;
            }
            //信用卡24个月最大正常还款次数
            statlnd.CREDIT_NORMAL_PAY_MAX = CREDIT_NORMAL_PAY_MAX;
            #endregion
        }
        private  int GetSpecialValue(bool isLoan)
        {
            if (lnList.Count == 0 && lndList.Count == 0 && stncardList.Count == 0 && recordInfoList.Count == 0)
                return CommonData._9999999;
            else if (isLoan && lnList.Count == 0)
                return CommonData._8888888;
            else if (!isLoan && lndList.Count== 0)
                return CommonData._8888888;
            return 0;
        }

        public CRD_CD_LNDEntity DeepCopy(CRD_CD_LNDEntity obj)
        {
            return new CRD_CD_LNDEntity()
            {
                Used_Credit_Limit_Amount = obj.Used_Credit_Limit_Amount,
                Credit_Limit_Amount = obj.Credit_Limit_Amount,
                Open_Date =obj.Open_Date
            };

        }
        private decimal factorial(decimal m, int n)
        {
            decimal ret = 1;
            for (int i = 0; i < n; i++)
            {
                ret = ret * m;
            }
            return ret;
        }
    }
}
