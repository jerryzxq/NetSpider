using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vcredit.Common.Helper;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.Common.Ext;
using System.Text;

namespace Vcredit.WeiXin.RestService
{
    public partial class RCResultQuery : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["orderid"] != null)
            {
                GetResult(Request.QueryString["orderid"].ToString());
            }
        }

        private void GetResult(string orderid)
        {
            StringBuilder sb = new StringBuilder();
            string basePath = HttpContext.Current.Server.MapPath("~/files/RC/");

            string strParam = FileOperateHelper.ReadFile(basePath + "param_" + orderid + ".txt");
            if (strParam == "不存在相应的目录")
            {
                return;
            }
            List<VBXML> VBList = strParam.DeserializeXML<List<VBXML>>();
            sb.Append("<ul>");

            string colName = string.Empty;
            foreach (VBXML item in VBList)
            {
                switch (item.VB_COL)
                {
                    case "SOCIALSECURITY_AGE": colName = "[卡卡贷]年龄"; break;
                    case "CREDIT_grade": colName = "[网络版征信]评分卡分数"; break;
                    case "SOCIAL_SCORE": colName = "[公积金社保]社保评分卡分数"; break;
                    case "KKD_LOANPERIOD": colName = "[卡卡贷]贷款期数"; break;
                    case "KKD_LOANAMT": colName = "[卡卡贷]贷款申请金额"; break;
                    case "M24_ContinuePay_SumMonth": colName = "[公积金社保]24个月内最近连续缴费月数"; break;
                    case "SOCIAL_BASEPAY": colName = "[公积金社保]缴纳基数"; break;
                    case "NC_M3_ALL_CNT_TOTAL": colName = "[网络版征信]三个月内所有征信查询次数"; break;
                    case "ALL_LOAN_HOUSE_DELAY90_CNT": colName = "[网络版征信]住房贷款发生过90天以上逾期的账户数(all)"; break;
                    case "ALL_LOAN_OTHER_DELAY90_CNT": colName = "[网络版征信]其他贷款发生过90天以上逾期的账户数(all)"; break;
                    case "ALL_CREDIT_DELAY90_CNT": colName = "[网络版征信]信用卡发生过90天以上逾期的账户数(all)"; break;
                    case "credit_dlq_amount": colName = "[网络版征信]信用卡逾期金额"; break;
                    case "loan_dlq_amount": colName = "[网络版征信]贷款逾期金额"; break;
                    case "ALL_LOAN_HOUSE_UNCLOSEACCOUNT_CNT": colName = "[网络版征信]住房贷款未销户账户数(all)"; break;
                    case "ALL_LOAN_OTHER_UNCLOSED_CNT": colName = "[网络版征信]其他贷款未销户账户数(all)"; break;
                    case "ALL_CREDIT_UNCLOSED_CNT": colName = "[网络版征信]信用卡未销户"; break;
                    case "CREDIT_LIMIT_AMOUNT_NORM_MAX": colName = "[网络版征信]正常信用卡最大额度"; break;
                    case "NC_NORMAL_USED_RATE": colName = "[网络版征信]正常信用卡使用率"; break;
                    case "SUM_USED_CREDIT_LIMIT_AMOUNT": colName = "[网络版征信]信用卡已使用总额度"; break;
                    case "NORMAL_CARD_AGE_MONTH": colName = "[网络版征信]正常状态最早卡龄"; break;
                    case "Normal_Used_Credit_Limit_Amount": colName = "[网络版征信]正常信用卡中的最大已使用额度"; break;
                    case "SUM_Loan_Amount": colName = "[网络版征信]所有贷款的贷款本金总额"; break;
                    default: colName = ""; break;
                }
                sb.Append(string.Format("<li><span style='color: blue'>{0}:{1} </span>:<span style='color: red'>{2}</span></li>", colName, item.VB_COL, item.VB_VALUE));
            }
            sb.Append("</ul>");
            this.ltl_params.Text = sb.ToString();

            string strResult = FileOperateHelper.ReadFile(basePath + "result_" + orderid + ".txt");
            if (strResult == "不存在相应的目录")
            {
                return;
            }
            DecisionResult decisionResult = strResult.DeserializeXML<List<DecisionResult>>().FirstOrDefault();
           
            if (decisionResult != null)
            {
                sb.Clear();
                sb.Append("<ul>");
                sb.Append(string.Format("<li><span style='color: blue'>结果：</span>：<span style='color: red'>{0}</span></li>", decisionResult.Result));
                foreach (var item in decisionResult.RuleResultCanShowSets)
                {
                    sb.Append(string.Format("<li><span style='color: blue'>{0}</span>：<span style='color: red'>{1}</span></li>", item.Key, item.Value));
                }
                sb.Append("</ul>");
                this.ltl_result.Text = sb.ToString();
            }
        }
    }
}