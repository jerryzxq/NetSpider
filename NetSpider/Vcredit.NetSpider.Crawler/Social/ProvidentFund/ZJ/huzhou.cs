using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.ZJ
{
    public class huzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://wsbs.hzgjj.com/";
        string fundCity = "zj_huzhou";
        #endregion

        #region   私有变量
        decimal payrate=(decimal)0;
        string CompanyNo = string.Empty;//单位账号
        string DATAlISTGHOST = string.Empty;
        string _APPLY = string.Empty;
        string _CHANNEL = string.Empty;
        string _DATAPOOL_ = string.Empty;
        string _ISPAGE = string.Empty;
        string _PROCID = string.Empty;
        string accname = string.Empty;
        string accnum = string.Empty;
        string accnumbal1 = string.Empty;
        string afchgbasenum = string.Empty;
        string amt4 = string.Empty;
        string bal = string.Empty;
        string basenum = string.Empty;
        string cardno = string.Empty;
        string certinum = string.Empty;
        string cydpsum = string.Empty;
        string deductionamt = string.Empty;
        string dynamicTable_configSqlChe = string.Empty;
        string dynamicTable_currentPage = string.Empty;
        string dynamicTable_id = string.Empty;
        string dynamicTable_nextPage = string.Empty;
        string dynamicTable_page = string.Empty;
        string dynamicTable_pageSize = string.Empty;
        string dynamicTable_paging = string.Empty;
        string errorFilter = string.Empty;
        string freeuse2 = string.Empty;
        string idno = string.Empty;
        string indiaccstate = string.Empty;
        string indiprop = string.Empty;
        string keepbal = string.Empty;
        string lastbal = string.Empty;
        string opnaccbank = string.Empty;
        string opnaccdate = string.Empty;
        string path = string.Empty;
        string planpayamt = string.Empty;
        string prop = string.Empty;
        string unitaccname = string.Empty;
        string unitaccnum = string.Empty;
        string unitprop = string.Empty;
       
        #endregion
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        { 
            //string testx = "\u5fb7\u6e05\u6c11\u4fe1\u7535\u529b\u5b89\u88c5\u6709\u9650\u516c\u53f8";
          
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "hzwsyyt/vericode.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "公积金账号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登陆
                string _PROCID = string.Empty;
                string errorMsg = string.Empty;
                Url = baseUrl + "hzwsyyt/per.login";
                postdata = string.Format("certinum={0}&accnum={1}&perpwd={2}&vericode={3}", fundReq.Identitycard, fundReq.Username, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gbk"),
                    Referer = "http://wsbs.hzgjj.com/hzwsyyt/person.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                // errorMsg = CommonFun.GetMidStr(httpResult.Html, "<span class=\"error\"></span>提示 ", "");
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='WTLoginError']/ul/li[1]", "innertext");
                if (results.Count > 0)
                {
                    errorMsg = results[0];
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                _PROCID = CommonFun.GetMidStr(httpResult.Html, "'公积金信息查询', url: '/hzwsyyt/init.summer?_PROCID=", "'}");
                #endregion


                #region  第二步
                Url = baseUrl + "hzwsyyt/platform/homeQuery.jsp?uuid=1437617199779";
                postdata = string.Format("task=menu");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Encoding = Encoding.GetEncoding("gbk"),
                    Referer = "http://wsbs.hzgjj.com/hzwsyyt/per.login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion

                #region 第三步

                Url = baseUrl + string.Format("hzwsyyt/init.summer?_PROCID={0}", _PROCID);
                //http://wsbs.hzgjj.com/hzwsyyt/init.summer?_PROCID=610080
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("gbk"),
                    Referer = baseUrl + "hzwsyyt/per.login",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accname']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accnum']", "value");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='certinum']", "value");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='opnaccdate']", "value");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='opnaccbank']", "value");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='cardno']", "value");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitaccnum']", "value");
                if (results.Count > 0)
                {
                    CompanyNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitaccname']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='indiaccstate']", "value");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "");
                if (results.Count > 0)
                {
                    DATAlISTGHOST = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='basenum']", "");
                if (results.Count > 0)
                {
                    payrate = results[0].ToDecimal(0);//工资基数
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='DATAlISTGHOST']", "value");
                if (results.Count > 0)
                {
                    _APPLY = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='_CHANNEL']", "value");
                if (results.Count > 0)
                {
                    _CHANNEL = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "");
                if (results.Count > 0)
                {

                    _DATAPOOL_ = new Regex(@"[\s;]*").Replace(results[0], "");
                }
                if (results.Count > 0)
                {
                    _ISPAGE = results[0];
                }
                _ISPAGE = CommonFun.GetMidStr(httpResult.Html, "'_IS': '", "',");
                accname = Res.Name;
                accnum = Res.ProvidentFundNo;//个人账号3305266451540
                afchgbasenum = string.Empty;
                amt4 = string.Empty;
                basenum = string.Empty;
                cardno = Res.BankCardNo; //6227001447460069698
                certinum = Res.IdentityCard;//330521197402142616
                cydpsum = string.Empty;
                dynamicTable_configSqlChe = "0";
                dynamicTable_currentPage = "1";//当前页数
                dynamicTable_id = "datalist1";
                dynamicTable_nextPage = "";//下一页
                dynamicTable_page = "/ydpx/610080/610080_01.ydpx";
                dynamicTable_pageSize = "10";
                dynamicTable_paging = "true";
                errorFilter = "1=";
                freeuse2 = string.Empty;
                idno = Res.IdentityCard; //330521197402142616
                indiaccstate = Res.Status;
                indiprop = string.Empty;
                keepbal = string.Empty;
                lastbal = string.Empty;
                opnaccbank = Res.Bank;
                opnaccdate = Res.OpenTime;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='path']", "value");
                if (results.Count > 0)
                {
                    path = results[0];
                }
                planpayamt = string.Empty;
                prop = string.Empty;
                unitaccname = Res.CompanyName;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='unitaccnum']", "value");
                if (results.Count > 0)
                {
                    unitaccname = results[0];
                }
                unitprop = string.Empty;
                #endregion

                //unitaccnum

                #region 添加年份
                List<string> Years = new List<string>();
                for (int i = DateTime.Now.AddYears(-3).Year; i <= DateTime.Now.Year; i++)
                {
                    Years.Add(i.ToString());
                }

                #endregion

                #region 第四步

                string _IS = string.Empty;
                string CardAccBank = String.Empty;
                string CardAccNo = string.Empty;
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + string.Format("hzwsyyt/command.summer?uuid=1439789058230");
                //6222021205011115320
                postdata = string.Format("%24page=%2Fydpx%2F610080%2F610080_01.ydpx&_TYPE=init&_BRANCHKIND=0&_SENDOPERID={0}&_WITHKEY=0&_SYSTEMTYPE=1&isSamePer=true&_ACCNUM={1}&_PAGEID=step1&_DEPUTYIDCARDNUM={2}&_PORCNAME=%E5%85%AC%E7%A7%AF%E9%87%91%E4%BF%A1%E6%81%AF%E6%9F%A5%E8%AF%A2&_INWAY=1&_UNITACCNUM={3}&_PROCID={4}&_UNITACCNAME={5}&_IS={6}&_ISCROP=0&_LOGTIME=20150817133126968&rw=&_OPERID={7}&_SENDDATE=2015-08-17&_SENDTIME=13%3A39%3A58&_ACCNAME={8}&accname={9}&accnum={10}&certinum={11}&opnaccdate={12}&opnaccbank=&cardno=&unitaccnum={13}&unitaccname={14}&indiaccstate={15}&unitprop=&indiprop=&prop=&basenum=2%2C828.00&planpayamt=&freeuse2=&afchgbasenum=&cydpsum=&amt4=&accnumbal1=14%2C722.83&lastbal=&keepbal=&bal=&deductionamt=&path=http%3A%2F%2Fwww.hzgjj.com%2Fyyt.htm&idno={16}&AccNum={17}&type=0", Res.ProvidentFundNo, Res.ProvidentFundNo, Res.IdentityCard, CompanyNo, _PROCID, Res.CompanyName.ToUrlEncode(Encoding.GetEncoding("gbk")), _ISPAGE, Res.ProvidentFundNo, Res.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), Res.Name.ToUrlEncode(Encoding.GetEncoding("gbk")), Res.ProvidentFundNo, Res.IdentityCard, Res.OpenTime, CompanyNo, Res.CompanyName.ToUrlEncode(Encoding.GetEncoding("gbk")), Res.Status.ToUrlEncode(Encoding.GetEncoding("gbk")), Res.IdentityCard, Res.ProvidentFundNo);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",

                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                Res.PersonalMonthPayRate = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:PerProp").ToDecimal(0);//个人缴费比率
                Res.PersonalMonthPayAmount = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:IndiPayAmt").ToDecimal(0);//个人月缴费

                Res.CompanyMonthPayRate = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:UnitProp").ToDecimal(0);//公司缴费比率
                Res.CompanyMonthPayAmount = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:UnitPayAmt").ToDecimal(0);//公司月缴费
                //Salary
                Res.SalaryBase = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:Salary").ToDecimal(0); ;
                //CardAccBank
                // Res.SalaryBase = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:CardAccBank").ToDecimal(0); 
                //bal
                Res.TotalAmount = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:Balance").ToDecimal(0);
                _IS = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:_IS");
                Res.Bank = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:CardAccBank");
                Res.BankCardNo = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:CardAccNo");//unitaccnum
                #endregion


                //循环年份
                foreach (string year in Years)
                {
                    #region  第五步
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Url = baseUrl + string.Format("hzwsyyt/command.summer?uuid=1439794113545");
                    postdata = string.Format("%24page=%2Fydpx%2F610080%2F610080_01.ydpx&_TYPE=init&_BRANCHKIND=0&_SENDOPERID={0}&_WITHKEY=0&_SYSTEMTYPE=1&isSamePer=true&_ACCNUM={1}&_PAGEID=step1&_DEPUTYIDCARDNUM={2}&_PORCNAME={3}&_INWAY=1&_UNITACCNUM={4}&_PROCID={5}&_UNITACCNAME={6}&_IS={7}&_ISCROP=0&_LOGTIME=20150818090506738&rw=&_OPERID={8}&_SENDDATE=2015-08-18&_SENDTIME=09%3A05%3A09&_ACCNAME={9}&accname={10}&accnum={11}&certinum={12}&opnaccdate={13}&opnaccbank={14}&cardno={15}&unitaccnum={16}&unitaccname={17}&indiaccstate={18}&unitprop=8&indiprop=8&prop=0&basenum=2%2C828.00&planpayamt=227&freeuse2=227&afchgbasenum=0&cydpsum=0&amt4=454&accnumbal1=14%2C722.83&lastbal=0&keepbal=0&bal=14%2C722.83&deductionamt=0&path=http%3A%2F%2Fwww.hzgjj.com%2Fyyt.htm&idno={19}&type=1&AccNum={20}&SearchYear={21}", Res.ProvidentFundNo, Res.ProvidentFundNo, Res.IdentityCard, "公积金信息查询".ToUrlEncode(Encoding.GetEncoding("utf-8")), CompanyNo, _PROCID, Res.CompanyName.ToUrlEncode(Encoding.GetEncoding("utf-8")), "-6761542", Res.ProvidentFundNo, Res.Name.ToUrlEncode(Encoding.GetEncoding("utf-8")), Res.Name.ToUrlEncode(Encoding.GetEncoding("utf-8")), Res.ProvidentFundNo, Res.IdentityCard, Res.OpenTime, CardAccBank, CardAccNo, CompanyNo, Res.CompanyName.ToUrlEncode(Encoding.GetEncoding("utf-8")), Res.Status.ToUrlEncode(Encoding.GetEncoding("utf-8")), Res.IdentityCard, Res.ProvidentFundNo, year);

                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Encoding = Encoding.GetEncoding("gbk"),
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    //请求失败后返回
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    #endregion

                    string y = year;
                    #region  第六步
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    int i = 1;
                    while (true)
                    {
                        int test = DateTime.Now.Subtract(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Milliseconds;
                        Url = baseUrl + string.Format("hzwsyyt/dynamictable");

                        postdata = string.Format("dynamicTable_id=datalist1&dynamicTable_currentPage=0&dynamicTable_pageSize=10&dynamicTable_nextPage={0}&dynamicTable_page=%2Fydpx%2F610080%2F610080_01.ydpx&dynamicTable_paging=true&dynamicTable_configSqlCheck=0&_ISPAGE={1}&errorFilter=1%3D1&accname={2}&accnum={3}&certinum={4}&opnaccdate={5}&opnaccbank={6}&cardno={7}&unitaccnum={8}&unitaccname={9}&indiaccstate={10}&unitprop=8&indiprop=8&prop=0&basenum=2%2C828.00&planpayamt=227&freeuse2=227&afchgbasenum=0&cydpsum=0&amt4=454&accnumbal1=14%2C722.83&lastbal=0&keepbal=0&bal=14%2C722.83&deductionamt=0&_APPLY=0&_CHANNEL=1&_PROCID={11}&DATAlISTGHOST={12}&_DATAPOOL_=rO0ABXNyABZjb20ueWR5ZC5wb29sLkRhdGFQb29sp4pd0OzirDkCAAZMAAdTWVNEQVRFdAASTGph%0AdmEvbGFuZy9TdHJpbmc7TAAGU1lTREFZcQB%2BAAFMAAhTWVNNT05USHEAfgABTAAHU1lTVElNRXEA%0AfgABTAAHU1lTV0VFS3EAfgABTAAHU1lTWUVBUnEAfgABeHIAEWphdmEudXRpbC5IYXNoTWFwBQfa%0AwcMWYNEDAAJGAApsb2FkRmFjdG9ySQAJdGhyZXNob2xkeHA%2FQAAAAAAAGHcIAAAAIAAAABh0AAVf%0AVFlQRXQABGluaXR0AAtfQlJBTkNIS0lORHQAATB0ABNDVVJSRU5UX1NZU1RFTV9EQVRFcHQAC19T%0ARU5ET1BFUklEdAANMzMwNTI2NTg2MjM0M3QACF9XSVRIS0VZcQB%2BAAd0AAtfU1lTVEVNVFlQRXQA%0AATF0AAlpc1NhbWVQZXJ0AAR0cnVldAAHX0FDQ05VTXEAfgAKdAAHX1BBR0VJRHQABXN0ZXAxdAAQ%0AX0RFUFVUWUlEQ0FSRE5VTXQAEjM2MDIyMjE5OTQwMTA2NjgxMnQACV9QT1JDTkFNRXQAFeWFrOen%0Ar%2BmHkeS%2FoeaBr%2BafpeivonQABl9JTldBWXEAfgANdAALX1VOSVRBQ0NOVU10AAwzMzA1MjEwMzE4%0AOTd0AAdfUFJPQ0lEdAAGNjEwMDgwdAAMX1VOSVRBQ0NOQU1FdAAk5rWZ5rGf6byO5Yqb5py65qKw%0A6IKh5Lu95pyJ6ZmQ5YWs5Y%2B4dAADX0lTc3IADmphdmEubGFuZy5Mb25nO4vkkMyPI98CAAFKAAV2%0AYWx1ZXhyABBqYXZhLmxhbmcuTnVtYmVyhqyVHQuU4IsCAAB4cP%2F%2F%2F%2F%2F%2FmNO6dAAHX0lTQ1JPUHEA%0AfgAHdAAHX1VTQktFWXB0AAhfTE9HVElNRXQAETIwMTUwODE4MDkwNTA2NzM4dAACcnd0AAB0AAdf%0AT1BFUklEcQB%2BAAp0AAlfU0VORERBVEV0AAoyMDE1LTA4LTE4dAAJX1NFTkRUSU1Fc3IADWphdmEu%0Ac3FsLlRpbWV0iUoN2TLEcQIAAHhyAA5qYXZhLnV0aWwuRGF0ZWhqgQFLWXQZAwAAeHB3CAAAAU8%2B%0AVqQ2eHQACF9BQ0NOQU1FdAAG5ZC056OKeHQACEBTeXNEYXRldAAHQFN5c0RheXQACUBTeXNNb250%0AaHQACEBTeXNUaW1ldAAIQFN5c1dlZWt0AAhAU3lzWWVhcg%3D%3D&path=http%3A%2F%2Fwww.hzgjj.com%2Fyyt.htm&idno={13}", i, "-6761542", Res.Name.ToUrlEncode(Encoding.GetEncoding("utf-8")), Res.ProvidentFundNo, Res.IdentityCard, Res.OpenTime, "工行".ToUrlEncode(Encoding.GetEncoding("utf-8")), "6222021205011115320", CompanyNo, Res.CompanyName.ToUrlEncode(Encoding.GetEncoding("utf-8")), Res.Status.ToUrlEncode(Encoding.GetEncoding("utf-8")), _PROCID, DATAlISTGHOST, Res.IdentityCard);
                        httpItem = new HttpItem
                        {
                            URL = Url,
                            Method = "Post",
                            Postdata = postdata,
                            //Encoding = Encoding.GetEncoding("utf-8"),
                            Referer = baseUrl + "hzwsyyt/init.summer?_PROCID=610080",
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        string testxx = Regex.Unescape(httpResult.Html);
                        List<HuzhouDetail> HuzhouDetail = jsonParser.DeserializeObject<List<HuzhouDetail>>(jsonParser.GetResultFromMultiNode(httpResult.Html.ToString(), "data:data"));

                        string count = CommonFun.GetMidStr(httpResult.Html, "pageCount\":", ",\"");
                        //string custAcct = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:pageCount");
                        if (i > count.ToInt()) break;
                        i++;
                        foreach (var item in HuzhouDetail)
                        {
                            detail = new ProvidentFundDetail();

                            // detail.PayTime = item.unitaccnum2.ToDateTime();
                            detail.PayTime = item.freeuse1.Insert(4, "-").Insert(7, "-").ToDateTime();
                            if (item.unitaccnum1.IndexOf("托收") > -1)
                            {
                                detail.ProvidentFundTime = item.unitaccnum2;
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                detail.PersonalPayAmount = item.accname2.ToDecimal(0) / 2;//金额
                                detail.CompanyPayAmount = item.accname2.ToDecimal(0) / 2;//金额
                                detail.ProvidentFundBase = Math.Round(detail.PersonalPayAmount / Res.PersonalMonthPayRate, 2);//缴费基数
                                detail.Description = item.accname1;
                                PaymentMonths++;
                            }
                            else if (item.unitaccnum1.IndexOf("结息") > -1 || item.unitaccnum1.IndexOf("结转") > -1)
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.Description = item.accname1;
                                detail.PersonalPayAmount = item.accname2.ToDecimal(0);
                            }
                            else
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.Description = item.accname1;
                            }
                            Res.ProvidentFundDetailList.Add(detail);
                        }
                    }
                    #endregion
                }

                #region 贷款
                #region 贷款基本信息
                Url = baseUrl + "hzwsyyt/init.summer?_PROCID=610090";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='loancontrnum']/option", "value");
                if (results.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(results[0])) goto End;
                }
                string dic_html = httpResult.Html;
                postdata = "";
                Dictionary<string, string> dict = new Dictionary<string,string>();
                try
                {
                    DATAlISTGHOST = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']")[0];
                    _DATAPOOL_ = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']")[0];

                    string jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                    jsonStr = "{" + jsonStr + "}";
                    dict = (Dictionary<string, string>)jsonParser.GetStringDictFromParser(jsonStr);

                    foreach (KeyValuePair<string, string> item in dict)
                    {
                        postdata += item.Key.ToUrlEncode() + "=" + item.Value.ToString().ToUrlEncode() + "&";
                    }
                }
                catch { goto End; }
                string temppostdata = postdata;
                postdata += string.Format("loancontrnum=&accname1=&transdate=&loanterm=&amt=&ahdrepayamt=&incint=&repaypun=&repaymode=21&rate=&loanbal=&terms=&state=&address=&planprin=&planint=&totalincome=&overdueprin=&repowecnt=&path=http%3A%2F%2Fwww.hzgjj.com%2Fyyt.htm&idno={0}&AccNum={1}&type=1", fundReq.Identitycard, fundReq.Username);
                Url = baseUrl + "hzwsyyt/command.summer";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                LoanBaseInfo _loanbaseinfo = new LoanBaseInfo();
                try
                {
                    _loanbaseinfo = jsonParser.DeserializeObject<LoanBaseInfo>(jsonParser.GetResultFromParser(httpResult.Html.FromUnicode(), "data"));

                    Res_Loan.Loan_Sid = _loanbaseinfo.LoanContrNum;
                    Res_Loan.Loan_Start_Date = _loanbaseinfo.LoanDate;
                    Res_Loan.Period_Total = _loanbaseinfo.LoanTerm;
                    Res_Loan.Loan_Credit = _loanbaseinfo.LoanAmt.ToDecimal(0);
                    Res_Loan.Principal_Payed = _loanbaseinfo.RepayPrin.ToDecimal(0);
                    Res_Loan.Interest_Payed = _loanbaseinfo.RepayInt.ToDecimal(0);
                    Res_Loan.Interest_Penalty = _loanbaseinfo.RepayPun.ToDecimal(0);
                    results = HtmlParser.GetResultFromParser(dic_html, "//select[@id='repaymode']/option[@value='" + _loanbaseinfo.RepayMode + "']");
                    if (results.Count > 0)
                    {
                        Res_Loan.Repay_Type = results[0];
                    }
                    Res_Loan.Loan_Rate = _loanbaseinfo.MonthRate;
                    Res_Loan.Loan_Balance = _loanbaseinfo.LoanBal.ToDecimal(0);
                    Res_Loan.Period_Payed = Res_Loan.Period_Total.ToInt(0) - _loanbaseinfo.LastTerms.ToInt(0);
                    string MonthPayFlag = string.Empty;
                    switch (_loanbaseinfo.MonthPayFlag)
                    {
                        case "":
                            MonthPayFlag = "无月冲";
                            break;
                        case "空":
                            MonthPayFlag = "无月冲";
                            break;
                        case "01":
                            MonthPayFlag = "申请";
                            break;
                        case "02":
                            MonthPayFlag = "正常";
                            break;
                        case "03":
                            MonthPayFlag = "终止";
                            break;
                        case "04":
                            MonthPayFlag = "作废";
                            break;
                    }
                    Res_Loan.Status = MonthPayFlag;
                    Res_Loan.Address = _loanbaseinfo.Address;
                    Res_Loan.Current_Repay_Principal = _loanbaseinfo.PlanPrin.ToDecimal(0);
                    Res_Loan.Current_Repay_Interest = _loanbaseinfo.PlanInt.ToDecimal(0);
                    Res_Loan.Current_Repay_Total = _loanbaseinfo.PlanAmt.ToDecimal(0);
                    Res_Loan.Overdue_Principal = _loanbaseinfo.OverdueAmt.ToDecimal(0);
                    Res_Loan.Overdue_Period = _loanbaseinfo.OverdueTimes.ToInt(0);
                }
                catch { goto End; }
                #endregion

                #region 贷款明细
                Url = baseUrl + "hzwsyyt/command.summer";
                temppostdata += "loancontrnum=0&accname1=" + Res.Name.ToUrlEncode();
                temppostdata += string.Format("&transdate={0}&loanterm={1}&amt={2}&ahdrepayamt={3}&incint={4}&repaypun={5}&repaymode={6}&rate={7}&loanbal={8}&terms={9}&state={10}&address={11}&planprin={12}&planint={13}&totalincome={14}&overdueprin={15}&repowecnt={16}", Res_Loan.Loan_Start_Date, Res_Loan.Period_Total, Res_Loan.Loan_Credit, Res_Loan.Principal_Payed, Res_Loan.Interest_Payed, Res_Loan.Interest_Penalty, _loanbaseinfo.RepayMode, Res_Loan.Loan_Rate.ToDecimal(0) * 100, Res_Loan.Loan_Balance, _loanbaseinfo.LastTerms, Res_Loan.Status.ToUrlEncode(), Res_Loan.Address.ToUrlEncode(), Res_Loan.Current_Repay_Principal, Res_Loan.Current_Repay_Interest, Res_Loan.Current_Repay_Total, Res_Loan.Overdue_Principal, Res_Loan.Overdue_Period);
                temppostdata += string.Format("&path=http%3A%2F%2Fwww.hzgjj.com%2Fyyt.htm&idno={0}&AccNum={1}&type=0&loanaccnum={2}", Res.IdentityCard, fundReq.Username, Res_Loan.Loan_Sid);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = temppostdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                int currentPage = 0;
                int pagecount = 0;
                do
                {
                    Url = baseUrl + "hzwsyyt/dynamictable";
                    postdata = "dynamicTable_id=datalist1";
                    postdata += "&dynamicTable_currentPage=" + currentPage;
                    postdata += "&dynamicTable_pageSize=10";
                    postdata += "&dynamicTable_nextPage=" + (currentPage + 1);
                    postdata += "&dynamicTable_page=%2Fydpx%2F610090%2F610090_01.ydpx";
                    postdata += "&dynamicTable_paging=true";
                    postdata += "&dynamicTable_configSqlCheck=0";
                    postdata += "&_ISPAGE=" + dict["_IS"];
                    postdata += "&errorFilter=1%3D1";
                    postdata += "&loancontrnum=0&accname1=" + Res.Name.ToUrlEncode();
                    postdata += string.Format("&transdate={0}&loanterm={1}&amt={2}&ahdrepayamt={3}&incint={4}&repaypun={5}&repaymode={6}&rate={7}&loanbal={8}&terms={9}&state={10}&address={11}&planprin={12}&planint={13}&totalincome={14}&overdueprin={15}&repowecnt={16}", Res_Loan.Loan_Start_Date, Res_Loan.Period_Total, Res_Loan.Loan_Credit, Res_Loan.Principal_Payed, Res_Loan.Interest_Payed, Res_Loan.Interest_Penalty, _loanbaseinfo.RepayMode, Res_Loan.Loan_Rate.ToDecimal(0) * 100, Res_Loan.Loan_Balance, _loanbaseinfo.LastTerms, Res_Loan.Status.ToUrlEncode(), Res_Loan.Address.ToUrlEncode(), Res_Loan.Current_Repay_Principal, Res_Loan.Current_Repay_Interest, Res_Loan.Current_Repay_Total, Res_Loan.Overdue_Principal, Res_Loan.Overdue_Period);
                    postdata += "&_APPLY=0&_CHANNEL=1&_PROCID=610090";
                    postdata += "&DATAlISTGHOST=" + DATAlISTGHOST.ToUrlEncode();
                    postdata += "&_DATAPOOL_=" + _DATAPOOL_.ToUrlEncode();
                    postdata += "&path=" + "http://www.hzgjj.com/yyt.htm".ToUrlEncode();
                    postdata += "&idno=" + fundReq.Identitycard;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = "http://wsbs.hzgjj.com/hzwsyyt/init.summer?_PROCID=610090",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    try
                    {
                        if (pagecount == 0)
                        {
                            pagecount = jsonParser.GetResultFromMultiNode(httpResult.Html, "data:pageCount").ToInt(0);
                        }
                        List<LoanDetail> _loandetail = jsonParser.DeserializeObject<List<LoanDetail>>(jsonParser.GetResultFromMultiNode(httpResult.Html.FromUnicode(), "data:data"));
                        foreach (LoanDetail _detail in _loandetail)
                        {
                            ProvidentFundLoanDetail _ProvidentFundLoanDetail = new ProvidentFundLoanDetail();
                            _ProvidentFundLoanDetail.Bill_Date = _detail.idnum1;
                            _ProvidentFundLoanDetail.Record_Date = _detail.freeuse1;
                            _ProvidentFundLoanDetail.Balance = _detail.freeuse2.ToDecimal(0);
                            _ProvidentFundLoanDetail.Principal = _detail.accnum2.ToDecimal(0);
                            _ProvidentFundLoanDetail.Interest = _detail.unitaccnum1.ToDecimal(0);
                            _ProvidentFundLoanDetail.Interest_Penalty = _detail.unitaccnum2.ToDecimal(0);
                            _ProvidentFundLoanDetail.Record_Period = _detail.accnum1;
                            _ProvidentFundLoanDetail.Description = _detail.freeuse3;

                            Res_Loan.ProvidentFundLoanDetailList.Add(_ProvidentFundLoanDetail);
                        }
                    }
                    catch { }
                    currentPage++;
                } while (currentPage < pagecount);

                #endregion
                #endregion

                Res.ProvidentFundLoanRes = Res_Loan;
            End:
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        class HuzhouDetail
        {
            public string freeuse2 { get; set; }
            public string freeuse1 { get; set; }
            public string instancenum { get; set; }
            public string unitaccnum1 { get; set; }
            public string unitaccnum2 { get; set; }
            public string accnum2 { get; set; }
            public string accnum1 { get; set; }
            public string accname2 { get; set; }
            public string accname1 { get; set; }
        }

        class LoanBaseInfo
        {
            public string LoanContrNum { get; set; }//借款合同号
            public string loancontrnum { get; set; }
            public string LoanDate { get; set; }//贷款日期
            public string LoanTerm { get; set; }//贷款期限
            public string loanterm { get; set; }
            public string LoanAmt { get; set; }//贷款金额
            public string RepayPrin { get; set; }//已还本金
            public string RepayInt { get; set; }//实收利息
            public string RepayPun { get; set; }//实还罚息
            public string repaypun { get; set; }
            public string RepayMode { get; set; }//还款方式
            public string repaymode { get; set; }
            public string MonthRate { get; set; }//月利率
            public string LoanBal { get; set; }//贷款余额
            public string loanbal { get; set; }
            public string LastTerms { get; set; }//剩余期数
            public string MonthPayFlag { get; set; }//月冲状态
            public string Address { get; set; }//房屋地址
            public string address { get; set; }
            public string PlanPrin { get; set; }//本期应还本金
            public string planprin { get; set; }
            public string PlanInt { get; set; }//本期应还利息
            public string planint { get; set; }
            public string PlanAmt { get; set; }//本期应还合计
            public string OverdueAmt { get; set; }//逾期金额
            public string OverdueTimes { get; set; }//累计逾期期数
        }

        class LoanDetail
        {
            public string freeuse2 { get; set; }
            public string freeuse1 { get; set; }
            public string unitaccnum1 { get; set; }
            public string unitaccnum2 { get; set; }
            public string accnum2 { get; set; }
            public string accnum1 { get; set; }
            public string idnum1 { get; set; }
            public string freeuse3 { get; set; }
        }
    }

}
