using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using LumiSoft.Net.IMAP;
using LumiSoft.Net.IMAP.Client;
using LumiSoft.Net.Mail;
using LumiSoft.Net.Mime;
using LumiSoft.Net.MIME;
using LumiSoft.Net.POP3.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using S22.Imap;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Processor;
using Vcredit.Common.Ext;

namespace TestConsoleApp
{
    public class ProvidentFundTool
    {
        static IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        static IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        static IPluginSecurityCode secodeParser = PluginServiceManager.GetSecurityCodeParserPlugin();
        static HttpHelper http = new HttpHelper();
        static CookieCollection cookies = new CookieCollection();

        /// <summary>
        /// 上海
        /// </summary>
        public static void GetShanghai()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                //url = "https://persons.shgjj.com";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //cookies = httpRes.CookieCollection;

                url = "https://persons.shgjj.com/VerifyImageServlet";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                string vercode = secodeParser.GetVerCode(httpRes.ResultByte);

                url = "https://persons.shgjj.com/SsoLogin";
                string username = "zzb980881";
                string password = "082713";
                string passwordMD5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5"); ;
                postdata = String.Format("username={0}&password={1}&imagecode={2}&password_md5={3}&ID=0", username, password, vercode, passwordMD5);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://bbs.shgjj.com/sso/sso.php?url=https://persons.shgjj.com/MainServlet?ID=1";
                postdata = String.Format("para1={0}&para2={1}", username, passwordMD5);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = HtmlParser.GetResultFromParser(httpRes.Html, "//script[1]", "src")[0];
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "https://persons.shgjj.com/MainServlet?ID=1";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "https://persons.shgjj.com/MainServlet?ID=11";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                results = HtmlParser.GetResultFromParser(httpRes.Html, "//TABLE[@class='table']", "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 苏州
        /// </summary>
        public static void GetSuzhou()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {

                url = "https://gr.szgjj.gov.cn/retail/validateCodeServlet";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCode(httpRes.ResultByte);

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "https://gr.szgjj.gov.cn/retail/service";
                string custacno = "0967751948-1";
                string logontype = "1";
                string paperid = "321322198612217216";

                if (custacno.IndexOf("-") != -1)
                {
                    custacno = custacno.Substring(0, custacno.IndexOf('-'));
                }
                if (custacno.Length < 10)
                {
                    custacno = "0000000000" + custacno;
                    custacno = custacno.Substring(custacno.Length - 10);
                }
                if (custacno.Length > 11)
                {
                    logontype = "2";
                }
                postdata = String.Format("service=com.jbsoft.i2hf.retail.services.UserLogon.unRegUserLogon&custacno={0}&paperid={1}&paperkind=A&logontype={3}&validateCode={2}", custacno, paperid, vercode, logontype);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string sid = CommonFun.GetMidStr(httpRes.Html, "window.parent.location='internet?sid=", "' + '");

                url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid + "&service=com.jbsoft.i2hf.retail.services.UserAccService.getBaseAccountInfo";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//td[@class='col2-1']", "");
                results.AddRange(HtmlParser.GetResultFromParser(httpRes.Html, "//td[@class='col2-2']", ""));
                foreach (string item in results)
                {
                    Console.WriteLine(item.Replace("&nbsp;", "").Replace(" ", ""));
                }

                url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid + "&service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfo";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string startTime = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='startTime']", "value")[0].Replace("-", "");
                string endTime = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='endTime']", "value")[0].Replace("-", "");

                url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid;
                postdata = string.Format("service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfoJSON&acdateFrom={0} 00:00:00&acdateTo={1} 23:59:59&busidetailtype=&page=1", startTime, endTime);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetSuzhou_GYQ()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {

                string sessionid = string.Empty;
                string param3 = string.Empty;
                url = "http://www.sipspf.org.cn/person_online/emp/loginend.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                sessionid = CommonFun.GetMidStrByRegex(httpRes.Html, "var sessionid = \"", "\";");

                url = "http://www.sipspf.org.cn/person_online/service/identify.do?sessionid=" + sessionid;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCode(httpRes.ResultByte);

                url = "http://www.sipspf.org.cn/person_online/service/problem.do?sessionid=" + sessionid;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Console.WriteLine(httpRes.Html);

                string answer = "";
                Console.Write("请输入答案：");
                answer = Console.ReadLine();
                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://www.sipspf.org.cn/person_online/service/EMPLogin/login?wqcall=1417161422909";
                string uname = "02915686";
                string upass = Utilities.SecurityToMD5("314333").ToLower(); //"b5153024f4a968ecb69709cc2906ac1f";
                //param3 = "78ff0a05fd3ac016df3720d5ebc6de4c2f820f5f94efd76cf3810f8751274b54b095605e27b675cf30775020c66155da70658a98cf3bb9b51b74140ca6743020073b0b81939a597be9ab5ab2044adcd3c730024490b8894c839ade56dd188fb4cf19ef35f054687e4aab557da14ad2fb4ab1c0897d1d110c96458711d918b203";
                param3 = "5d44dc5d5dc3fb27dc94b87dd8a8f8c99e7ccf1930fa83b20be65664caf12601f82a78b49676038d87b5ab1923fe5abf4671760060aeadaa4dae5619ea26ea7e578c6e07cb706b60699e7f568b02df6b60ba824847ccc15677995f56a30c344fa1af60f8fd0320975dacfa0c0fd865c090f0d737d91bdd265501dc4f63f08067";
                postdata = String.Format("uname={0}&upass={1}&sessionid={2}&identify={3}&answer={4}&param3={5}", uname, upass, sessionid, vercode, answer, param3);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string regkey = httpRes.Html;

                url = string.Format("http://www.sipspf.org.cn/person_online/service/EmpInfo/getInfo?wqcall=1417162229906&uname={0}&thistype=firstdo", uname);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = string.Format("http://www.sipspf.org.cn/person_online/servlet/PersonRetiringServlet");
                postdata = "membid" + uname;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = string.Format("http://www.sipspf.org.cn/person_online/emp/queryinfo.jsp?random=1417164727833");
                postdata = "thistype=firstdo&uname=" + uname;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = string.Format("http://www.sipspf.org.cn/person_online/service/EMPLogin/validLogin?wqcall=1417162706941&regkey={0}", regkey);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //此步有问题
                url = "http://www.sipspf.org.cn/sipspf/web/auth/account?random=1417162707099";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 成都
        /// </summary>
        public static void GetChengdu()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {

                url = "http://www.cdzfgjj.gov.cn/api.php?op=checkcode&code_len=4&font_size=20&width=130&height=50";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCode(httpRes.ResultByte);

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=login";
                string username = "6222108327840856";
                string password = "602178";
                postdata = String.Format("cardNo={0}&password={1}&verifyCode={2}", username, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//div[@class='content guery']", "");

                url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=account";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//table[@class='form-table']/tr/td[@class='c']", "");

                string stime = "2000-01-01";
                string etime = DateTime.Now.ToString("yyyy-MM-dd");
                url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=detailquery";
                postdata = string.Format("startDate={0}&endDate={1}", stime, etime);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//tbody/tr", "");
                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetHangzhou()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://www.hzgjj.gov.cn:8080/WebAccounts/pages/per/perLogin.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string postUrl = HtmlParser.GetResultFromParser(httpRes.Html, "//form[@name='userLoginForm']", "action")[0];

                url = "http://www.hzgjj.gov.cn:8080/WebAccounts/codeMaker";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCode(httpRes.ResultByte);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.hzgjj.gov.cn:8080" + postUrl;
                string cust_no = "018238266572";
                string password = "123456";

                postdata = String.Format("cust_type=2&flag=1&user_type_2=1&user_type=1&cust_no={0}&password={1}&validate_code={2}&checkbox=", cust_no, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perComInfo.do?flag=1";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                postUrl = CommonFun.GetMidStrByRegex(httpRes.Html, "WebAccounts/perBillNext.do?", "\">明细");

                url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perBillDetial.do";
                postdata = string.Format("check_ym=2014&button1=+%B2%E9%D1%AF+&flag=0&begin_date=20140101&end_date=20141128&{0}", postUrl);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    CookieCollection = cookies,
                    Postdata = postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perComInfo.do";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                postUrl = CommonFun.GetMidStrByRegex(httpRes.Html, "/WebAccounts/comPerInfo.do?", "\">查看");

                url = "http://www.hzgjj.gov.cn:8080/WebAccounts/comPerInfo.do?" + postUrl;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetHangzhou_yuhang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                string postUrl = string.Empty;
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                url = "http://www.yhgjj.gov.cn/gjjcx/Login.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];

                url = "http://www.yhgjj.gov.cn/gjjcx/comm/Image.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.yhgjj.gov.cn/gjjcx/Login.aspx";
                string TxtName = "330184198702064515";
                string password = "064515";
                postdata = String.Format("__VIEWSTATE={3}&__EVENTVALIDATION={4}&sessionUnUse=0&TxtName={0}&TxtPassword={1}&SmsCode=&TxtVerifyCode={2}&ImageButton1=%E7%99%BB+%E5%BD%95&HidUrl=", TxtName, password, vercode, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.yhgjj.gov.cn/gjjcx/gjj/gjjzhcx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//div[@id='objWebDataWindowControl1_datawindow']", "");
                results = HtmlParser.GetResultFromParser(results[0], "//input", "value");
                foreach (string item in results)
                {
                    Console.WriteLine(item);
                }

                url = "http://www.yhgjj.gov.cn/gjjcx/gjj/gjjmxcx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetHangzhou_xiaoshan()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            start:
                string postUrl = string.Empty;
                url = "http://183.129.195.94:9001/Controller/Image.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://183.129.195.94:9001/Controller/login.ashx";
                string name = "360502197409285018";
                string password = "285018";
                postdata = String.Format("name={0}&password={1}&yzm={2}&logintype=0&usertype=10&dn=&signdata=&1=y", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string success = jsonParser.GetResultFromParser(httpRes.Html, "success");
                if (success.ToLower() == "false")
                {
                    goto start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://183.129.195.94:9001/Controller/GR/gjcx/dwjbxx.ashx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://183.129.195.94:9001/Controller/GR/gjcx/gjjzlcx.ashx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://183.129.195.94:9001/Controller/GR/gjcx/gjjmx.ashx?transDateBegin=2005-01-01&transDateEnd=2014-12-31&page=1&rows=30&sort=mxbc&order=desc";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://183.129.195.94:9001/Controller/GR/gjcx/gjcx.ashx";
                postdata = "m=grjcmx&start=2005-01-01&end=2014-12-31&page=1&rows=30&sort=csrq&order=desc";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void GetHangzhou_fuyang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                string postUrl = string.Empty;
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                url = "http://gjjsearch.fuyang.gov.cn/Login.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];

            Lable_vercode:
                url = "http://gjjsearch.fuyang.gov.cn/ashx/ValidateCode.ashx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.NumberAndLower);

                //url = "http://gjjsearch.fuyang.gov.cn/ashx/CheckLoginCode.ashx";
                //postdata = "code=" + vercode;
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "post",
                //    Postdata=postdata,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //if (httpRes.Html == "no")
                //{
                //    goto Lable_vercode;
                //}
                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://gjjsearch.fuyang.gov.cn/Login.aspx";
                string name = "330123196809064812";
                string password = "940802";
                postdata = String.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={3}&__EVENTVALIDATION={4}&login=geren&txtSId={0}&txtPwd={1}&txtCode={2}&btnLogin=%E7%99%BB+%E5%BD%95", name, password, vercode, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetHangzhou_tonglu()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://xxgk.tonglu.gov.cn:8081/tonglu/query/gjj_result.jsp";
                postdata = String.Format("sfz=330122198203160319&gjjzh=");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetHangzhou_chunan()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://www.cagjj.cn/old/yg2.asp";
                postdata = String.Format("D1=%B9%AB%BB%FD%BD%F0%D3%E0%B6%EE&T1=330127197507221923&B1=%B2%E9%D1%AF");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetHangzhou_jiande()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://218.108.68.162/zxcx.aspx?userid=012150008029&sfz=610326197911251026&mm=251026";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetZhejiang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            Lable_Start:
                url = "http://web.zjgjj.com:7001/szhfsnet/codeMaker";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://web.zjgjj.com:7001/szhfsnet/pages/login.do";
                string name = HttpUtility.UrlEncode("虞跃虹");
                string password = "123456";
                postdata = String.Format("action=login&ccust_no=&oper_no={0}&password={1}&ukey_code=&validate_code={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("验证码不正确") != -1)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://web.zjgjj.com:7001/szhfsnet/pages/net/per/perBaseInfo.do?no=8020001";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://web.zjgjj.com:7001/szhfsnet/af/t_query/perinfo.do?acct_no=10100034009111&query_flag=1";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 无锡（验证码破解：N）
        /// </summary>
        public static void GetWuxi()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            Lable_Start:
                url = "http://www.wxgjj.com.cn/jcaptcha";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.wxgjj.com.cn/logon.do";
                string name = HttpUtility.UrlEncode("081792205");
                string password = "801127";
                postdata = String.Format("logontype=2&loginname={0}&type=person&password={1}&_login_checkcode={2}&x=36&y=9", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("登录错误") != -1)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.wxgjj.com.cn/zg_info.do";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://www.wxgjj.com.cn/mx_info.do";
                postdata = "zjlx=1&hjstatus=&submit=%B2%E9++%D1%AF";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.wxgjj.com.cn/mx_info.do";
                postdata = "zjlx=2&hjstatus=&submit=%B2%E9++%D1%AF";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 南京（验证码破解：Y）
        /// </summary>
        public static void GetNanjing()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            Lable_Start:
                url = "http://www.njgjj.com/vericode.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.NumberAndUpper);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.njgjj.com/per.login";
                string name = HttpUtility.UrlEncode("320112198405010015");
                string password = "970800";
                postdata = String.Format("certinum={0}&perpwd={1}&vericode={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//div[@class='WTLoginError']/ul/li[@class='text']", "text");

                if (httpRes.Html.IndexOf("验证码错误") != -1)
                {
                    goto Lable_Start;
                }
                //cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.njgjj.com/init.summer?_PROCID=80000003";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string jsonStr = CommonFun.GetMidStr(httpRes.Html, "poolSelect = {", "};");
                postdata = "{" + jsonStr + ",'accnum':'3201000267519708','prodcode':'1'}";
                JObject o = JObject.Parse(postdata);

                postdata = "";
                foreach (KeyValuePair<string, JToken> item in o)
                {
                    postdata+=HttpUtility.UrlEncode(item.Key)+"="+HttpUtility.UrlEncode(item.Value.ToString())+"&";
                }
                postdata += "accname="+HttpUtility.UrlEncode("杨光");
                url = "http://www.njgjj.com/command.summer?uuid=1423100273767";



                //string result = "";
                //WebClient myClient = new WebClient();
                //myClient.Headers.Add("Accept: */*");
                //myClient.Headers.Add("User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; .NET4.0E; .NET4.0C; InfoPath.2; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; SE 2.X MetaSr 1.0)");
                //myClient.Headers.Add("Accept-Language: zh-cn");
                //myClient.Headers.Add("Content-Type: application/x-www-form-urlencoded; charset=utf-8");
                //myClient.Headers.Add("Accept-Encoding: gzip, deflate");
                //myClient.Headers.Add("Cache-Control: no-cache");
                //myClient.Headers.Add("Cookie", httpRes.CookieCollection[0].Name + "=" + httpRes.CookieCollection[0].Value);
                //myClient.Encoding = Encoding.GetEncoding("gbk");
                //result = myClient.UploadString(url, postdata);

                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    //Referer = "http://www.njgjj.com/init.summer?_PROCID=80000003",
                    CookieCollection = cookies,
                    Encoding=Encoding.GetEncoding("gbk"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpitem.Header.Add("Accept-Language","zh-cn,zh");
                httpRes = http.GetHtml(httpitem);
                jsonStr = jsonParser.GetResultFromParser(httpRes.Html, "data");

                url = "http://www.njgjj.com/init.summer?_PROCID=70000002";

                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                jsonStr = CommonFun.GetMidStr(httpRes.Html, "poolSelect = {", "};");
                postdata = "{" + jsonStr + "}";
                o = JObject.Parse(postdata);

                postdata = "";
                foreach (KeyValuePair<string, JToken> item in o)
                {
                    postdata += item.Key.ToUrlEncode() + "=" + item.Value.ToString().ToUrlEncode() + "&";
                }
                string DATAlISTGHOST = HtmlParser.GetResultFromParser(httpRes.Html, "//textarea[@name='DATAlISTGHOST']", "",true)[0];
                string _DATAPOOL_ = HtmlParser.GetResultFromParser(httpRes.Html, "//textarea[@name='_DATAPOOL_']", "", true)[0];


                postdata += "&begdate=2004-02-05";
                postdata += "&enddate=2015-02-05";
                postdata += "&accname=" + "杨光".ToUrlEncode();
                postdata += "&accnum=3201000267519708";
                url = "http://www.njgjj.com/command.summer";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                //postdata = "dynamicTable_id=datalist2&dynamicTable_currentPage=0&dynamicTable_pageSize=10&dynamicTable_nextPage=1&dynamicTable_page=%2Fydpx%2F70000002%2F700002_01.ydpx&dynamicTable_paging=true&dynamicTable_configSqlCheck=0&errorFilter=1%3D1&begdate=2004-02-05&enddate=2015-02-05&accnum=3201000267519708&accname=%E6%9D%A8%E5%85%89&_APPLY=0&_CHANNEL=1&_PROCID=70000002&DATAlISTGHOST=" + CommonFun.UrlEncodeToUpper(DATAlISTGHOST) + "&_DATAPOOL_=" + CommonFun.UrlEncodeToUpper(_DATAPOOL_);
                //postdata = "dynamicTable_id=datalist2&dynamicTable_currentPage=0&dynamicTable_pageSize=10&dynamicTable_nextPage=1&dynamicTable_page=%2Fydpx%2F70000002%2F700002_01.ydpx&dynamicTable_paging=true&dynamicTable_configSqlCheck=0&errorFilter=1%3D1&begdate=2005-02-05&enddate=2015-02-05&accnum=3201000267519708&accname=%E6%9D%A8%E5%85%89&_APPLY=0&_CHANNEL=1&_PROCID=70000002&DATAlISTGHOST=rO0ABXNyABNqYXZhLnV0aWwuQXJyYXlMaXN0eIHSHZnHYZ0DAAFJAARzaXpleHAAAAABdwQAAAAK%0Ac3IAJWNvbS55ZHlkLm5icC5lbmdpbmUucHViLkRhdGFMaXN0R2hvc3RCsjhA3j2pwwIAA0wAAmRz%0AdAASTGphdmEvbGFuZy9TdHJpbmc7TAAEbmFtZXEAfgADTAADc3FscQB%2BAAN4cHQAEHdvcmtmbG93%0ALmNmZy54bWx0AAlkYXRhbGlzdDJ0ALxzZWxlY3QgaW5zdGFuY2UsIHVuaXRhY2NudW0xLCB1bml0%0AYWNjbmFtZSwgYWNjbnVtMSwgYWNjbmFtZTEsIGNlcnRpbnVtLCB0cmFuc2RhdGUsIHJlYXNvbiAs%0AIGRwYnVzaXR5cGUsIGJhc2VudW0sIHBheXZvdWFtdCwgc2Vxbm8gZnJvbSBkcDA3NyB3aGVyZSBp%0AbnN0YW5jZSA9LTEyNjIzMSBvcmRlciBieSB0cmFuc2RhdGUgZGVzY3g%3D&_DATAPOOL_=rO0ABXNyABZjb20ueWR5ZC5wb29sLkRhdGFQb29sp4pd0OzirDkCAAZMAAdTWVNEQVRFdAASTGph%0AdmEvbGFuZy9TdHJpbmc7TAAGU1lTREFZcQB%2BAAFMAAhTWVNNT05USHEAfgABTAAHU1lTVElNRXEA%0AfgABTAAHU1lTV0VFS3EAfgABTAAHU1lTWUVBUnEAfgABeHIAEWphdmEudXRpbC5IYXNoTWFwBQfa%0AwcMWYNEDAAJGAApsb2FkRmFjdG9ySQAJdGhyZXNob2xkeHA%2FQAAAAAAAGHcIAAAAIAAAABV0AAdf%0AQUNDTlVNdAAQMzIwMTAwMDI2NzUxOTcwOHQAA19SV3QAAXd0AAtfVU5JVEFDQ05VTXB0AAdfUEFH%0ARUlEdAAFc3RlcDF0AANfSVNzcgAOamF2YS5sYW5nLkxvbmc7i%2BSQzI8j3wIAAUoABXZhbHVleHIA%0AEGphdmEubGFuZy5OdW1iZXKGrJUdC5TgiwIAAHhw%2F%2F%2F%2F%2F%2F%2F%2BEul0AAxfVU5JVEFDQ05BTUV0ACTl%0AjZfkuqzkuJzlsbHlhazkuqTlrqLov5DmnInpmZDlhazlj7h0AAZfTE9HSVB0ABEyMDE1MDIwNTE3%0AMjcwNjk1MnQACF9BQ0NOQU1FdAAG5p2o5YWJdAAJaXNTYW1lUGVydAAFZmFsc2V0AAdfUFJPQ0lE%0AdAAINzAwMDAwMDJ0AAtfU0VORE9QRVJJRHQAEjMyMDExMjE5ODQwNTAxMDAxNXQAEF9ERVBVVFlJ%0ARENBUkROVU10ABIzMjAxMTIxOTg0MDUwMTAwMTV0AAlfU0VORFRJTUV0AAoyMDE1LTAyLTA1dAAL%0AX0JSQU5DSEtJTkR0AAEwdAAJX1NFTkREQVRFdAAKMjAxNS0wMi0wNXQAE0NVUlJFTlRfU1lTVEVN%0AX0RBVEVxAH4AInQABV9UWVBFdAAEaW5pdHQAB19JU0NST1BxAH4AIHQACV9QT1JDTkFNRXQAGOS4%0AquS6uuaYjue7huS%2FoeaBr%2BafpeivonQAB19VU0JLRVlwdAAIX1dJVEhLRVlxAH4AIHh0AAhAU3lz%0ARGF0ZXQAB0BTeXNEYXl0AAlAU3lzTW9udGh0AAhAU3lzVGltZXQACEBTeXNXZWVrdAAIQFN5c1ll%0AYXI%3D";
                url = "http://www.njgjj.com/dynamictable";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                string pageNum = string.Empty;
                string pageCount = string.Empty;
                do
                {
                    if (!String.IsNullOrEmpty(pageNum))
                    {
                        pageNum = (int.Parse(pageNum) + 1).ToString();
                    }
                    url = "http://www.njgjj.com/skywcm/query/gjj_query/gr/grmxcx.jsp";
                    postdata = string.Format("biz_action=&startTime={0}&endTime={1}&pageNum={2}&pageCount={3}", pageNum, pageCount);
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);
                    pageNum = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='pageNum']", "value")[0];
                    pageCount = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='pageCount']", "value")[0];
                }
                while (pageNum != pageCount);
                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 合肥（验证码破解：Y）
        /// </summary>
        public static void GetHefei()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            Lable_Start:
                url = "http://220.178.98.86/hfgjj/code.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://220.178.98.86/hfgjj/jsp/web/public/search/grloginAct.jsp";
                string name = HttpUtility.UrlEncode("342427199005054415");
                string password = "123456";
                postdata = String.Format("lb=b&hm={0}&mm={1}&yzm={2}&imageField.x=48&imageField.y=8", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("登陆码输入不正确") != -1)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://220.178.98.86/hfgjj/jsp/web/public/search/grCenter.jsp?rnd=1417413440497";
                postdata = "url=2&dkzh=";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://220.178.98.86/hfgjj/jsp/web/public/search/grCenter.jsp?rnd=1417413440497";
                postdata = "url=3_1&dkzh=";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 青岛（验证码破解：Y）
        /// </summary>
        public static void GetQingdao()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            start:
                string postUrl = string.Empty;
                url = "http://219.147.7.52:89/Controller/Image.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://219.147.7.52:89/Controller/login.ashx";
                string name = "370203198402015919";
                string password = "456456";
                postdata = String.Format("name={0}&password={1}&yzm={2}&logintype=0&usertype=10&dn=&signdata=&1=y", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string success = jsonParser.GetResultFromParser(httpRes.Html, "success");
                if (success.ToLower() == "false")
                {
                    goto start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://219.147.7.52:89/Controller/GR/gjcx/dwjbxx.ashx?dt=1417415847293";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://219.147.7.52:89/Controller/GR/gjcx/gjjzlcx.ashx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://219.147.7.52:89/Controller/GR/gjcx/gjjmx.ashx?transDateBegin=2005-01-01&transDateEnd=2014-12-31&page=1&rows=30&sort=mxbc&order=desc";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://219.147.7.52:89/Controller/GR/gjcx/gjcx.ashx";
                postdata = "m=grjcmx&start=2005-01-01&end=2014-12-31&page=1&rows=30&sort=csrq&order=desc";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 石家庄（验证码破解：Y）
        /// </summary>
        public static void GetShijiazhuang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            start:
                string postUrl = string.Empty;
                url = "http://110.249.253.234/webQ/netQuery/include/ranCode.jsp?tab=yzm";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://110.249.253.234/webQ/netQuery/queryPage/webQ.login";

                string name = "130124197910040157";
                string password = "666666";
                string accname = HttpUtility.UrlEncode("张庆肖");
                postdata = String.Format("trancode=700001&type=1&certinum={0}&accnum=&cardno=&accname={3}&pwd={1}&tYzm={2}", name, password, vercode, accname);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string accnum = CommonFun.GetMidStrByRegex(httpRes.Html, "accnum='", "'");
                string loancontrnum = CommonFun.GetMidStrByRegex(httpRes.Html, "loancontrnum='", "'");

                url = "http://110.249.253.234/webQ/netQuery/include/send.jsp?task=grxx&trancode=700002";
                postdata = string.Format("loancontrnum={0}&accnum={1}", loancontrnum, accnum);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://110.249.253.234/webQ/netQuery/include/send.jsp?task=grmx&trancode=700003&accnum1=" + accnum;
                postdata = "begdate=1995-01-01&enddate=2014-12-31";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 重庆（验证码破解：Y）
        /// </summary>
        public static void GetChongqing()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                string postUrl = string.Empty;
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];

            start:
                url = "http://www.cqgjj.cn/Code.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.NumberAndUpper);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr";
                string name = "dzx123456";
                string password = "dzx060017";
                postdata = String.Format("__VIEWSTATE={3}&__EVENTVALIDATION={4}&txt_loginname={0}&txt_pwd={1}&txt_code={2}&but_send=", name, password, vercode, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("公积金用户登录成功") == -1)
                {
                    goto start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.cqgjj.cn/Member/gr/gjjyecx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.cqgjj.cn/Member/gr/gjjmxcx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void GetXian()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = string.Format("http://124.114.130.149:7001/wscx/zfbzgl/gjjxxcx/gjjxx_cx.jsp?sfzh=511028198207177721&zgxm=%C0%BC%D4%C6%B7%EF&zgzh=1365654&dwbm=0008578&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get"
                };
                httpRes = http.GetHtml(httpitem);

                url = string.Format("http://124.114.130.149:7001/wscx/zfbzgl/gjjmxcx/gjjmx_cx.jsp?sfzh=511028198207177721&zgxm=%C0%BC%D4%C6%B7%EF&zgzh=1365654&dwbm=0008578&cxyd=%B5%B1%C7%B0%C4%EA%B6%C8");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get"
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetShanxi()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                string postUrl = string.Empty;
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;
                url = "http://www.sxgjj.com/seach/Sigin.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];

            start:
                string vercode = string.Empty;

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.sxgjj.com/seach/Sigin.aspx";
                string name = "610112195902022514";
                string password = "123123";
                postdata = String.Format("__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={3}&__EVENTVALIDATION={4}&txtzhanghao={0}&txtpwd={1}&Button1=%C8%B7%C8%CF", name, password, vercode, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.sxgjj.com/seach/xinxi.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.sxgjj.com/seach/Gerendnmx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.sxgjj.com/seach/Gerensnmx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 武汉（验证码破解：Y）
        /// </summary>
        public static void GetWuhan()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
            start:
                string postUrl = string.Empty;
                url = "http://www.whgjj.org.cn/image.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.whgjj.org.cn/gjjlogin.jspx";

                string name = "wanghuiming123";
                string password = "whm123";
                postdata = String.Format("username={0}&userpwd={1}&randCodeFont={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string id = httpRes.Html;
                if (id == "0" || id == "4" || id == "5")
                {
                    goto start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.whgjj.org.cn/viewByid.jspx";
                postdata = string.Format("acctNo={0}", id);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://www.whgjj.org.cn/viewByidmore.jspx";
                postdata = string.Format("id={0}", id);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void GetXiamen()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = string.Format("http://222.76.242.141:8888/login.shtml");
                string name = "35220119870814361X";
                string password = "qian814";
                postdata = string.Format("username={0}&password={1}&securityCode2=&securityCode=", name, password);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://222.76.242.141:8888/queryZgzh.shtml";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://222.76.242.141:8888/queryPersonXx.shtml";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://222.76.242.141:8888/queryGrzhxxJson.shtml";
                postdata = "custAcct=10034320244&startDate=20051201&endDate=20141201";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 北京（验证码破解：Y）
        /// </summary>
        public static void GetBeijing()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://www.bjgjj.gov.cn/wsyw/wscx/gjjcx-login.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

            start:
                string postUrl = string.Empty;
                url = "http://www.bjgjj.gov.cn/wsyw/servlet/PicCheckCode1";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.NumberAndLower);

                url = "http://www.bjgjj.gov.cn/wsyw/wscx/asdwqnasmdnams.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    CookieCollection = cookies,
                    Referer = "http://www.bjgjj.gov.cn/wsyw/wscx/gjjcx-login.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();

                string Key1 = "pdcss123";
                string Key2 = "css11q1a";
                string Key3 = "co1qacq11";

                string username = "6222620910004952226";
                string password = "whm123";
                url = "http://www.bjgjj.gov.cn/wsyw/wscx/gjjcx-choice.jsp";
                postdata = String.Format("lb=5&bh={0}&mm={1}&gjjcxjjmyhpppp={2}&lk={3}", MultiKeyDES.EncryptDES(username, Key1, Key2, Key3), MultiKeyDES.EncryptDES(password, Key1, Key2, Key3), vercode, "1");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string id = httpRes.Html;
                if (id == "0" || id == "4" || id == "5")
                {
                    goto start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.whgjj.org.cn/viewByid.jspx";
                postdata = string.Format("acctNo={0}", id);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://www.whgjj.org.cn/viewByidmore.jspx";
                postdata = string.Format("id={0}", id);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                //Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetQinming()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                //url = string.Format("http://localhost:10171/spiderservice/pbccrc/register/xml");
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "post",
                //    Postdata="{\"test\":123}",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //string readStr = FileOperateHelper.ReadFile("c://1.txt");
                //results = HtmlParser.GetResultFromParser(readStr,"//table/tr/td/table","");

                url = string.Format("http://localhost:10171/spiderservice/vercode/tesseract");
                url = string.Format("http://10.100.12.18:7000/spiderservice/vercode/tesseract");
                byte[] bytes = FileToStream("c:\\validateImg.jpg");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    PostdataByte = bytes,
                    PostDataType = PostDataType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static byte[] FileToStream(string fileName)
        {
            // 打开文件
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            return bytes;
        }
    }
}
