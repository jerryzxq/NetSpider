using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.DataAccess.Cache;
namespace Vcredit.NetSpider.Crawler.Social.SocialSecurity.ZJ
{
    public class hangzhou : ISocialSecurityCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "";
        string socialCity = "zj_hangzhou";
        #endregion

        #region 私有方法
        enum InfoType
        {
            养老保险,
            医疗保险,
            失业保险
        }
        Hashtable PageHash = new Hashtable();//存放各种保险对应的页面信息
        Hashtable YearDetail = new Hashtable();//存放近5年的信息
        Hashtable ThisYearPayMonths = new Hashtable();
        /// <summary>
        /// 将各种保险对应的页面所需信息存入PageHash
        /// </summary>
        void InitPageHash()
        {
            PageHash.Add(InfoType.养老保险, new string[] { "11" });
            PageHash.Add(InfoType.医疗保险, new string[] { "31" });
            PageHash.Add(InfoType.失业保险, new string[] { "21" });
        }
        /// <summary>
        /// 获取某类保险的某页的信息
        /// </summary>
        /// <param name="type">保险类型</param>
        void GetAllDetail(InfoType Type, HttpClient httpClient, ref SocialSecurityQueryRes Res)
        {
            string Url = string.Empty;
            string postdata = string.Empty;
            string responseString = string.Empty;
            byte[] responseData;
            List<string> results = new List<string>();
            //HttpClient httpClient = new HttpClient();

            for (int i = 0; i < 5; i++)
            {
                int Year = DateTime.Now.AddYears(-i).Year;
                int pageNo = 1;
                bool isFinish = false;
                do
                {
                    Url = string.Format("http://wsbs.zjhz.hrss.gov.cn/unit/web_zgjf_query/web_zgjf_doQuery.html?m_aae002={0}&m_aae140={1}&pageNo={2}", Year, ((string[])PageHash[Type])[0], pageNo);
                    responseData = httpClient.DownloadData(Url);//得到返回字符流  
                    responseString = Encoding.UTF8.GetString(responseData);//解码 
                    results = HtmlParser.GetResultFromParser(responseString, "//table[@class='grid']/tr[position()>1]", "inner", true);

                    List<string> EachYearDetail = null;
                    if (YearDetail[Year] != null)
                        EachYearDetail = (List<string>)YearDetail[Year];

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 7 || tdRow[0] == "")
                        {
                            isFinish = true;
                            continue;
                        }

                        string SocialInsuranceTime = tdRow[0];
                        SocialSecurityDetailQueryRes detailRes = null;
                        bool NeedSave = false;
                        if (Type != InfoType.养老保险)
                        {
                            if (tdRow[6] == "正常应缴")
                                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && o.PaymentType == ServiceConsts.SocialSecurity_PaymentType_Normal).FirstOrDefault();
                            else
                                detailRes = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && o.PaymentType != ServiceConsts.SocialSecurity_PaymentType_Normal).FirstOrDefault();
                        }

                        if (detailRes == null)
                        {
                            NeedSave = true;
                            detailRes = new SocialSecurityDetailQueryRes();
                            detailRes.Name = Res.Name;
                            detailRes.IdentityCard = Res.IdentityCard;
                            detailRes.PayTime = tdRow[0];
                            detailRes.SocialInsuranceTime = tdRow[0];
                            detailRes.SocialInsuranceBase = tdRow[2].ToDecimal(0);
                            //detailRes.PensionAmount = tdRow[3].ToDecimal(0);
                            detailRes.CompanyName = tdRow[5];
                            if (tdRow[6] == "正常应缴")
                            {
                                detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                            }
                            else
                            {
                                detailRes.PaymentType = tdRow[6];
                                detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            }
                            if (EachYearDetail != null)
                                detailRes.YearPaymentMonths = EachYearDetail[0].ToInt(0);
                        }

                        switch (Type)
                        {
                            case InfoType.养老保险:
                                detailRes.PensionAmount = tdRow[3].ToDecimal(0);
                                if (i == 0)
                                {
                                    try
                                    {
                                        ThisYearPayMonths.Add(tdRow[0], 1);
                                    }
                                    catch { }
                                }
                                break;
                            case InfoType.医疗保险:
                                detailRes.MedicalAmount = tdRow[3].ToDecimal(0);
                                break;
                            case InfoType.失业保险:
                                detailRes.UnemployAmount = tdRow[3].ToDecimal(0);
                                break;
                        }

                        if(NeedSave)
                            Res.Details.Add(detailRes);
                    }
                    pageNo++;
                }
                while (!isFinish);
            }

            if (Type == InfoType.养老保险)
            {
                Res.PaymentMonths += ThisYearPayMonths.Count;
            }
        }
        #endregion
        public VerCodeRes SocialSecurityInit()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            HttpClient httpClient = new HttpClient();
            byte[] responseData;
            try
            {
                Url = "http://wsbs.zjhz.hrss.gov.cn/captcha.svl";
                responseData = httpClient.DownloadData(Url);//得到返回字符流  

                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, responseData);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(responseData);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(token, httpClient);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_InitError;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_InitError, e);
            }
            return Res;
        }

        public SocialSecurityQueryRes SocialSecurityQuery(SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SocialSecurityDetailQueryRes detailRes = null;
            Res.SocialSecurityCity = socialCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            string responseString = string.Empty;
            byte[] responseData;
            int PaymentMonths = 0;
            List<string> results = new List<string>();
            HttpClient httpClient = new HttpClient();

            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(socialReq.Token) != null)
                {
                    httpClient = (HttpClient)SpiderCacheHelper.GetCache(socialReq.Token);
                    SpiderCacheHelper.RemoveCache(socialReq.Token);
                }
                //校验参数
                if (socialReq.Identitycard.IsEmpty() || socialReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录



                Url = "http://wsbs.zjhz.hrss.gov.cn/loginvalidate.html?logintype=2&captcha=" + socialReq.Vercode;
                postdata = String.Format("type=01&persontype=01&account={0}%40hz.cn&password={1}&captcha1={2}", socialReq.Identitycard, socialReq.Password, socialReq.Vercode);
                httpClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                responseData = httpClient.UploadData(Url, "POST", Encoding.UTF8.GetBytes(postdata));//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码  

                responseString = CommonFun.GetMidStr(responseString, "['", "']");
                if (responseString != "success")
                {
                    Res.StatusDescription = "账号或密码错误";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第二步，个人基本信息
                Url = "http://wsbs.zjhz.hrss.gov.cn/person/personInfo/index.html";
                responseData = httpClient.DownloadData(Url);//得到返回字符流  
                responseString = Encoding.UTF8.GetString(responseData);//解码 
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[1]/td[2]", "text", true);

                if (results.Count > 0)
                {
                    Res.Name = results[0];//姓名
                }
                else
                {
                    Res.StatusDescription = Res.SocialSecurityCity + ServiceConsts.SocialSecurity_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[1]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];//身份证号
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[2]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeNo = results[0];//个人编号
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[2]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.Sex = results[0];//性别
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[3]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.Race = results[0];//民族
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[3]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.BirthDate = results[0];//出生日期

                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[4]/td[2]", "text", true);
                if (results.Count > 0)
                {
                    Res.WorkDate = results[0];
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[4]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.EmployeeStatus = results[0];
                }
                results = HtmlParser.GetResultFromParser(responseString, "/html/body/table/tr/td/table[2]/tr/td/table/tr[7]/td[4]", "text", true);
                if (results.Count > 0)
                {
                    Res.Address = results[0];
                }
                #endregion

                #region 第三步，养老

                //int EndYear = DateTime.Now.Year;
                for (int i = 0; i < 5; i++)
                {
                    YearDetail.Add(DateTime.Now.AddYears(-i).Year, null);
                }

                int pageNo = 1;
                bool isFinish = false;
                int payMonth = 0;
                string payYear = string.Empty;
                decimal insuranceBase = 0;
                do
                {
                    Url = "http://wsbs.zjhz.hrss.gov.cn/person/ylgrzhQuery/index.html";
                    postdata = String.Format("pageNo={0}&message=", pageNo);
                    httpClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    responseData = httpClient.UploadData(Url, "POST", Encoding.UTF8.GetBytes(postdata));//得到返回字符流  
                    responseString = Encoding.UTF8.GetString(responseData);//解码  

                    results = HtmlParser.GetResultFromParser(responseString, "//table[@class='grid']/tr[position()>4]", "inner", true);

                    foreach (string item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                        if (tdRow.Count != 21 || tdRow[0] == "&nbsp;")
                        {
                            isFinish = true;
                            continue;
                        }
                        if (tdRow[3] == "0&nbsp;")
                        {
                            continue;
                        }
                        payYear = tdRow[0].ToTrim("&nbsp;");
                        payMonth = tdRow[3].ToTrim("&nbsp;").ToInt(0);
                        //insuranceBase = tdRow[2].ToTrim("&nbsp;").ToDecimal(0) / payMonth;
                        PaymentMonths += payMonth;



                        if (YearDetail.Contains(payYear.ToInt(0)))
                        {
                            List<string> EachYearDetail = new List<string>();
                            EachYearDetail.Add(payMonth.ToString());

                            YearDetail[payYear.ToInt(0)] = EachYearDetail;
                        }

                        //for (int i = 1; i <= payMonth; i++)
                        //{
                        //    detailRes = new SocialSecurityDetailQueryRes();
                        //    detailRes.Name = Res.Name;
                        //    detailRes.IdentityCard = Res.IdentityCard;

                        //    if (i < 10)
                        //    {
                        //        detailRes.PayTime = payYear + "0" + i;
                        //        detailRes.SocialInsuranceTime = payYear + "0" + i;
                        //    }
                        //    else
                        //    {
                        //        detailRes.PayTime = payYear + i;
                        //        detailRes.SocialInsuranceTime = payYear + i;
                        //    }

                        //    detailRes.SocialInsuranceBase = insuranceBase;
                        //    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                        //    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;

                        //    //养老
                        //    detailRes.PensionAmount = tdRow[5].ToTrim("&nbsp;").ToDecimal(0);
                        //    detailRes.CompanyPensionAmount = tdRow[6].ToTrim("&nbsp;").ToDecimal(0);

                        //    Res.Details.Add(detailRes);
                        //}
                    }
                    pageNo++;
                }
                while (!isFinish);
                #region 养老详细数据
                
                #endregion
                #endregion

                //Res.PaymentMonths = PaymentMonths;

                Res.PaymentMonths = PaymentMonths;

                InitPageHash();

                foreach (InfoType info in Enum.GetValues(typeof(InfoType)))
                {
                    try
                    {
                        GetAllDetail(info, httpClient, ref Res);
                    }
                    catch
                    {
                        return Res;
                    }
                }


                Res.StatusDescription = socialCity + ServiceConsts.SocialSecurity_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(socialCity + ServiceConsts.SocialSecurity_QueryError, e);
            }
            return Res;
        }
    }
}
