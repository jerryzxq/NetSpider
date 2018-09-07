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
    public class SociaSecurityTool
    {
        static IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        static IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        static IPluginSecurityCode secodeParser = PluginServiceManager.GetSecurityCodeParserPlugin();
        static HttpHelper http = new HttpHelper();
        static CookieCollection cookies = new CookieCollection();
        /// <summary>
        /// 成都（验证码：Y）
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

                url = "http://www.cdhrss.gov.cn/images/image.jsp";
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
                url = "http://www.cdhrss.gov.cn/LoginSIAction.do";
                string siusername = "011522782";
                string sipassword = "74543567";
                postdata = String.Format("siusername={0}&sipassword={1}&randCode={2}", siusername, sipassword, vercode);
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

                url = "http://www.cdhrss.gov.cn/QueryListAction.do";
                string year = "2014";
                postdata = String.Format("p_year={0}&x=23&y=14&bizID=BC.001.000.001", year);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
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
        public static void GetChengdu1()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "https://gr.cdhrss.gov.cn:442/cdwsjb/login.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "https://gr.cdhrss.gov.cn:442/cdwsjb/CaptchaImg";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    //SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultCookieType = ResultCookieType.CookieCollection
                }; 
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCode(httpRes.ResultByte);

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "https://gr.cdhrss.gov.cn:442/cdwsjb/j_spring_security_check";
                string siusername = "tangqiguo123";
                string sipassword = "tangqiguo456";
                postdata = String.Format("j_username={0}&j_password={1}&checkCode={2}", siusername, sipassword, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "https://gr.cdhrss.gov.cn:442/cdwsjb/login.jsp",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "https://gr.cdhrss.gov.cn:442/cdwsjb/runqian/reportJsp/showReport.jsp?raq=gt0200.raq";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    //SecurityProtocolType = SecurityProtocolType.Ssl3,
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
        /// 成都（验证码：Y）
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

                url = "http://221.215.38.136/grcx/common/checkcode.do";
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
                url = "http://221.215.38.136/grcx/work/login.do?method=login";
                string username = "230122197204123722";
                string password = "123321";
                postdata = String.Format("method=login&domainId=1&groupid=-95&loginName={0}&loginName18=&password={1}&checkCode={2}", username, password, vercode);
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

                int currpage = 1;
                do
                {
                    url = "http://221.215.38.136/grcx/work/m01/f1203/oldQuery.action?page_oldQuery=" + currpage;
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://221.215.38.136/grcx/work/m01/f1204/medicalQuery.action?page_medicalQuery=" + currpage;
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://221.215.38.136/grcx/work/m01/f1205/unemployQuery.action?page_unemployQuery=" + currpage;
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        Method = "GET",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);
                    currpage++;

                } while (currpage < 3);

                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 苏州（验证码：Y）
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
            Lable_Start:
                url = "http://www.szsbzx.net.cn:9900/web/website/rand.action";
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
                url = "http://www.szsbzx.net.cn:9900/web/website/indexProcess?frameControlSubmitFunction=checkLogin";
                string name = "0500476063";
                string name1 = "321322198308151638";
                string password = "74543567";
                postdata = String.Format("grbh={0}&sfzh={1}&yzcode={2}", name, name1, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string errormsg = jsonParser.GetResultFromParser(httpRes.Html, "errormsg");
                if (errormsg != "")
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                postdata = String.Format("xz=ybdz&pageIndex=1&pageCount=20");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                string pageIndex = "1";
                string pageCount = string.Empty;
                do
                {

                    url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                    postdata = string.Format("xz=qyylmx&pageIndex={0}&pageCount=20", pageIndex, pageCount);
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);
                    pageIndex = jsonParser.GetResultFromParser(httpRes.Html, "pageIndex"); ;
                    pageCount = CommonFun.GetMidStr(httpRes.Html, "第" + pageIndex + "\\/", "页");
                }
                while (pageIndex != pageCount);

                pageIndex = "1";
                pageCount = string.Empty;
                do
                {

                    url = "http://www.szsbzx.net.cn:9900/web/website/personQuery/personQueryAction?frameControlSubmitFunction=getPagesAjax";
                    postdata = string.Format("xz=ylbx&pageIndex={0}&pageCount=20", pageIndex, pageCount);
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);
                    pageIndex = jsonParser.GetResultFromParser(httpRes.Html, "pageIndex"); ;
                    pageCount = CommonFun.GetMidStr(httpRes.Html, "第" + pageIndex + "\\/", "页");
                }
                while (pageIndex != pageCount);
                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void GetSuzhou_taicang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {

                url = "http://www.tchrss.gov.cn/tclss/wsbsdt/logincheck.jsp";
                string name = "0500837091";
                string name1 = "32038219851204312X";
                string password = "837091";
                postdata = String.Format("type=2&EAC012={0}&password={2}&AAC002={1}&Submit=%C8%B7+%B6%A8", name, name1, password);
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

                url = "http://www.tchrss.gov.cn/tclss/wsbsdt/view_grjcxx.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.tchrss.gov.cn/tclss/wsbsdt/view_ylbx.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.tchrss.gov.cn/tclss/wsbsdt/view_ylbxgrzh.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.tchrss.gov.cn/tclss/wsbsdt/view_grjaofei.jsp";
                postdata = "AKA101=2014&Submit=%C8%B7+%B6%A8";
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
        public static void GetSuzhou_wujiang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {

                url = "http://www.wjrs.gov.cn:8080/wscx/index.jsp";
                string name = "G26777";
                string name1 = "321322198612217216";
                postdata = String.Format("akc020={0}&aac002={1}&submit=%E6%9F%A5%E8%AF%A2", name, name1);
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    Referer = "	http://www.wjrs.gov.cn:8080/wscx/index.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpitem.Header.Add("Accept-Language", "zh-cn,zh;");
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                Console.WriteLine(httpRes.Html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 常熟（验证码：N）
        /// </summary>
        public static void GetSuzhou_changshu()
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

            start:
                url = "http://www.cssbzx.com/cssbfw/Register/loginUser.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                url = HtmlParser.GetResultFromParser(httpRes.Html, "//span[@id='Span_img']", "")[0];
                url = HtmlParser.GetResultFromParser(url, "//img", "src")[0];

                url = "http://www.cssbzx.com" + url;
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

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://www.cssbzx.com/cssbfw/Register/loginUser.aspx";
                string name = "yangermei";
                string password = "5325738";
                postdata = String.Format("__VIEWSTATE={3}&txtKey=&txtUserName={0}&txtPwd={1}&txtYzm={2}&btnLogin=%E7%99%BB%E5%BD%95", name, password, vercode, HttpUtility.UrlEncode(VIEWSTATE));
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("登录成功") == -1)
                {
                    goto start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.cssbzx.com/cssbfw/wsfw/search.aspx?type=1";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.cssbzx.com/cssbfw/wsfw/search.aspx?type=2";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                string EVENTTARGET = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTTARGET']", "value")[0];
                string EVENTARGUMENT = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTARGUMENT']", "value")[0];
                string LASTFOCUS = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__LASTFOCUS']", "value")[0];


                postdata = String.Format("__VIEWSTATE={0}&__EVENTTARGET={1}&__EVENTARGUMENT={2}&__LASTFOCUS={3}&txtKey=&myhs=100&yh=1", HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTTARGET), HttpUtility.UrlEncode(EVENTARGUMENT), HttpUtility.UrlEncode(LASTFOCUS));
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.cssbzx.com/cssbfw/wsfw/search.aspx?type=31";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.cssbzx.com/cssbfw/wsfw/search.aspx?type=32";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
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
        public static void GetSuzhou_zhangjiagang()
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


                url = "http://www.12333zjg.gov.cn/Searchzjgsb/SearchForm1.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];

            start:
                url = "http://www.12333zjg.gov.cn/Searchzjgsb/VerifyCode.aspx";
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

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://www.12333zjg.gov.cn/Searchzjgsb/SearchForm1.aspx";
                string name = "30494208";
                string password = "30494208";
                postdata = String.Format("__VIEWSTATE={3}&__EVENTVALIDATION={4}&ctl00%24ContentPlaceHolder1%24RadioButtonList1=%E4%B8%AA%E4%BA%BA%E5%8F%82%E4%BF%9D%E7%BC%96%E5%8F%B7&ctl00%24ContentPlaceHolder1%24TextBox4={0}&ctl00%24ContentPlaceHolder1%24TextBox2={1}&ctl00%24ContentPlaceHolder1%24TextBox3={2}&ctl00%24ContentPlaceHolder1%24Button1=%E7%99%BB%E5%BD%95", name, password, vercode, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                //if (httpRes.Html.IndexOf("退出系统") != -1)
                //{
                //    goto start;
                //}
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://www.12333zjg.gov.cn/Searchzjgsb/SearchForm2.aspx";
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

                string year = "2014";
                string month = "12";
                url = "http://www.12333zjg.gov.cn/Searchzjgsb/SearchForm1.aspx";
                postdata = String.Format("__VIEWSTATE={4}&__EVENTVALIDATION={5}&ctl00%24ContentPlaceHolder1%24DropDownList2={2}&ctl00%24ContentPlaceHolder1%24DropDownList3={3}&ctl00%24ContentPlaceHolder1%24Button2=%E6%9F%A5++%E8%AF%A2&ctl00%24ContentPlaceHolder1%24TextBox4=&ctl00%24ContentPlaceHolder1%24TextBox1={0}&ctl00%24ContentPlaceHolder1%24TextBox2={1}&ctl00%24ContentPlaceHolder1%24TextBox3=", name, password, year, month, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];

                url = "http://www.12333zjg.gov.cn/Searchzjgsb/SearchForm1.aspx";
                postdata = String.Format("__VIEWSTATE={2}&__EVENTVALIDATION={3}&ctl00%24ContentPlaceHolder1%24Button2=%E6%9F%A5++%E8%AF%A2&ctl00%24ContentPlaceHolder1%24TextBox4=&ctl00%24ContentPlaceHolder1%24TextBox1={0}&ctl00%24ContentPlaceHolder1%24TextBox2={1}&ctl00%24ContentPlaceHolder1%24TextBox3=", name, password, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION));
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

                url = "http://wsbs.zjhz.hrss.gov.cn/loginvalidate.html?logintype=2";
                string name = HttpUtility.UrlEncode("320324198312291897@hz.cn");
                string password = "123456";
                postdata = String.Format("type=01&persontype=01&account={0}&password={1}", name, password);
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpitem.Header.Add("Accept-Language", "zh-cn,zh;");
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://wsbs.zjhz.hrss.gov.cn/person/personInfo/index.html";
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.zjhz.hrss.gov.cn/person/ylgrzhQuery/index.html";
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.zjhz.hrss.gov.cn/person/ylgrzhQuery/index.html";
                postdata = "pageNo=2&message=";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.zjhz.hrss.gov.cn/person/web_ybxfxx_query/ybxfxx_query.html";
                postdata = "akc001_start=20140101&akc001_end=20141231&pageNo=1";
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

                url = "http://oa.hzyhldbz.gov.cn/oa/IndiLogin.aspx";
                string name = "342425199308174032";
                string password = "123456";
                postdata = String.Format("idcard={0}&password={1}&x=199&y=31", name, password);
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

                url = String.Format("http://oa.hzyhldbz.gov.cn/oa/duo.aspx?idc={0}&from=", name);
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@type='button']", "onclick");
                string indi = string.Empty;
                foreach (string item in results)
                {

                    indi = CommonFun.GetMidStr(item, "myform.indi.value='", "'");
                    url = String.Format("http://oa.hzyhldbz.gov.cn/oa/duo.aspx?idc={0}&from=", name);
                    postdata = String.Format("idc={0}&indi={1}", name, indi);
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

                    url = "http://oa.hzyhldbz.gov.cn/oa/User/UserInfo.aspx";
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://oa.hzyhldbz.gov.cn/oa/User/UserCBINFO.aspx";
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://oa.hzyhldbz.gov.cn/oa/User/UserWX.aspx";
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://oa.hzyhldbz.gov.cn/oa/User/ProvideHr.aspx";
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://oa.hzyhldbz.gov.cn/oa/User/UserJBYLBX.aspx";
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);

                    url = "http://oa.hzyhldbz.gov.cn/oa/User/Medical.aspx";
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);
                    Console.WriteLine(httpRes.Html);
                }
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
                url = "http://www.ldbz.xs.zj.cn/xslss/wsbs/logincheck.jsp";
                string name = "330824198209243714";
                string password = "243714";
                postdata = String.Format("errors=&select=1&login.x=29&login.y=22&aac00z={0}&password={1}&num12=7313&num=7313", name, password);
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

                url = String.Format("http://www.ldbz.xs.zj.cn/xslss/wsbs/ybxx.jsp");
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = String.Format("http://www.ldbz.xs.zj.cn/xslss/wsbs/jbylbxxx.jsp");
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//td[@id='hdjl']", "");

                foreach (string item in results)
                {
                    url = HtmlParser.GetResultFromParser(item, "//a", "href")[0];
                    url = String.Format("http://www.ldbz.xs.zj.cn/xslss/wsbs/" + url);
                    httpitem = new HttpItem()
                    {
                        URL = url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpRes = http.GetHtml(httpitem);
                }
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
                url = "http://218.75.78.86/pages/wsbs/company/f80020201/f80020201_input.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://218.75.78.86/apply/f80020201.action";
                string idcard = "330501198107097313";
                string name = HttpUtility.UrlEncode("叶健娥");
                postdata = String.Format("aab001=&doquery_ec2=true&aae135={1}&aac003={0}", name, idcard);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
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

        /// <summary>
        /// 无锡（验证码：Y）
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
                url = "http://ggfw.wxhrss.gov.cn/captcha.svl";
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
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Character);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://ggfw.wxhrss.gov.cn/personloginvalidate.html";
                string name = "320203198011270016";
                string password = "123456";
                postdata = String.Format("account={0}&password={1}&type=1&captcha={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html != "['success']")
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://ggfw.wxhrss.gov.cn/person/personBaseInfo.html";
                httpitem = new HttpItem()
                {
                    URL = url,
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
        /// <summary>
        /// 无锡江阴（验证码：N）
        /// </summary>
        public static void GetWuxi_jiangyin()
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
                string EVENTTARGET = string.Empty;
                string EVENTARGUMENT = string.Empty;

            start:
                url = "http://58.214.13.116/wsbsdt/frontdesk/GeRen/grywcx.aspx";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                VIEWSTATE = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__VIEWSTATE']", "value")[0];
                EVENTVALIDATION = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTVALIDATION']", "value")[0];
                EVENTTARGET = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTTARGET']", "value")[0];
                EVENTARGUMENT = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='__EVENTARGUMENT']", "value")[0];

                url = "http://58.214.13.116/wsbsdt/Backstage/ValidateCode.aspx";
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

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://58.214.13.116/wsbsdt/frontdesk/GeRen/grywcx.aspx";
                string name = "8102059684";
                string name1 = "342225198103144010";
                string password = "123456789";
                postdata = String.Format("__EVENTVALIDATION={5}&__EVENTTARGET={6}&__EVENTARGUMENT={7}&__VIEWSTATE={4}&Tbgrbh={0}&Tbsfid={1}&Tbpassw={2}&TxtCheckNum={3}&Btload=%E7%99%BB++%E5%BD%95&select=---+%E5%9B%BD%E5%86%85%E5%8A%B3%E5%8A%A8%E4%BF%9D%E9%9A%9C%E7%BD%91%E7%AB%99+---&select2=---+%E7%9C%81%E5%86%85%E5%8A%B3%E5%8A%A8%E4%BF%9D%E9%9A%9C%E7%BD%91%E7%AB%99+---&select3=-------+%E5%B8%82%E5%86%85%E7%BD%91%E7%AB%99+-------&select4=-----+%E5%9B%BD%E5%86%85%E5%85%B6%E4%BB%96%E7%BD%91%E7%AB%99+-----", name, name1, password, vercode, HttpUtility.UrlEncode(VIEWSTATE), HttpUtility.UrlEncode(EVENTVALIDATION), HttpUtility.UrlEncode(EVENTTARGET), HttpUtility.UrlEncode(EVENTARGUMENT));
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                //if (httpRes.Html.IndexOf("登录成功") == -1)
                //{
                //    goto start;
                //}
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "http://58.214.13.116/wsbsdt/frontdesk/GeRen/GR_CaiJxx.aspx";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://58.214.13.116/wsbsdt/frontdesk/GeRen/GrSbDz.aspx";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://58.214.13.116/wsbsdt/frontdesk/GeRen/GRYJSJ.aspx";
                httpitem = new HttpItem()
                {
                    Encoding = Encoding.UTF8,
                    URL = url,
                    Method = "get",
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
        /// <summary>
        /// 无锡宜兴
        /// </summary>
        public static void GetWuxi_yixing()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();

            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://61.160.91.3:7061/yxwsbs/j_unieap_security_check.do";
                string name = "330501198107097313";
                string password = "1000919898";
                postdata = String.Format("j_username={0}&j_password={1}&j_logintype=0&com_username={0}&com_password={1}&per_username={0}&per_password={1}", name, password);
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

                url = String.Format("http://61.160.91.3:7061/yxwsbs/czqs/pages/qs/si/personComplexInfo.jsp");
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = "",
                    Referer = "	http://61.160.91.3:7061/yxwsbs/czqs/controller.do?location=%2Fczqs%2Fpages%2Fqs%2Fsi%2FpersonComplexInfo.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = String.Format("http://61.160.91.3:7061/yxwsbs/ria_codelist.do?method=loadCodeLists&category=%7B%22AAE140%22%3A%7B%7D%2C%22AAC008%22%3A%7B%7D%7D");
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{},parameters:{}}}";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://61.160.91.3:7061/yxwsbs/czqs/pages/qs/si/personComplexInfo.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = String.Format("http://61.160.91.3:7061/yxwsbs/SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01");
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"basicInfoDs\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"basicInfoDs\",pageNumber:1,pageSize:0,recordCount:0,rowSetName:\"si.t_wsbs_ac01_ab01\",condition:\"t_wsbs_ac01.aac001 = '1000919898'\"}},parameters:{}}}";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "	http://61.160.91.3:7061/yxwsbs/czqs/pages/qs/si/personComplexInfo.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = String.Format("http://61.160.91.3:7061/yxwsbs/SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01");
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"basicInfoDs\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"basicInfoDs\",pageNumber:1,pageSize:0,recordCount:0,rowSetName:\"si.t_wsbs_ac02_ab01\",condition:\"t_wsbs_ac02.aac001 = '1000919898'\"}},parameters:{}}}";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = String.Format("http://61.160.91.3:7061/yxwsbs/SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01");
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"basicInfoDs\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"basicInfoDs\",pageNumber:1,pageSize:0,recordCount:0,rowSetName:\"si.t_wsbs_ac43_ab01\",condition:\"t_wsbs_ac43.aac001 = '1000919898'\"}},parameters:{}}}";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = String.Format("http://61.160.91.3:7061/yxwsbs/SiBusinessDelegateAction.do?method=submit&BUSINESS_REQUEST_ID=REQ-QS-A-001-01");
                postdata = "{header:{\"code\":0,\"message\":{\"title\":\"\",\"detail\":\"\"}},body:{dataStores:{\"basicInfoDs\":{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"basicInfoDs\",pageNumber:1,pageSize:0,recordCount:0,rowSetName:\"si.t_wsbs_kc51_ab01\",condition:\"t_wsbs_kc51.aac001 = '1000919898'\"}},parameters:{}}}";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
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
        /// <summary>
        /// 南京（验证码：Y）
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
                url = "http://wsbs.njhrss.gov.cn/NJLD/index.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

            Lable_Start:
                url = "http://wsbs.njhrss.gov.cn/NJLD/Images";
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
                url = "http://wsbs.njhrss.gov.cn/NJLD/LoginAction?act=CompanyLoginPerson";
                string name = "1883749006";
                string password = "48392468";
                postdata = String.Format("u={0}&p={1}&lx=1&key={2}&dl=", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("/NJLD/company/system/lesmain.jsp") == -1)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //url = "http://wsbs.njhrss.gov.cn/NJLD/ForWard?act=ForWardLes&id=180";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=1&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//table[@class='table1' and position() >1]/tr[position() >1]", "text", true);


                url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=2&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=3&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=4&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://wsbs.njhrss.gov.cn/NJLD/ZjGrJf?act=perform";
                postdata = "xz=5&hide=null&Submit=%E6%9F%A5%E8%AF%A2";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
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
        /// <summary>
        /// 南京通过验证码查询（验证码：Y）
        /// </summary>
        public static void GetNanjing_yzm()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://wsbs.njhrss.gov.cn/NJLD/web/cbzm/yzpt.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

            Lable_Start:
                url = "http://wsbs.njhrss.gov.cn/NJLD/Images";
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
                url = "http://wsbs.njhrss.gov.cn/NJLD/CbzmAction?act=grYz";
                string name = "136uuy2dea";
                string password = "48392468";
                postdata = String.Format("sjm={0}&yzm={1}&dl=", name, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("南京市社会保险个人参保缴费证明") == -1)
                {
                    goto Lable_Start;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

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
                url = "http://60.173.202.221/wssb/grlogo.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

            Lable_Start:
                url = "http://60.173.202.221/wssb/imagecoder";
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

                url = "http://60.173.202.221/wssb/admin/grpass.jsp";
                string name = "342622197510032954";
                string password = "A20029444";
                string name1 = HttpUtility.UrlEncode("荀忠", Encoding.GetEncoding("gb2312")).ToUpper();
                postdata = String.Format("xingm={2}&AtAction=Logon&sfz={0}&sbh={1}&verify={3}", name, password, name1);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "	http://60.173.202.221/wssb/grlogo.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://60.173.202.221/wssb/admin/000001/Grwxcb.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://60.173.202.221/wssb/admin/000001/Grzmmx.jsp?flag=T";
                httpitem = new HttpItem()
                {
                    URL = url,
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
        public static void GetShenyang()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://60.173.202.221/wssb/grlogo.jsp";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://account2.syyb.gov.cn/grinfo2.aspx";
                string name = "1002882428";
                string password = "19830301";
                postdata = String.Format("aac001={0}", name);
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

                string RYLB = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='RYLB']", "value")[0];
                string SFZH = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='SFZH']", "value")[0];
                string __EVENTVALIDATION = HttpUtility.UrlEncode(HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='__EVENTVALIDATION']", "value")[0]);
                string __VIEWSTATE = HttpUtility.UrlEncode(HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='__VIEWSTATE']", "value")[0]);
                string __VIEWSTATEGENERATOR = HttpUtility.UrlEncode(HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='__VIEWSTATEGENERATOR']", "value")[0]);

                string flag = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='flag']", "value")[0];
                string grbh = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='grbh']", "value")[0];
                string h_GRBH = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@id='h_GRBH']", "value")[0];

                url = "http://account2.syyb.gov.cn/A_17.aspx";
                postdata = String.Format("__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION={2}&grbh={3}&flag={4}&h_GRBH={5}&SFZH={6}&RYLB={7}",
                    __VIEWSTATE, __VIEWSTATEGENERATOR, __EVENTVALIDATION, grbh, flag, h_GRBH, SFZH, RYLB);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://account2.syyb.gov.cn/grinfo2.aspx",
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
        /// <summary>
        /// 石家庄（验证码：Y）
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
                url = "http://110.249.137.2:8080/eapdomain/login.do?method=begin";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string pid = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='pid']", "value")[0];


            Lable_Start:
                url = "http://110.249.137.2:8080/eapdomain/jcaptcha";
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
                url = "http://110.249.137.2:8080/eapdomain/j_unieap_security_check.do";
                string name = "130106197009060312";
                string password = "224631";
                postdata = String.Format("Method=P&pid={3}&j_username={0}&j_password={1}&jcaptcha_response={2}", name, password, vercode, pid);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = "	http://110.249.137.2:8080/eapdomain/login.do?method=begin",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html.IndexOf("刷新验证码") != -1)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://110.249.137.2:8080/eapdomain/login.do?method=login";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://110.249.137.2:8080/eapdomain/unieap/pages/index.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://110.249.137.2:8080/eapdomain/enterapp.do?method=begin&name=/si&welcome=/si/pages/index.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://110.249.137.2:8080/eapdomain/infoTip.do?method=retrieveTips";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = "",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://110.249.137.2:8080/eapdomain/ria_grid.do?method=query";
                postdata = "{header:{\"code\": -100, \"message\": {\"title\": \"\"}},body:{dataStores:{contentStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"contentStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.content\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}},xzStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"xzStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.xzxx\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}},userStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"userStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.grxx\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}},sbkxxStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"sbkxxStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.sbkxx\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}}},parameters:{\"BUSINESS_ID\": \"UCI314\", \"BUSINESS_REQUEST_ID\": \"REQ-IC-Q-098-60\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://110.249.137.2:8080/eapdomain/si/pages/default.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://110.249.137.2:8080/eapdomain/ria_grid.do?method=query";
                postdata = "{header:{\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}},body:{dataStores:{searchStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"searchStore\",pageNumber:1,pageSize:20,recordCount:0,context:{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\"},statementName:\"si.treatment.ggfw.yljf\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}}},parameters:{\"BUSINESS_ID\": \"UOA017\", \"BUSINESS_REQUEST_ID\": \"REQ-OA-M-013-01\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}";
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

                url = "http://110.249.137.2:8080/eapdomain/ria_grid.do?method=query";
                postdata = "{header:{\"code\": -100, \"message\": {\"title\": \"\", \"detail\": \"\"}},body:{dataStores:{contentStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"contentStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.content\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}},xzStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"xzStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.xzxx\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}},userStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"userStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.grxx\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}},sbkxxStore:{rowSet:{\"primary\":[],\"filter\":[],\"delete\":[]},name:\"sbkxxStore\",pageNumber:1,pageSize:2147483647,recordCount:0,statementName:\"si.treatment.ggfw.sbkxx\",attributes:{\"AAC002\": [\"130106197009060312\", \"12\"], \"AAE135\": [\"130106700906031\", \"12\"]}}},parameters:{\"BUSINESS_ID\": \"UCI314\", \"BUSINESS_REQUEST_ID\": \"REQ-IC-Q-098-60\", \"CUSTOMVPDPARA\": \"\", \"PAGE_ID\": \"\"}}}";
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 重庆（验证码：Y）
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
                url = "http://www.cqldbz.gov.cn:8003/ggfw/index.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

            Lable_Start:
                url = "http://www.cqldbz.gov.cn:9001/ggfw/validateCodeBLH_image.do?time=1417592386998";
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

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://www.cqldbz.gov.cn:8003/ggfw/validateCodeBLH_valid.do";
                postdata = "yzm=" + vercode;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string message = jsonParser.GetResultFromParser(httpRes.Html, "message");

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://www.cqldbz.gov.cn:9001/ggfw/LoginBLH_login.do";
                string name = "512229196706060017";
                string password = Convert.ToBase64String(Encoding.Default.GetBytes("95927X"));
                password = "95927X".ToBase64();
                postdata = String.Format("sfzh={0}&password={1}&validateCode={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                message = jsonParser.GetResultFromParser(httpRes.Html, "message");
                if (message != "操作成功!")
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_main.do?code=000";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_main.do?code=011";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                results = HtmlParser.GetResultFromParser(httpRes.Html, "//td[@name='xm']", "", true);

                url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_main.do?code=014";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_query.do";
                postdata = "code=015&year=2014";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_main.do?code=027";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);


                url = "http://www.cqldbz.gov.cn:9001/ggfw/QueryBLH_query.do";
                postdata = "code=023&year=2014";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
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

        /// <summary>
        /// 西安（验证码：Y）
        /// </summary>
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

            Lable_Start:
                //url = "http://www.cqldbz.gov.cn:9001/ggfw/validateCodeBLH_image.do?time=1417592386998";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    ResultType = ResultType.Byte,
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                //Utilities.WriteFileFromByte(httpRes.ResultByte);
                //string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                //Console.Write("请输入验证码：");
                //vercode = Console.ReadLine();
                url = "http://shbxcx.sn12333.gov.cn/sxlssLogin.do";
                string name = "622301198204234412";
                string password = HttpUtility.UrlEncode("潘宗仁", Encoding.GetEncoding("gb2312")).ToUpper();
                postdata = String.Format("uname={0}&aac003={1}&PSINPUT=d4hq&Icon2.x=63&Icon2.y=15", name, password);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                //string message = jsonParser.GetResultFromParser(httpRes.Html, "message");
                //if (message != "操作成功!")
                //{
                //    goto Lable_Start;
                //}
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://shbxcx.sn12333.gov.cn/personInfoQuery.do";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://shbxcx.sn12333.gov.cn/paymentQuery.do";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://shbxcx.sn12333.gov.cn/personAccountQuery.do";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
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
        /// <summary>
        /// 武汉（验证码：Y）
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
                url = "https://221.232.64.242:7022/grws/login.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    CerPath = "C:\\root.cer",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "https://221.232.64.242:7022/grws/dologin";
                string name = "420103196608163730";
                string password = "12345678";
                postdata = String.Format("j_username={0}&j_password={1}", name, password);
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

                postdata = String.Format("j_username={0}&j_password={1}", name, password);
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

                url = "https://221.232.64.242:7022/grws/action/MainAction?menuid=grwssb_grzlcx_grcbzlcx&ActionType=grwssb_grzlcx_grcbzlcx_q";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "https://221.232.64.242:7022/grws/action/MainAction?menuid=grwssb_grzlcx_grjfxxcx&ActionType=grwssb_grzlcx_grjfxxcx_q";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "https://221.232.64.242:7022/grws/jsp/common/tabledata.jsp";
                postdata = "id=grwssb_grsbyw_grjfxxcx_l&grwssb_grsbyw_grjfxxcx_l_page=1&ActionType=grwssb_grzlcx_grjfxxcx_q&filterOnNoDataRight=false&subTotalName=%25E5%25B0%258F%25E8%25AE%25A1&display=block&pageSize=10&hasPage=true&hasTitle=true&whereCls=%2520G.grsxh%2520%253D%25201388170%2520and%2520DWJFBZ%2520in%2520('1'%252C'2')%2520and%2520XZLX%2520in%2520('10'%252C'20'%252C'30'%252C'40'%252C'50'%252C'53'%252C'54')%2520order%2520by%2520JFNY%2520desc%252Cxzlx&type=q&title=%25E4%25B8%25AA%25E4%25BA%25BA%25E7%25BC%25B4%25E8%25B4%25B9%25E4%25BF%25A1%25E6%2581%25AF%25E5%2588%2597%25E8%25A1%25A8&menuid=grwssb_grzlcx_grjfxxcx&isPageOper=true&rowCnt=847&useAjaxPostPars=true";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "https://221.232.64.242:7022/grws/action/MainAction?menuid=grwssb_grzlcx_ylgrzhcx&ActionType=grwssb_grzlcx_ylgrzhcx_q";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "https://221.232.64.242:7022/grws/action/MainAction?menuid=grwssb_grzlcx_ylbxgrzhqkcx&ActionType=grwssb_grzlcx_ylbxgrzhqkcx_q";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "https://221.232.64.242:7022/grws/jsp/common/tabledata.jsp";
                postdata = "id=GRWS_GRCX_YLBXGRZHQK_XF_L&GRWS_GRCX_YLBXGRZHQK_XF_L_page=1&ActionType=grwssb_grzlcx_ylbxgrzhqkcx_q&filterOnNoDataRight=false&subTotalName=%25E5%25B0%258F%25E8%25AE%25A1&display=block&pageSize=20&hasPage=true&hasTitle=true&whereCls=%2520GRSXH%253D1388170%2520order%2520by%2520JSRQ%2520desc&type=q&title=%25E5%258C%25BB%25E7%2596%2597%25E4%25BF%259D%25E9%2599%25A9%25E5%258D%25A1%25E5%25B8%2590%25E6%2588%25B7%25E6%2598%258E%25E7%25BB%2586%25EF%25BC%2588%25E6%25B6%2588%25E8%25B4%25B9%25EF%25BC%2589&menuid=grwssb_grzlcx_ylbxgrzhqkcx&isPageOper=true&rowCnt=37&useAjaxPostPars=true";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
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
        /// <summary>
        /// 厦门（验证码：Y）
        /// </summary>
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
                url = "https://app.xmhrss.gov.cn/wcm/servlet/FirstServlet";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

            Lable_Start:
                url = "https://app.xmhrss.gov.cn/wcm/servlet/VCodeServlet";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.Number);

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "https://app.xmhrss.gov.cn/wcm/ChangeYzm?self=loginIndex&vcode=" + vercode;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = "1=1",
                    //SecurityProtocolType = "ssl",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                if (httpRes.Html != "{msg:'校验码成功'}")
                {
                    goto Lable_Start;
                }

                url = "https://app.xmhrss.gov.cn/wcm/servlet/WebLoginServlet?self=loginIndex";
                string name = "35220119870814361X";
                string password = "888814";
                postdata = String.Format("id0000={0}&userpwd={1}&vcode={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                //string message = jsonParser.GetResultFromParser(httpRes.Html, "message");
                //if (message != "操作成功!")
                //{
                //    goto Lable_Start;
                //}
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                url = "https://app.xmhrss.gov.cn/wcm/servlet/PersonalServlet";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "/html[1]/body[1]/table[1]/tr[1]/td[2]/table[1]/tr[1]/td[4]/div[5]/table[1]/tr/td[@align='left']", "text");


                url = "https://app.xmhrss.gov.cn/wcm/servlet/PaymentInfoServlet";
                postdata = "qsnyue=201412&jznyue=201408&xzdm00=&zmlx00=";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "/html[1]/body[1]/table[1]/tr[1]/td[2]/table[1]/tr[1]/td[4]/div[3]/form[1]/table[1]/tr[position()>1]", "");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 广州（验证码：Y）
        /// </summary>
        public static void GetGuangzhou()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "http://gzlss.hrssgz.gov.cn/cas/login";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string lt = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='lt']", "value")[0];
            Lable_Start:
                url = "http://gzlss.hrssgz.gov.cn/cas/captcha.jpg";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                Utilities.WriteFileFromByte(httpRes.ResultByte);
                string vercode = secodeParser.GetVerCodeByCharSort(httpRes.ResultByte, CharSort.NumberAndUpper);

                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
                url = "http://gzlss.hrssgz.gov.cn/cas/login";
                string name = "440104197411044732";
                string password = "1001311602";
                postdata = String.Format("username={0}&password={1}&yzm={2}&lt={3}&_eventId=submit", name, password, vercode, lt);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    //SecurityProtocolType = "ssl",
                    Referer = "	http://gzlss.hrssgz.gov.cn/cas/login",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpitem.Header.Add("Accept-Language", "zh-cn,zh;");
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//span[@id='*.errors']", "");
                if (results.Count > 0)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);


                //url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://gzlss.hrssgz.gov.cn/cas/login",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //url = "http://gzlss.hrssgz.gov.cn/cas/login?service=http%3A%2F%2Fgzlss.hrssgz.gov.cn%3A80%2Fgzlss_web%2Fbusiness%2Ftomain%2Fmain.xhtml";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://gzlss.hrssgz.gov.cn/cas/login",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                //url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml?ticket=ST-2074-AoewGuS2DDvAcF1W1aoI-cas";
                //httpitem = new HttpItem()
                //{
                //    URL = url,
                //    Method = "get",
                //    CookieCollection = cookies,
                //    Referer = "http://gzlss.hrssgz.gov.cn/cas/login",
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpRes = http.GetHtml(httpitem);
                //cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://gzlss.hrssgz.gov.cn/cas/login",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/tomain/main.xhtml";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://gzlss.hrssgz.gov.cn/cas/login",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/message/msg/refreshMsg.xhtml";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    Referer = "http://gzlss.hrssgz.gov.cn/cas/login",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC-MENU";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string csrftoken = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='csrftoken']", "value")[0];

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/realGetPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC";
                postdata = "csrftoken=" + csrftoken + "&pd=&type=1";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/anonview/viewPersonPayHistoryInfo.xhtml?aac001=1001311602&xzType=1&startStr=201502&endStr=201311&querylog=true&businessocde=291QB-GRJFLS&visitterminal=PC";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getHealthcarePersonPayHistorySumup.xhtml?query=1&querylog=true&businessocde=291QB_YBGRJFLSCX&visitterminal=PC&aac001=1001311602&startStr=201311&endStr=201501";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
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
        public static void GetShenzhen()
        {
            string cookieStr = string.Empty;
            string url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            HttpResult httpRes = null;
            HttpItem httpitem = null;
            try
            {
                url = "https://e.szsi.gov.cn/siservice/";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
                string pid = HtmlParser.GetResultFromParser(httpRes.Html, "//input[@name='pid']", "value")[0];
            Lable_Start:
                url = "https://e.szsi.gov.cn/siservice/PImages?pid=" + pid;
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    SecurityProtocolType = SecurityProtocolType.Ssl3,
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
                url = "https://e.szsi.gov.cn/siservice/LoginAction.do";
                string name = "442528196811030022";
                string password = "Lsy12345".ToBase64();
                postdata = String.Format("Method=P&pid=" + pid + "&type=&AAC002={0}&CAC222={1}&PSINPUT={2}", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                string errorStr = CommonFun.GetMidStr(httpRes.Html, "<script language='JavaScript'>alert('", "')");
                if (results.Count > 0)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getPersonInfoSearch.xhtml?querylog=true&businessocde=291QB-GRJCXX&visitterminal=PC";
                postdata = "pd=&type=1";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/anonview/viewPersonPayHistoryInfo.xhtml?aac001=1001311602&xzType=1&startStr=201502&endStr=201311&querylog=true&businessocde=291QB-GRJFLS&visitterminal=PC";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getHealthcarePersonPayHistorySumup.xhtml?query=1&querylog=true&businessocde=291QB_YBGRJFLSCX&visitterminal=PC&aac001=1001311602&startStr=201311&endStr=201501";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
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
                url = "http://www.bjld.gov.cn/csibiz/indinfo/login.jsp";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);
            Lable_Start:
                url = "http://www.bjld.gov.cn/csibiz/indinfo/validationCodeServlet.do";
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
                url = "http://www.bjld.gov.cn/csibiz/indinfo/login_check";
                string name = "152102198706230019";
                string password = "wentao1234";
                postdata = String.Format("type=1&flag=3&j_username={0}&j_password={1}&safecode={2}&x=37&y=18", name, password, vercode);
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Expect100Continue = false
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//font[@color='red']", "text", true);
                if (results.Count > 0)
                {
                    goto Lable_Start;
                }
                cookies = Utilities.GetCookieCollection(cookies, httpRes.CookieCollection);

                url = "http://www.bjld.gov.cn/csibiz/indinfo/search/ind/indNewInfoSearchAction";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "post",
                    Postdata = "dasdf",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Expect100Continue = false
                };
                httpRes = http.GetHtml(httpitem);
                results = HtmlParser.GetResultFromParser(httpRes.Html, "//table[2]/tr[position() >1]/td", "text", true);

                url = "http://www.bjld.gov.cn/csibiz/indinfo/search/ind/indPaySearchAction!oldage?searchYear=2014";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpRes = http.GetHtml(httpitem);

                url = "http://gzlss.hrssgz.gov.cn/gzlss_web/business/front/foundationcentre/getHealthcarePersonPayHistorySumup.xhtml?query=1&querylog=true&businessocde=291QB_YBGRJFLSCX&visitterminal=PC&aac001=1001311602&startStr=201311&endStr=201501";
                httpitem = new HttpItem()
                {
                    URL = url,
                    Method = "get",
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
    }

}
