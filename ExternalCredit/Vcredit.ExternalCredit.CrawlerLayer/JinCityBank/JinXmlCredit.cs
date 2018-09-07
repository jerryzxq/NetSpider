using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
namespace Vcredit.ExternalCredit.CrawlerLayer.JinCityBank
{
    public class JinXmlCredit
    {
        const string rootPath = @"SingleQueryResultMessage0005/ReportMessage/";
        XmlHelper xml = null;
        CreditContainer jincityContianer=null;
        public JinXmlCredit()
        {
            xml = new XmlHelper(ConfigData.jinCityCreditXmlPath);
            jincityContianer = new CreditContainer();
            ///jincityContianer.Report.SourceType = (byte)SysEnums.SourceType.JinCity;//设置征信类型
        }
        public void GetJinXmlCreditInfo()
        {

            //头信息
            AnalysisHeaderInfo();
            //个人信息
            AnalysisPersonalInfo();
            //信息汇总
            AnalysisInfoSummary();
            //信用卡信息
            AnalysisCreditDetail();
            //公共信息
            AnalysisPublicInfo();
            //查询记录
            AnalysisQueryRecord();
            new JinCityComBus().SaveJinCityCreditInfoToDB(jincityContianer);
        }
        #region  Header （头信息）
    
        private void AnalysisHeaderInfo()
        {
            var node= xml.SetAndReturnCurrentXmlNode(rootPath+"Header");
            var messageHeaderchildnodes= node.FirstChild.ChildNodes;
            jincityContianer.Report.Report_Sn = messageHeaderchildnodes[0].InnerText;
            jincityContianer.Report.Query_Time =messageHeaderchildnodes[1].InnerText.ToNotNullDateTime();
            jincityContianer.Report.Report_Create_Time =messageHeaderchildnodes[2].InnerText.ToNotNullDateTime();
            var messageHeaderQueryReq = node.LastChild.ChildNodes;
            jincityContianer.Report.Name = messageHeaderQueryReq[0].InnerText;
            jincityContianer.Report.Cert_Type = messageHeaderQueryReq[1].InnerText;
            jincityContianer.Report.Cert_No = messageHeaderQueryReq[2].InnerText;
            jincityContianer.Report.Query_Reason = messageHeaderQueryReq[3].InnerText;
            var organdcode= messageHeaderQueryReq[4].InnerText.Split('/');
            jincityContianer.Report.Query_Org = organdcode[0];
            jincityContianer.Report.User_Code = organdcode[1];
           
        }
        #endregion

        #region PersonalInfo（个人信息）
        private void AnalysisPersonalInfo()
        {
            xml.SetAndReturnCurrentXmlNode(rootPath + "PersonalInfo");
            //获取身份信息
            GetIdentityInfo();
            //获取居住信息
            GetResidenceInfo();
            //获取职业信息
            GetProfessionInfo();    
        }
        private void GetIdentityInfo()
        {
           var identityNodes= xml.GetCurrentXmlNodeNode("Identity").ChildNodes;
           jincityContianer.Identity.Gender = identityNodes[0].InnerText;
           jincityContianer.Identity.Birthday = identityNodes[1].InnerText.ToNotNullDateTime();
           jincityContianer.Identity.Marital_State=identityNodes[2].InnerText;
           jincityContianer.Identity.Mobile =identityNodes[3].InnerText;
           jincityContianer.Identity.Office_Telephone_No = identityNodes[4].InnerText;
           jincityContianer.Identity.Home_Telephone_No = identityNodes[5].InnerText;
           jincityContianer.Identity.Edu_Level = identityNodes[6].InnerText;
           jincityContianer.Identity.Edu_Degree = identityNodes[7].InnerText;
           jincityContianer.Identity.Post_Address= identityNodes[8].InnerText;
           jincityContianer.Identity.Registered_Address = identityNodes[9].InnerText;

           var SpouseNodes = xml.GetCurrentXmlNodeNode("Spouse").ChildNodes;
           if (SpouseNodes == null)
               return;
           jincityContianer.Identity.Mate_Name = SpouseNodes[0].InnerText;
           jincityContianer.Identity.Mate_Cert_Type = SpouseNodes[1].InnerText;
           jincityContianer.Identity.Mate_Cert_No = SpouseNodes[2].InnerText;
           jincityContianer.Identity.Mate_Employer = SpouseNodes[3].InnerText;
           jincityContianer.Identity.Mate_Telephone_No = SpouseNodes[4].InnerText;
        }
        private void GetResidenceInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_RESIDENCEEntity residence = new CRD_PI_RESIDENCEEntity()
                {
                    Address = param.ChildNodes[0].InnerText,
                    Residence_Type = param.ChildNodes[1].InnerText,
                    Get_Time = DateTime.Parse(param.ChildNodes[2].InnerText)
                };
                jincityContianer.ResidenceList.Add(residence);
            };
            AddCurrentXmlNodesInfo("Residence", aciton);
        }
        private void GetProfessionInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_PROFESSNLEntity profession = new CRD_PI_PROFESSNLEntity()
                {
                    Employer = param.ChildNodes[0].InnerText,
                    Employer_Address = param.ChildNodes[1].InnerText,
                    Occupation = param.ChildNodes[2].InnerText,
                    Industry = param.ChildNodes[3].InnerText,
                    Duty = param.ChildNodes[4].InnerText,
                    Title_Dw = param.ChildNodes[5].InnerText,
                    Start_Year = param.ChildNodes[6].InnerText,
                    Get_Time = DateTime.Parse(param.ChildNodes[7].InnerText)
                };
                jincityContianer.ProfessionList.Add(profession);
            };
            AddCurrentXmlNodesInfo("Professional", aciton);
        }
        #endregion

        #region InfoSummary（信息汇总）
        XmlNodeList infoSummaryNode;
        private void AnalysisInfoSummary()
        {
            infoSummaryNode = xml.SetAndReturnCurrentXmlNode(rootPath + "InfoSummary").FirstChild.ChildNodes;
            //获取为销户贷记卡信息
            GetUndestoryLoancard();
            //获取未销户准贷记卡信息
            GetUndestoryStandardLoancard();
            //获取未结清贷记卡信息
            GetUnpaidLoan();
        }

        private void GetUndestoryLoancard()
        {
            var xmlNodeList = infoSummaryNode[0].ChildNodes;
            jincityContianer.NoCancellLND.FinanceMan_OrgNum =xmlNodeList[0].InnerText.ToNotNullInt();
            jincityContianer.NoCancellLND.Finance_OrgNum = xmlNodeList[1].InnerText.ToNotNullInt();
            jincityContianer.NoCancellLND.AccountNum = xmlNodeList[2].InnerText.ToNotNullInt();
            jincityContianer.NoCancellLND.TotalCreditAmount =xmlNodeList[3].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellLND.MaxCreditAmount = xmlNodeList[4].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellLND.MinimumCreditAmount = xmlNodeList[5].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellLND.UsedAmount =xmlNodeList[6].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellLND.AverageRecent6Months = xmlNodeList[7].InnerText.ToNotNullDecimal();
        }
        private void GetUndestoryStandardLoancard()
        {
            var xmlNodeList = infoSummaryNode[1].ChildNodes;
            jincityContianer.NoCancellSTNCARD.FinanceMan_OrgNum = xmlNodeList[0].InnerText.ToNotNullInt();
            jincityContianer.NoCancellSTNCARD.Finance_OrgNum = xmlNodeList[1].InnerText.ToNotNullInt();
            jincityContianer.NoCancellSTNCARD.AccountNum = xmlNodeList[2].InnerText.ToNotNullInt();
            jincityContianer.NoCancellSTNCARD.TotalCreditAmount = xmlNodeList[3].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellSTNCARD.MaxCreditAmount = xmlNodeList[4].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellSTNCARD.MinimumCreditAmount =xmlNodeList[5].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellSTNCARD.OverDueNum =xmlNodeList[6].InnerText.ToNotNullDecimal();
            jincityContianer.NoCancellSTNCARD.AverageRecent6Months =xmlNodeList[7].InnerText.ToNotNullDecimal();
        }
        private void GetUnpaidLoan()
        {
            var xmlNodeList = infoSummaryNode[2].ChildNodes;
            jincityContianer.OutStandeSummary.LoanManOrgNum = xmlNodeList[0].InnerText.ToNotNullInt();
            jincityContianer.OutStandeSummary.LoanOrgNum =xmlNodeList[1].InnerText.ToNotNullInt();
            jincityContianer.OutStandeSummary.LoanNum =xmlNodeList[2].InnerText.ToNotNullInt();
            jincityContianer.OutStandeSummary.ContractAmount =xmlNodeList[3].InnerText.ToNotNullDecimal();
            jincityContianer.OutStandeSummary.Balance =xmlNodeList[4].InnerText.ToNotNullDecimal();
            jincityContianer.OutStandeSummary.RecentSixMonthRepayment=xmlNodeList[5].InnerText.ToNotNullDecimal();
        }
        #endregion

        #region CreditDetail（信用卡信息）
        private void AnalysisCreditDetail()
        {
            xml.SetAndReturnCurrentXmlNode(rootPath + "CreditDetail");
            //获取贷款信息
            GetLoad();
            //获取贷记卡信息
            GetLoadcard();
            //获取准贷记卡信息
            GetStandardLoancard();
            //获取对外担保信息
            GetExternalGuaranteeLoan();

        }
        XmlNode currentCreditNode = null; // 当前操作的信用节点（贷款，贷记卡，准贷记卡）
        #region 获取贷款信息

        CRD_CD_LNEntity currentLn = null;
        private void GetLoad()
        {
            Action<XmlNode> aciton = (param) =>
            {
                currentCreditNode = param;
                currentLn = new CRD_CD_LNEntity();
                GetCueContractInfoAndState();
                GetCurrAccountInfo();
                GetCurrOverdue();
                GetLatest24MonthPaymentState();
                GetLatest5YearOverdueRecord();
                GetSpecialTrade();
                jincityContianer.LnList.Add(currentLn);
            };
            AddCurrentXmlNodesInfo("Loan", aciton);
        }
        /// <summary>
        /// 获取cue ，contractInfo，state
        /// </summary>
        private void GetCueContractInfoAndState()
        {
            var nodeList = currentCreditNode.ChildNodes;
            currentLn.Cue = nodeList[0].InnerText;
            currentLn.Finance_Org = nodeList[1].ChildNodes[0].InnerText;
            currentLn.FinanceType = nodeList[1].ChildNodes[1].InnerText;
            currentLn.Account_Dw = nodeList[1].ChildNodes[2].InnerText;
            currentLn.Type_Dw = nodeList[1].ChildNodes[3].InnerText;
            currentLn.Currency = nodeList[1].ChildNodes[4].InnerText;
            currentLn.Open_Date =nodeList[1].ChildNodes[5].InnerText.ToNotNullDateTime();
            currentLn.End_Date = nodeList[1].ChildNodes[6].InnerText.ToNotNullDateTime();
            currentLn.Credit_Limit_Amount = nodeList[1].ChildNodes[7].InnerText.ToNotNullDecimal();
            currentLn.Guarantee_Type = nodeList[1].ChildNodes[8].InnerText;
            currentLn.Payment_Rating = nodeList[1].ChildNodes[9].InnerText;
            currentLn.Payment_Cyc = nodeList[1].ChildNodes[10].InnerText;
            currentLn.State_End_Date =nodeList[1].ChildNodes[11].InnerText.ToNotNullDateTime();
            currentLn.State = nodeList[2].InnerText;
        }
        private void GetCurrAccountInfo()
        {
            var currAccountInfo = xml.GetNode(currentCreditNode, "CurrAccountInfo");
            if (currAccountInfo == null)
                return;
            currentLn.Class5_State = xml.GetNodeValue(currAccountInfo, "Class5State");
            currentLn.Balance = GetNodeDecimalVal(currAccountInfo, "Balance");
            currentLn.Remain_Payment_Cyc = GetNodeDecimalVal(currAccountInfo, "RemainPaymentCyc");
            currentLn.Scheduled_Payment_Amount = GetNodeDecimalVal(currAccountInfo, "ScheduledPaymentAmount");
            currentLn.Scheduled_Payment_Date = GetNodeDateTimeVal(currAccountInfo, "ScheduledPaymentDate");
            currentLn.Actual_Payment_Amount = GetNodeDecimalVal(currAccountInfo, "ActualPaymentAmount");
            currentLn.Recent_Pay_Date = GetNodeDateTimeVal(currAccountInfo, "RecentPayDate");

        }
        private void  GetCurrOverdue()
        {
            var currOverdue = xml.GetNode(currentCreditNode, "CurrOverdue");
            if (currOverdue == null)
                return;
            currentLn.Curr_Overdue_Cyc = GetNodeDecimalVal(currOverdue,"CurrOverdueCyc");
            currentLn.Curr_Overdue_Amount = GetNodeDecimalVal(currOverdue,"CurrOverdueAmount");
            currentLn.Overdue31_To60_Amount = GetNodeDecimalVal(currOverdue,"Overdue31To60Amount");
            currentLn.Overdue61_To90_Amount = GetNodeDecimalVal(currOverdue,"Overdue61To90Amount");
            currentLn.Overdue91_To180_Amount = GetNodeDecimalVal(currOverdue,"Overdue91To180Amount");
            currentLn.Overdue_Over180_Amount = GetNodeDecimalVal(currOverdue,"OverdueOver180Amount");

        }  
        private void GetLatest24MonthPaymentState()
        {
            var latest24MonthPaymentState = xml.GetNode(currentCreditNode, "Latest24MonthPaymentState");
            if (latest24MonthPaymentState == null)
                return;
            currentLn.Payment_State_Begin_Month = xml.GetNodeValue(latest24MonthPaymentState, "BeginMonth");
            currentLn.Payment_State_End_Month = xml.GetNodeValue(latest24MonthPaymentState, "EndMonth");
            currentLn.Payment_State = xml.GetNodeValue(latest24MonthPaymentState, "Latest24State");
        }
        private void  GetLatest5YearOverdueRecord()
        {
            var latest5YearOverdueRecord = xml.GetNode(currentCreditNode, "Latest5YearOverdueRecord");
            if (latest5YearOverdueRecord == null)
                return;

            var childNodes = latest5YearOverdueRecord.ChildNodes;
            currentLn.Overdue_Record_Begin_Month =childNodes[0].InnerText;
            currentLn.Overdue_Record_End_Month=childNodes[1].InnerText;
            for(int i=2;i<childNodes.Count;i++)
            {
                CRD_CD_LN_OVDEntity overdue = new CRD_CD_LN_OVDEntity()
                    {
                        Month_Dw=childNodes[i].ChildNodes[0].InnerText,
                        Last_Months=childNodes[i].ChildNodes[1].InnerText.ToNotNullDecimal(),
                        Amount= childNodes[i].ChildNodes[2].InnerText.ToNotNullDecimal()
                    };
                currentLn.LnoverList.Add(overdue);
            }
        }
        
        private void GetSpecialTrade()
        {
            var specialTradeNodes = xml.GetNodeList(currentCreditNode, "SpecialTrade");
            if (specialTradeNodes == null||specialTradeNodes.Count==0)
                return;
            foreach (XmlNode item in specialTradeNodes)
            {
                CRD_CD_LN_SPLEntity lnspl = new CRD_CD_LN_SPLEntity()
                    {
                        Type_Dw=xml.GetNodeValue(item,"Type"),
                        Get_Time=GetNodeDateTimeVal(item,"GetTime"),
                        Changing_Months=GetNodeDecimalVal(item,"ChangingMonths"),
                        Changing_Amount=GetNodeDecimalVal(item,"ChangingAmount"),
                        Content=xml.GetNodeValue(item,"Content")

                    };
                currentLn.LnSPLList.Add(lnspl);
            }
        }

        #endregion

        #region 获取贷记卡信息
        CRD_CD_LNDEntity currentLnd = null;
        private void GetLoadcard()
        {
            Action<XmlNode> aciton = (param) =>
            {
                currentCreditNode = param;
                currentLnd = new CRD_CD_LNDEntity();
                GetCueAwardCreditInfoAndState();
                GetRepayInfo();
                GetLoadcardCurrOverdue();
                GetLoadcardLatest24MonthPaymentState();
                GetLoadcardLatest5YearOverdueRecord();
                GetLoadcardSpecialTrade();
                jincityContianer.LndiLst.Add(currentLnd);
            };
            AddCurrentXmlNodesInfo("Loancard", aciton);

         
        }
        /// <summary>
        /// 获取cue ，AwardCreditInfo，state
        /// </summary>
        private void GetCueAwardCreditInfoAndState()
        {
            currentLnd.Cue = xml.GetNodeValue(currentCreditNode, "Cue");
            currentLnd.Finance_Org = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/FinanceOrg");
            currentLnd.FinanceType = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/FinanceType");
            currentLnd.Account_Dw = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/Account");
            currentLnd.Currency = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/Currency");
            currentLnd.Open_Date = GetNodeDateTimeVal(currentCreditNode, "AwardCreditInfo/OpenDate");
            currentLnd.Credit_Limit_Amount = GetNodeDecimalVal(currentCreditNode, "AwardCreditInfo/CreditLimitAmount");
            currentLnd.Guarantee_Type = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/GuaranteeType");
            currentLnd.State_End_Date = GetNodeDateTimeVal(currentCreditNode, "AwardCreditInfo/StateEndDate");
            currentLnd.State = xml.GetNodeValue(currentCreditNode, "State");
            currentLnd.Share_Credit_Limit_Amount = GetNodeDecimalVal(currentCreditNode, "AwardCreditInfo/ShareCreditLimitAmount");
        }
        private void GetRepayInfo()
        {
            var RepayInfo = xml.GetNode(currentCreditNode, "RepayInfo");
            if (RepayInfo == null)
                return;
            currentLnd.Used_Credit_Limit_Amount =GetNodeDecimalVal(RepayInfo, "UsedCreditLimitAmount");
            currentLnd.Latest6_Month_Used_Avg_Amount = GetNodeDecimalVal(RepayInfo, "Latest6MonthUsedAvgAmount");
            currentLnd.Used_Highest_Amount = GetNodeDecimalVal(RepayInfo, "UsedHighestAmount");
            currentLnd.Scheduled_Payment_Amount = GetNodeDecimalVal(RepayInfo, "ScheduledPaymentAmount");
            currentLnd.Scheduled_Payment_Date = GetNodeDateTimeVal(RepayInfo, "ScheduledPaymentDate");
            currentLnd.Actual_Payment_Amount = GetNodeDecimalVal(RepayInfo, "ActualPaymentAmount");
            currentLnd.Recent_Pay_Date = GetNodeDateTimeVal(RepayInfo, "RecentPayDate");

        }
        private void GetLoadcardCurrOverdue()
        {
            var currOverdue = xml.GetNode(currentCreditNode, "CurrOverdue");
            if (currOverdue == null)
                return;
            currentLnd.Curr_Overdue_Cyc = GetNodeDecimalVal(currOverdue, "CurrOverdueCyc");
            currentLnd.Curr_Overdue_Amount = GetNodeDecimalVal(currOverdue, "CurrOverdueAmount");
            if (currOverdue.ChildNodes.Count > 2)
            {
                currentLnd.Overdue31_To60_Amount = GetNodeDecimalVal(currOverdue, "Overdue31To60Amount");
                currentLnd.Overdue61_To90_Amount = GetNodeDecimalVal(currOverdue, "Overdue61To90Amount");
                currentLnd.Overdue91_To180_Amount = GetNodeDecimalVal(currOverdue, "Overdue91To180Amount");
                currentLnd.Overdue_Over180_Amount = GetNodeDecimalVal(currOverdue, "OverdueOver180Amount");
            }
        }
        private void GetLoadcardLatest24MonthPaymentState()
        {
            var latest24MonthPaymentState = xml.GetNode(currentCreditNode, "Latest24MonthPaymentState");
            if (latest24MonthPaymentState == null)
                return;
            currentLnd.Payment_State_Begin_Month = xml.GetNodeValue(latest24MonthPaymentState, "BeginMonth");
            currentLnd.Payment_State_End_Month = xml.GetNodeValue(latest24MonthPaymentState, "EndMonth");
            currentLnd.Payment_State = xml.GetNodeValue(latest24MonthPaymentState, "Latest24State");
            currentLnd.Overdue_Cyc = GetOverdue_Cyc(currentLnd.Payment_State);
        }
      
        private void GetLoadcardLatest5YearOverdueRecord()
        {
            var latest5YearOverdueRecord = xml.GetNode(currentCreditNode, "Latest5YearOverdueRecord");
            if (latest5YearOverdueRecord == null)
                return;

            var childNodes = latest5YearOverdueRecord.ChildNodes;
            currentLnd.Overdue_Record_Begin_Month = childNodes[0].InnerText;
            currentLnd.Overdue_Record_End_Month = childNodes[1].InnerText;
            for (int i = 2; i < childNodes.Count; i++)
            {
                CRD_CD_LND_OVDEntity overdue = new CRD_CD_LND_OVDEntity()
                {
                    Month_Dw = childNodes[i].ChildNodes[0].InnerText,
                    Last_Months =childNodes[i].ChildNodes[1].InnerText.ToNotNullDecimal(),
                    Amount =childNodes[i].ChildNodes[2].InnerText.ToNotNullDecimal()
                };
                currentLnd.LndoverList.Add(overdue);
            }
        }
        private void GetLoadcardSpecialTrade()
        {
            var specialTradeNodes = xml.GetNodeList(currentCreditNode, "SpecialTrade");
            if (specialTradeNodes == null||specialTradeNodes.Count==0)
                return;
            foreach (XmlNode item in specialTradeNodes)
            {
                CRD_CD_LND_SPLEntity lnspl = new CRD_CD_LND_SPLEntity()
                {
                    Type_Dw = xml.GetNodeValue(item, "Type"),
                    Get_Time = GetNodeDateTimeVal(item, "GetTime"),
                    Changing_Months = GetNodeDecimalVal(item, "ChangingMonths"),
                    Changing_Amount = GetNodeDecimalVal(item, "ChangingAmount"),
                    Content = xml.GetNodeValue(item, "Content")

                };
                currentLnd.LndSPLList.Add(lnspl);
            }
        }
        #endregion

        #region 获取准贷记卡信息

        CRD_CD_STNCARDEntity currentStn = null;
        private void GetStandardLoancard()
        {
            Action<XmlNode> aciton = (param) =>
            {
                currentCreditNode = param;
                currentStn = new CRD_CD_STNCARDEntity();
                GetStnCueAwardCreditInfoAndState();
                GetStnRepayInfo();
                GetStnCurrOverdue();
                GetStnLatest24MonthPaymentState();
                GetStnLatest5YearOverdueRecord();
                GetStnSpecialTrade();
                jincityContianer.StnCardList.Add(currentStn);
            };
            AddCurrentXmlNodesInfo("StandardLoancard", aciton);
        }
        /// <summary>
        /// 获取cue ，AwardCreditInfo，state
        /// </summary>
        private void GetStnCueAwardCreditInfoAndState()
        {
            currentStn.Cue = xml.GetNodeValue(currentCreditNode, "Cue");
            currentStn.Finance_Org = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/FinanceOrg");
            currentStn.FinanceType = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/FinanceType");
            currentStn.Account_Dw = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/Account");
            currentStn.Currency = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/Currency");
            currentStn.Open_Date = GetNodeDateTimeVal(currentCreditNode, "AwardCreditInfo/OpenDate");
            currentStn.Credit_Limit_Amount = GetNodeDecimalVal(currentCreditNode, "AwardCreditInfo/CreditLimitAmount");
            currentStn.Guarantee_Type = xml.GetNodeValue(currentCreditNode, "AwardCreditInfo/GuaranteeType");
            currentStn.State_End_Date = GetNodeDateTimeVal(currentCreditNode, "AwardCreditInfo/StateEndDate");
            currentStn.State = xml.GetNodeValue(currentCreditNode, "State");
            currentStn.Share_Credit_Limit_Amount = GetNodeDecimalVal(currentCreditNode, "AwardCreditInfo/ShareCreditLimitAmount");
        }
        private void GetStnRepayInfo()
        {
            var RepayInfo = xml.GetNode(currentCreditNode, "RepayInfo");
            if (RepayInfo == null)
                return;
            currentStn.Used_Credit_Limit_Amount = GetNodeDecimalVal(RepayInfo, "UsedCreditLimitAmount");
            currentStn.Latest6_Month_Used_Avg_Amount = GetNodeDecimalVal(RepayInfo, "Latest6MonthUsedAvgAmount");
            currentStn.Used_Highest_Amount = GetNodeDecimalVal(RepayInfo, "UsedHighestAmount");
            currentStn.Scheduled_Payment_Amount = GetNodeDecimalVal(RepayInfo, "ScheduledPaymentAmount");
            currentStn.Scheduled_Payment_Date = GetNodeDateTimeVal(RepayInfo, "ScheduledPaymentDate");
            currentStn.Actual_Payment_Amount = GetNodeDecimalVal(RepayInfo, "ActualPaymentAmount");
            currentStn.Recent_Pay_Date = GetNodeDateTimeVal(RepayInfo, "RecentPayDate");

        }
        private void GetStnCurrOverdue()
        {
            var currOverdue = xml.GetNode(currentCreditNode, "CurrOverdue");
            if (currOverdue == null)
                return;
     
            currentStn.OVERDUE_OVER180_AMOUNT = GetNodeDecimalVal(currOverdue, "OverdueOver180Amount");
            if (currOverdue.ChildNodes.Count >1)
            {
                currentStn.Curr_Overdue_Cyc = GetNodeDecimalVal(currOverdue, "CurrOverdueCyc");
                currentStn.Curr_Overdue_Amount = GetNodeDecimalVal(currOverdue, "CurrOverdueAmount");
                currentStn.Overdue31_To60_Amount = GetNodeDecimalVal(currOverdue, "Overdue31To60Amount");
                currentStn.Overdue61_To90_Amount = GetNodeDecimalVal(currOverdue, "Overdue61To90Amount");
                currentStn.Overdue91_To180_Amount = GetNodeDecimalVal(currOverdue, "Overdue91To180Amount");
            }
        }
        private void GetStnLatest24MonthPaymentState()
        {
            var latest24MonthPaymentState = xml.GetNode(currentCreditNode, "Latest24MonthPaymentState");
            if (latest24MonthPaymentState == null)
                return;
            currentStn.Payment_State_Begin_Month = xml.GetNodeValue(latest24MonthPaymentState, "BeginMonth");
            currentStn.Payment_State_End_Month = xml.GetNodeValue(latest24MonthPaymentState, "EndMonth");
            currentStn.Payment_State = xml.GetNodeValue(latest24MonthPaymentState, "Latest24State");
        }
        private void GetStnLatest5YearOverdueRecord()
        {
            var latest5YearOverdueRecord = xml.GetNode(currentCreditNode, "Latest5YearOverdueRecord");
            if (latest5YearOverdueRecord == null)
                return;

            var childNodes = latest5YearOverdueRecord.ChildNodes;
            currentStn.Overdue_Record_Begin_Month = childNodes[0].InnerText;
            currentStn.Overdue_Record_End_Month = childNodes[1].InnerText;
            for (int i = 2; i < childNodes.Count; i++)
            {
                CRD_CD_STN_OVDEntity overdue = new CRD_CD_STN_OVDEntity()
                {
                    Month_Dw = childNodes[i].ChildNodes[0].InnerText,
                    Last_Months = childNodes[i].ChildNodes[1].InnerText.ToNotNullDecimal(),
                    Amount = childNodes[i].ChildNodes[2].InnerText.ToNotNullDecimal()
                };
                currentStn.StnoverList.Add(overdue);
            }
        }
        private void GetStnSpecialTrade()
        {
            var specialTradeNodes = xml.GetNodeList(currentCreditNode, "SpecialTrade");
            if (specialTradeNodes == null)
                return;
            foreach (XmlNode item in specialTradeNodes)
            {
                CRD_CD_STN_SPLEntity lnStn = new CRD_CD_STN_SPLEntity()
                {
                    Type_Dw = xml.GetNodeValue(item, "Type"),
                    Get_Time = GetNodeDateTimeVal(item, "GetTime"),
                    Changing_Months = GetNodeDecimalVal(item, "ChangingMonths"),
                    Changing_Amount = GetNodeDecimalVal(item, "ChangingAmount"),
                    Content = xml.GetNodeValue(item, "Content")

                };
                currentStn.StnSPLList.Add(lnStn);
            }
        }
        #endregion

        /// <summary>
        /// 获取对外担保信息
        /// </summary>
        private void GetExternalGuaranteeLoan()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_GUARANTEEEntity guarantee = new CRD_CD_GUARANTEEEntity()
                {
                    Organ_Name = xml.GetNodeValue(param, "GuaranteeOrg"),
                    Contract_Money = GetNodeDecimalVal(param, "Account"),
                    Begin_Date = GetNodeDateTimeVal(param, "OpenDate"),
                    End_Date = GetNodeDateTimeVal(param, "EndDate"),
                    Guarantee_Money = GetNodeDecimalVal(param, "GuaranteeAmt"),
                    Guarantee_Balance = GetNodeDecimalVal(param, "Balance"),
                    Class5_State = xml.GetNodeValue(param, "Class5State"),
                    Billing_Date = GetNodeDateTimeVal(param, "ScheduledPaymentDate")
                };
                jincityContianer.GuaranteeList.Add(guarantee);
            };
            AddCurrentXmlNodesInfo("ExternalGuaranteeLoan", aciton);
        }

        #endregion

        #region PublicInfo（公共信息）
        private void AnalysisPublicInfo()
        {
            xml.SetAndReturnCurrentXmlNode(rootPath + "PublicInfo");
            //获取公积金缴存记录
            GetAccFund();
            //获取养老缴存记录
            GetEndowmentInsuranceDeposit();
            //获取养老金发放记录
            GetEndowmentInsuranceDeliver();
            //获取低保救济记录
            GetSalvation();
        }

        private void GetAccFund()
        {
            Action<XmlNode> aciton = (param) =>
                {
                    CRD_PI_ACCFUNDEntity accfund = new CRD_PI_ACCFUNDEntity()
                    {
                        Area = xml.GetNodeValue(param, "Area"),
                        Register_Date = xml.GetNodeValue(param, "RegisterDate"),
                        First_Month = xml.GetNodeValue(param, "FirstMonth"),
                        To_Month = xml.GetNodeValue(param, "ToMonth"),
                        State = xml.GetNodeValue(param, "State"),
                        Pay = GetNodeDecimalVal(param, "Pay"),
                        Own_Percent = xml.GetNodeValue(param, "OwnPercent"),
                        Com_Percent = xml.GetNodeValue(param, "ComPercent"),
                        Organ_Name = xml.GetNodeValue(param, "Organname"),
                        Get_Time = GetNodeDateTimeVal(param, "GetTime")
                    };
                    jincityContianer.AccfundList.Add(accfund);
                };
            AddCurrentXmlNodesInfo("AccFund", aciton);

        }
        private void GetEndowmentInsuranceDeposit()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_ENDINSDPTEntity deposit = new CRD_PI_ENDINSDPTEntity()
                {
                    Area = xml.GetNodeValue(param, "Area"),
                    Register_Date = xml.GetNodeValue(param, "RegisterDate"),
                    Month_Duration = GetNodeDecimalVal(param, "MonthDuration"),
                    Work_Date = xml.GetNodeValue(param, "WorkDate"),
                    State = xml.GetNodeValue(param, "State"),
                    Own_Basic_Money = GetNodeDecimalVal(param, "OwnBasicMoney"),
                    Money = GetNodeDecimalVal(param, "Money"),
                    Organ_Name = xml.GetNodeValue(param, "Organname"),
                    Pause_Reason = xml.GetNodeValue(param, "PauseReason"),
                    Get_Time = GetNodeDateTimeVal(param, "GetTime")
                };
                jincityContianer.EndInsdptList.Add(deposit);
            };
            AddCurrentXmlNodesInfo("EndowmentInsuranceDeposit", aciton);
        }
        private void GetEndowmentInsuranceDeliver()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_ENDINSDLREntity deliver = new CRD_PI_ENDINSDLREntity()
                {
                    Area = xml.GetNodeValue(param, "Area"),
                    Retire_Type = xml.GetNodeValue(param, "RetireType"),
                    Retired_Date = xml.GetNodeValue(param, "RetiredDate"),
                    Work_Date = xml.GetNodeValue(param, "WorkDate"),
                    Money = GetNodeDecimalVal(param, "Money"),
                    Organ_Name = xml.GetNodeValue(param, "Organname"),
                    Pause_Reason = xml.GetNodeValue(param, "PauseReason"),
                    Get_Time = GetNodeDateTimeVal(param, "GetTime")
                };
                jincityContianer.EndinsdlrList.Add(deliver);
            };
            AddCurrentXmlNodesInfo("EndowmentInsuranceDeliver", aciton);
        }
        private void GetSalvation()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_SALVATIONEntity salvation = new CRD_PI_SALVATIONEntity()
                {
                    Area = xml.GetNodeValue(param, "Area"),
                    Personnel_Type = xml.GetNodeValue(param, "PersonnelType"),
                    Register_Date = GetNodeDateTimeVal(param, "RegisterDate"),
                    Money = GetNodeDecimalVal(param, "Money"),
                    Organ_Name = xml.GetNodeValue(param, "Organname"),
                    Get_Time = GetNodeDateTimeVal(param, "GetTime"),
                    Pass_Date = GetNodeDateTimeVal(param, "PassDate")
                };
                jincityContianer.SalvationList.Add(salvation);
            };
            AddCurrentXmlNodesInfo("Salvation", aciton);
        }
        #endregion

        #region QueryRecord（查询记录）
        private void AnalysisQueryRecord()
        {
            xml.SetAndReturnCurrentXmlNode(rootPath + "QueryRecord");
            Action<XmlNode> aciton = (param) =>
            {
                CRD_QR_RECORDDTLINFOEntity queryRecord = new CRD_QR_RECORDDTLINFOEntity()
                {
                    Query_Date = GetNodeDateTimeVal(param, "QueryDate"),
                    Querier = xml.GetNodeValue(param, "Querier"),
                    Query_Reason = xml.GetNodeValue(param, "QueryReason")
                
                };
                jincityContianer.RecorddtlinfoList.Add(queryRecord);
            };
            AddCurrentXmlNodesInfo("RecordDetail", aciton);

        }
        #endregion

        #region 公共私有方法
        private int  GetNodeIntVal(XmlNode  node,string  xpath)
        {
           return  xml.GetNodeValue(node, xpath).ToNotNullInt();
        }
        private decimal GetNodeDecimalVal(XmlNode node, string xpath)
        {
            return xml.GetNodeValue(node, xpath).ToNotNullDecimal();
        }
        private DateTime GetNodeDateTimeVal(XmlNode node, string xpath)
        {
            return xml.GetNodeValue(node, xpath).ToNotNullDateTime();
        }
        private void AddCurrentXmlNodesInfo(string xpath, Action<XmlNode> action)
        {
            var nodes = xml.GetCurrentXmlNodeNodeList(xpath);
            if (nodes == null||nodes.Count==0)
                return;
            foreach (XmlNode item in nodes)
            {
                action(item);
            }
        }
        private byte GetOverdue_Cyc(string payment_state)
        {
            if (string.IsNullOrEmpty(payment_state))
                return 0;
            return (byte)Regex.Matches(payment_state, "[1-7]").Count;

        }
        #endregion


    }
}
