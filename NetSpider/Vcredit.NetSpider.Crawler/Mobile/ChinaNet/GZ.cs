using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    /// <summary>
    /// （测试账号:18089686918,198493）
    /// </summary>
    public class GZ : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion
        #region 私有变量
        string lt = string.Empty;//验证码核对信息
        #endregion
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                //第一步，初始化登录页面
                Url = "http://www.189.cn/dqmh/login/loginJT.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/cms/index/login_jx.jsp"
                };
                httpResult = httpHelper.GetHtml(httpItem);

                //第二步
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "https://uam.ct10000.com/ct10000uam/login?service=http://www.189.cn/dqmh/Uam.do?method=loginJTUamGet&returnURL=1&register=register2.0&UserIp=210.22.124.10,%20223.202.80.16";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/login/loginJT.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='c2000004']/table/tr[7]/td[2]/input[@name='lt']", "value");
                if (results.Count > 0)
                {
                    lt = results[0];
                }

                //第三步，获取验证码
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "https://uam.ct10000.com/ct10000uam/validateImg.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "贵州电信初始化完成";
                Res.nextProCode = ServiceConsts.NextProCode_Login;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "贵州电信初始化异常";
                Log4netAdapter.WriteError("贵州电信初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    CacheHelper.RemoveCache(mobileReq.Token);
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://uam.ct10000.com/ct10000uam/login?service=http://www.189.cn/dqmh/Uam.do?method=loginJTUamGet&returnURL=1&register=register2.0&UserIp=210.22.124.10,%20223.202.80.16";
                postdata = string.Format("forbidpass=null&forbidaccounts=null&authtype=c2000004&customFileld02=24&areaname=%E8%B4%B5%E5%B7%9E&username={0}&customFileld01=3&password={1}&randomId={2}&lt={3}&_eventId=submit&open_no=1", mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, lt);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = Url,
                    //Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='c2000004']/table/tr[6]/td[@id='status2']", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 如果上一步请求,设定为手动跳转执行此步
                //string url302Found = httpResult.Header["Location"];
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //Url = url302Found;
                //httpItem = new HttpItem
                //{
                //    URL = Url,
                //    Referer = "https://uam.ct10000.com/ct10000uam/login?service=http://www.189.cn/dqmh/Uam.do?method=loginJTUamGet&returnURL=1&register=register2.0&UserIp=210.22.124.10,%20223.202.80.16",
                //    CookieCollection = cookies,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                #endregion
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = CommonFun.GetMidStr(httpResult.Html, "location.replace('", "');</script>");
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "贵州电信初始化异常";
                Log4netAdapter.WriteError("贵州电信初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            return null;
        }
        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            return null;
        }
        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Basic mobile = new Basic();//基本信息
            MonthBill bill = null;//月消费
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            MobileMongo mobileMongo = new MobileMongo(appDate);
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                    CacheHelper.RemoveCache(mobileReq.Token);
                }
                #region 个人信息

                Url = "http://gz.189.cn/service";
                httpItem = new HttpItem
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://www.189.cn/gz/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gz.189.cn/service/manage/index.jsp?FLAG=1";
                httpItem = new HttpItem
                {
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    URL = Url,
                    Referer = "http://gz.189.cn/service/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gz.189.cn/service/service_login.jsp?RET_URL=%2Fservice%2Fmanage%2Findex.jsp%3FFLAG%3D1";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://gz.189.cn/service/manage/index.jsp?FLAG=1",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = CommonFun.GetMidStr(httpResult.Html, "location.replace('", "');</script>");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "http://gz.189.cn/service/manage/index.jsp?FLAG=1",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = CommonFun.GetMidStr(httpResult.Html, "location.replace(\"", "\");</script>");
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gz.189.cn" + HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='RET_URL']", "value")[0];
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Url = CommonFun.GetMidStr(httpResult.Html, "$(\"#my_Info\").load(\"", "\");");
                Url = "http://gz.189.cn" + Url;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='frmModify']/table/tr[4]/td[1]", "text");
                if (results.Count > 0)
                {
                    mobile.Idcard = results[0];//证件号码
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='span_RelaAddress']", "text");
                if (results.Count > 0)
                {
                    mobile.Address = results[0];//地址
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='span_RelaEmail']", "text");
                if (results.Count > 0)
                {
                    mobile.Email = results[0];//邮箱
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='frmModify']/table/tr[8]/td[1]", "text");
                if (results.Count > 0)
                {
                    mobile.Regdate = DateTime.Parse(results[0]).ToString(Consts.DateFormatString11);//创建日期
                }

                Url = "http://gz.189.cn/service/manage/index.jsp?FLAG=2";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gz.189.cn" + "/service/service_login.jsp?RET_URL=%2Fservice%2Fmanage%2Findex.jsp%3FFLAG%3D2";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gz.189.cn/service/manage/index.jsp?FLAG=2";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = CommonFun.GetMidStr(httpResult.Html, "$(\"#my_Info\").load(\"", "\");");
                Url = "http://gz.189.cn" + Url;
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://gz.189.cn/service/manage/prod_baseinfo.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab_prodinfo_1']/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Mobile = results[0];//手机号
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tab_prodinfo_1']/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.PackageBrand = results[0];//套餐品牌
                }

                Url = "http://gz.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-1";
                httpItem = new HttpItem
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string strPostMsg = CommonFun.GetMidStr(httpResult.Html, "gotoUrl('", "');");
                Regex regex = new Regex(@"[0-9][0-9]*");
                MatchCollection matchs = regex.Matches(strPostMsg);
                string ACC_NBR = string.Empty;//手机号(产品号)
                string AREA_CODE = string.Empty;//城市区号
                // string PROD_NO = string.Empty;
                if (matchs.Count == 3)
                {
                    ACC_NBR = matchs[0].Value;
                    AREA_CODE = matchs[1].Value;
                    // PROD_NO = matchs[2].Value;
                }
                //puk
                Url = "http://gz.189.cn/service/info/puk_query_sub.jsp";
                postdata = string.Format("AREACODE={1}&NUM={0}&NAME=&SERV_NO=&SERV_KIND=&SHOW_NUM={0}", ACC_NBR, AREA_CODE);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://gz.189.cn/support/attribution_search/index.jsp?faqIndex=2",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[2]/td", "text");
                if (results.Count > 0)
                {
                    regex = new Regex(@"[\s]*");
                    mobile.PUK = regex.Replace(results[0], "");//table 包含PUK和PUK2,取PUK保存 
                }
                //积分

                Url = "http://gz.189.cn/service/jf/marklist.jsp";
                postdata = string.Format("SERV_NO=JFCX&SERV_TYPE=FSE-4");
                httpItem = new HttpItem
                {
                    Accept = "text/html, */*; q=0.01",
                    URL = Url,
                    Referer = "http://gz.189.cn/service/jf/index.jsp?SERV_NO=JFCX",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//select[@id='CUST_ID']/option", "value");
                string CUST_ID = string.Empty;//客户编码
                if (results.Count > 0)
                {
                    CUST_ID = results[0];
                }
                Url = "http://gz.189.cn/service/jf/marklist.jsp";
                //将当前进程暂停0.5秒后执行
                System.Threading.Thread.Sleep(500);

                postdata = string.Format("SERV_NAME=&USER_FLAG=&SERV_TYPE=FSE-4&SERV_NO=JFCX&SERV_KIND=&SUB_FUN_ID=&FUN_ID=&FLAG=1&QUERY_KIND=21&DESC_OPEN_TYPE=&OPER_FLAG=1&ID_TYPE=2&CUST_ID={0}&NUM={1}", CUST_ID, ACC_NBR);
                httpItem = new HttpItem
                {
                    Accept = "*/*",
                    URL = Url,
                    Referer = "http://gz.189.cn/service/jf/index.jsp?SERV_NO=JFCX",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tabPrint']/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tabPrint']/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    regex = new Regex(@"[0-91-9]*");
                    mobile.Integral = regex.Match(results[0]).Value;//可用积分
                }

                #endregion
                #region 账单查询

              
                Url = "http://gz.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-3";
                httpItem = new HttpItem
                {
                    URL = Url,
                    //Referer = "http://gz.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                int startBillingMonth = int.Parse(DateTime.Now.ToString("yyyyMM"))-1;//201509
                int endBillingMonth = startBillingMonth - 5;//201504
                do
                {
                    MonthBill monthBill = new MonthBill();
                    Url = "http://gz.189.cn/service/bill/cust_bill/index.jsp?SERV_NO=FSE-2-3";
                    postdata = string.Format("PreshForm=1&BillingMonth={0}",startBillingMonth);
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        Referer = "http://gz.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-3",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='divDetails']/div[@class='divC']/table[1]/tr[3]/td", "text");
                    if (results.Count>0)
                    {
                        string temp = results[0].Trim().Replace("计费周期：", "");//2015.09.01-2015.09.30 
                        if (temp.Contains("-"))
                        {
                            monthBill.BillCycle = (Convert.ToDateTime(temp.Split('-')[0])).ToString(Consts.DateFormatString12);//计费周期
                        }
                    }
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='credit1']/tr[position()>1]", "inner");
                    decimal monthlyBasicFee  = 0;//套餐基本月费
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                        if (tdRow.Count!=2)
                        {
                            continue;
                        }
                        monthlyBasicFee += tdRow[1].ToDecimal(0);
                        if (tdRow[0].IndexOf("套餐超出费用", StringComparison.Ordinal)>-1)
                        {
                            break;
                        }
                    }
                    monthBill.PlanAmt = monthlyBasicFee.ToString(CultureInfo.InvariantCulture);//套餐金额
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='credit1']/tr[@valign='top']", "inner");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//th", "text");
                        if (tdRow.Count != 2)
                        {
                            continue;
                        }
                        if (tdRow[0].IndexOf("本项小计", StringComparison.Ordinal) > -1)
                        {
                            monthBill.TotalAmt = tdRow[1].Trim();//总金额
                        }
                    }
                    mobile.BillList.Add(monthBill);
                    startBillingMonth--;
                } while (endBillingMonth<=startBillingMonth);
                #endregion
                #region==========短信详单（须短信验证,无卡未做）==========

                Sms sms = null;//短息
                #endregion
                #region==========流量详单（须短信验证,无卡未做）==========

                Net gprs = null;//上网
                #endregion
                #region==========语音详单（须短信验证,无卡未做）==========

                Call call = null;//电话
                #endregion
                //保存
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "贵州电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusDescription = "贵州电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("贵州电信手机账单抓取异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 解析抓取的原始数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="crawlerDate"></param>
        /// <returns></returns>
        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
        }
    }
}
