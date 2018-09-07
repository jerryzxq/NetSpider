using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.WorkFlow;
using Vcredit.NetSpider.Parser;
using Vcredit.NetSpider.Processor;
using Vcredit.NetSpider.WorkFlow;
using Vcredit.Common.Ext;

namespace Vcredit.NetSpider.RestService.Operation
{
    public class SocialSecurityOpr
    {
        #region 声明变量、接口
        ITaobaoExecutor taobaoExecutor = ExecutorManager.GetTaobaoExecutor();
        IExecutor Executor = ExecutorManager.GetExecutor();
        IRuntimeService runService = ProcessEngine.GetRuntimeService();
        IVerCodeParserService vercodeService = ParserServiceManager.GetVerCodeParserService();
        CookieCollection cookies = new CookieCollection();
       
        #endregion
        /// <summary>
        /// 上海社保采集
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public SocialSecurityQueryRes QueryShangHai(string username, string password)
        {
            SocialSecurityQueryRes QueryRes = new SocialSecurityQueryRes();
            ProcessInstance proc = null;
            SocialSecurityDetailQueryRes detailRes = null;
            try
            {
                proc = runService.CreateProcess("上海社保");
                proc.Variable.Add("username", username);
                proc.Variable.Add("password", password);
                Executor.ExecuteById(proc.id, null, 1);
                if (proc.GatherDataTable.Count > 0)
                {
                    DataTable socialDT = proc.GatherDataTable["social"];
                    DataTable summaryDT = proc.GatherDataTable["summary"];
                    DataTable info1DT = proc.GatherDataTable["info1"];
                    DataTable info2DT = proc.GatherDataTable["info2"];
                    QueryRes.StatusCode = ServiceConstants.StatusCode_success;
                    QueryRes.Name = info1DT.Rows[0]["姓名"].ToString();
                    //QueryRes.IdentityCard = info1DT.Rows[0]["身份证"].ToString();
                    QueryRes.IdentityCard = username;
                    QueryRes.WorkingAge = info1DT.Rows[0]["连续工龄"].ToString();
                    QueryRes.DeadlineYearAndMonth = GetYearAndMonth(info2DT.Rows[0]["截至年月"].ToString());
                    QueryRes.PaymentMonths = (info2DT.Rows[0]["累计缴费月数"].ToString()).ToInt(0);
                    QueryRes.InsuranceTotal = (info2DT.Rows[0]["养老金本息总额"].ToString()).ToDecimal(0);
                    QueryRes.PersonalInsuranceTotal = (info2DT.Rows[0]["养老金总额个人部分"].ToString()).ToDecimal(0);

                    int RowNum = socialDT.Rows.Count;
                    for (int i = 0; i < RowNum; i++)
                    {
                        detailRes = new SocialSecurityDetailQueryRes();
                        detailRes.Name = QueryRes.Name;
                        detailRes.IdentityCard = QueryRes.IdentityCard;

                        detailRes.PayTime = GetYearAndMonth(socialDT.Rows[i]["年月"].ToString());
                        detailRes.SocialInsuranceTime = detailRes.PayTime;
                        detailRes.SocialInsuranceBase = (socialDT.Rows[i]["缴费基数"].ToString()).ToDecimal(0);
                        detailRes.PensionAmount = (socialDT.Rows[i]["养保个人缴费额"].ToString()).ToDecimal(0);
                        detailRes.MedicalAmount = (socialDT.Rows[i]["医保个人缴费额"].ToString()).ToDecimal(0);
                        detailRes.UnemployAmount = (socialDT.Rows[i]["失保个人缴费额"].ToString()).ToDecimal(0);

                        foreach (DataRow dr in summaryDT.Rows)
                        {
                            if (detailRes.PayTime == GetYearAndMonth(dr["年份"].ToString()))
                            {
                                detailRes.YearPaymentMonths = (dr["社保缴费年度内实际缴费月数"].ToString()).ToInt(0);
                                detailRes.PersonalTotalAmount = (dr["个人部分"].ToString()).ToDecimal(0);
                                detailRes.SocialInsuranceTotalAmount = (dr["合计部分"].ToString()).ToDecimal(0);
                            }
                        }
                        QueryRes.Details.Add(detailRes);
                    }
                }
                else
                {
                    QueryRes.StatusCode = ServiceConstants.StatusCode_fail;
                    QueryRes.StatusDescription = "无用户数据或者用户验证失败";
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return QueryRes;
        }

        /// <summary>
        /// 青岛社保采集
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public SocialSecurityQueryRes QueryQingDao(string username, string password)
        {
            SocialSecurityQueryRes QueryRes = new SocialSecurityQueryRes();
            ProcessInstance proc = null;
            SocialSecurityDetailQueryRes detailRes = null;
            try
            {
                proc = runService.CreateProcess("青岛社保");
                proc.Variable.Add("username", username);
                proc.Variable.Add("password", password);
                bool isExec = QingDaoForEachQuery(proc, 3);
                if (isExec)
                {
                    if (proc.GatherData.Count > 0 && proc.GatherDataTable.Count > 0)
                    {
                        QueryRes.StatusCode = ServiceConstants.StatusCode_success;
                        List<string> mainInfo = proc.GatherData["main"];
                        QueryRes.CompanyNo = mainInfo[0];
                        QueryRes.CompanyType = mainInfo[1];
                        QueryRes.CompanyStatus = mainInfo[2];
                        QueryRes.District = mainInfo[3];
                        QueryRes.CompanyName = mainInfo[4];
                        QueryRes.EmployeeNo = mainInfo[5];
                        QueryRes.Name = mainInfo[6];
                        QueryRes.IdentityCard = mainInfo[7];
                        QueryRes.Sex = mainInfo[8];
                        QueryRes.WorkDate = mainInfo[9];
                        QueryRes.BirthDate = mainInfo[10];
                        QueryRes.EmployeeStatus = mainInfo[11];
                        QueryRes.Race = mainInfo[12];
                        QueryRes.IsSpecialWork = mainInfo[13] == "否" ? false : true;
                        QueryRes.RetireType = mainInfo[14];
                        QueryRes.PensionLevel = mainInfo[15];
                        QueryRes.HealthType = mainInfo[16];
                        QueryRes.SpecialPaymentType = mainInfo[17];
                        QueryRes.Bank = mainInfo[18];
                        QueryRes.BankAddress = mainInfo[19];
                        if (!String.IsNullOrEmpty(mainInfo[20]))
                        {
                            QueryRes.PaymentMonths = mainInfo[20].ToInt(0);
                        }
                        if (!String.IsNullOrEmpty(mainInfo[21]))
                        {
                            QueryRes.OldPaymentMonths = mainInfo[21].ToInt(0);
                        }
                        QueryRes.Phone = mainInfo[22];
                        QueryRes.ZipCode = mainInfo[23];
                        QueryRes.Address = mainInfo[24];


                        DataTable PensionDT = proc.GatherDataTable["Pension"];
                        DataTable MedicalDT = proc.GatherDataTable["Medical"];
                        DataTable UnemployDT = proc.GatherDataTable["Unemploy"];
                        int RowNum = PensionDT.Rows.Count;
                        for (int i = 0; i < RowNum; i++)
                        {
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = QueryRes.Name;
                            detailRes.IdentityCard = QueryRes.IdentityCard;

                            detailRes.PayTime = PensionDT.Rows[i]["缴费年月"].ToString();
                            detailRes.SocialInsuranceTime = PensionDT.Rows[i]["应属年月"].ToString();
                            detailRes.PaymentType = PensionDT.Rows[i]["缴费类别"].ToString();
                            detailRes.SocialInsuranceBase = (PensionDT.Rows[i]["个人基数"].ToString()).ToDecimal(0);
                            detailRes.PaymentFlag = PensionDT.Rows[i]["缴费标志"].ToString();

                            //养老
                            detailRes.PensionAmount = (PensionDT.Rows[i]["个人缴费"].ToString()).ToDecimal(0);
                            detailRes.CompanyPensionAmount = (PensionDT.Rows[i]["单位划入帐户"].ToString()).ToDecimal(0);
                            detailRes.NationPensionAmount = (PensionDT.Rows[i]["社平划入"].ToString()).ToDecimal(0);

                            //医疗
                            detailRes.MedicalAmount = (MedicalDT.Rows[i]["个人缴费"].ToString()).ToDecimal(0);
                            detailRes.CompanyMedicalAmount = (MedicalDT.Rows[i]["单位划入帐户"].ToString()).ToDecimal(0);
                            detailRes.EnterAccountMedicalAmount = (MedicalDT.Rows[i]["共划入账户"].ToString()).ToDecimal(0);
                            detailRes.IllnessMedicalAmount = (MedicalDT.Rows[i]["大额救助"].ToString()).ToDecimal(0);
                            //失业
                            detailRes.UnemployAmount = (UnemployDT.Rows[i]["个人缴费"].ToString()).ToDecimal(0);

                            QueryRes.Details.Add(detailRes);
                        }
                    }
                    else
                    {
                        QueryRes.StatusCode = ServiceConstants.StatusCode_fail;
                        QueryRes.StatusDescription = "无用户数据或者用户验证失败";
                    }
                }
                else
                {
                    QueryRes.StatusCode = ServiceConstants.StatusCode_fail;
                    QueryRes.StatusDescription = "用户验证失败";
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return QueryRes;
        }
        private bool QingDaoForEachQuery(ProcessInstance proc, int count)
        {
            int cookieCount = 0;
            object objBytes = Executor.ExecuteByIdToObject(proc.id, null, 1);
            if (CacheHelper.GetCache(proc.id) != null)
            {
                cookies = (CookieCollection)CacheHelper.GetCache(proc.id);
                cookieCount = cookies.Count;
            }
            string vercode = vercodeService.GetVerCode((byte[])objBytes);
            proc.Variable.Add("imagecode", vercode);
            Executor.ExecuteById(proc.id, null, proc.currentsort, proc.currentsort + 1);
            if (CacheHelper.GetCache(proc.id) != null)
            {
                cookies = (CookieCollection)CacheHelper.GetCache(proc.id);
            }
            if (cookieCount < cookies.Count)
            {
                Executor.ExecuteById(proc.id, null, proc.currentsort);
                return true;
            }
            else
            {
                if (count > 0)
                {
                    count--;
                    proc.Variable.Remove("imagecode");
                    return QingDaoForEachQuery(proc, count);
                }
                else
                {
                    return false;
                }
            }
        }
        private string GetYearAndMonth(string inputStr)
        {
            string year = inputStr.Substring(0, 4);
            string month = inputStr.Replace("年", "").Replace("月", "");
            month = month.Substring(4, month.Length - 4);
            if (month.Length == 1)
            {
                month = "0" + month;
            }
            return year + month;
        }


    }
}