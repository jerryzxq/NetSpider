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
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    class HA : IMobileCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        ApplyLogMongo logMongo = new ApplyLogMongo();
        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.189.cn/ha/";
                httpItem = new HttpItem
                {
                    URL = Url,
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

                Url = "http://www.189.cn/dqmh/cms/index/login_jx.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/ha/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/login/loginJT.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/cms/index/login_jx.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = string.Format("http://www.189.cn/dqmh/Uam.do?method=loginUamSendJT&logintype=telephone&shopId=null&loginRequestURLMark=http://www.189.cn/dqmh/login/loginJT.jsp&date=1444464434465");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/login/loginJT.jsp",
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string location = httpResult.Header["Location"].Replace(" ", "%20");
                //c2000004 手机号码登录 c2000001 固话号码登录 c2000002 宽带账号登录 
                Url = location;
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://www.189.cn/dqmh/login/loginJT.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@id='c2000004']/table/tr[7]/td[2]/input[@name='lt']", "value");
                string lt = string.Empty;//登陆标记
                if (results.Count > 0)
                {
                    lt = results[0];
                }
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

                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                ////保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                Res.StatusDescription = "河南电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;

                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("cookies", cookies);
                dic.Add("lt", lt);
                dic.Add("location", location);
                CacheHelper.SetCache(token, dic);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河南电信初始化异常";
                Log4netAdapter.WriteError("河南电信初始化异常", e);

                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Init)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            string lt = string.Empty;//登陆标记
            string location = string.Empty;//相应url
            Dictionary<string, object> dic = new Dictionary<string, object>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    lt = dic["lt"].ToString();
                    location = dic["location"].ToString();
                }
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "http://login.189.cn/login/ajax";
                postdata = string.Format("m=checkphone&phone={0}", mobileReq.Mobile);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(httpResult.Html);
                string customFileld02 = jsonObj["ProvinceID"].ToString();
                string AreaCode = jsonObj["AreaCode"].ToString();
                if (customFileld02 != "17")
                {
                    Res.StatusDescription = "用户名不存在";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = location;
                postdata = string.Format("forbidpass=null&forbidaccounts=null&authtype=c2000004&customFileld02={0}&areaname=%E6%B2%B3%E5%8D%97&username={1}&customFileld01=3&password={2}&randomId={3}&lt={4}&_eventId=submit&open_no=c2000004", customFileld02, mobileReq.Mobile, mobileReq.Password, mobileReq.Vercode, lt);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    Referer = location,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                List<string> results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='status2']", "text");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0]))
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }
                Url = CommonFun.GetMidStr(httpResult.Html, "location.replace('", "');"); ;
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
                    Referer = "http://www.189.cn/ha/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;
                dic = new Dictionary<string, object>();
                dic.Add("AreaCode", AreaCode);
                dic.Add("cookies", cookies);
                CacheHelper.SetCache(Res.Token, dic);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河南电信手机登录异常";
                Log4netAdapter.WriteError("河南电信手机登录异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_Login)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            VerCodeRes Res = new VerCodeRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            string AreaCode = string.Empty;
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    dic = (Dictionary<string, object>)CacheHelper.GetCache(mobileReq.Token);
                    cookies = (CookieCollection)dic["cookies"];
                    AreaCode = (string)dic["AreaCode"];
                }
                //重登陆开始
                Url = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    Referer = "http://www.189.cn/ha/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/public/v4/common/control/log/wb_event.jsp";
                postdata = string.Format("BROWSER_NAME=MOZILLA+41.0&OPER_SYSTEM=+rv%3A41.0&SCREEN_SIZE=1280*1024&VISIT_URL={1}&SOURCE_URL={0}&OBJECT_ID=", "http://www.189.cn/ha/".ToUrlEncode(), "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2".ToUrlEncode());
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/public/pages/service/bill/bill_tabs.jsp";
                postdata = string.Format("SERV_TYPE=FSE-2&AREA_CODE={0}&USER_FLAG=001&SIZE=12&COL_SIZE=4&SELECTED_SERV_NO=FSE-2-2", AreaCode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/public/v4/common/ui/serv_path.jsp";
                postdata = string.Format("SERV_KIND=FSE&SERV_TYPE=FSE-2");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = " http://ha.189.cn/service/service_login.jsp";
                postdata = string.Format("loginFlag=0&SERV_NAME=");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://www.189.cn/dqmh/frontLink.do?method=linkTo&shopId=10017&toStUrl=/service/bill/index.jsp?SERV_NO=FSE-2-2";//302
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "www.189.cn",
                    Referer = "http://ha.189.cn/service/service_login.jsp",
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = httpResult.Header["Location"];//302
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "uam.ct10000.com",
                    Referer = "http://ha.189.cn/service/service_login.jsp",
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = httpResult.Header["Location"];//302
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "uam.ct10000.com",
                    Referer = "http://ha.189.cn/service/service_login.jsp",
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = httpResult.Header["Location"];//200
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "uam.ct10000.com",
                    Referer = "http://ha.189.cn/service/service_login.jsp",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = CommonFun.GetMidStr(httpResult.Html, "location.replace('", "');</script>");//302
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "uam.ha.ct10000.com",
                    Allowautoredirect = false,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string referLocation = httpResult.Header["Location"];
                Url = referLocation;//200
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    Referer = referLocation,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/public/v4/common/control/log/wb_event.jsp";
                postdata = string.Format("BROWSER_NAME=MOZILLA+41.0&OPER_SYSTEM=+rv%3A41.0&SCREEN_SIZE=1280*1024&VISIT_URL={0}&SOURCE_URL={1}&OBJECT_ID=", "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2".ToUrlEncode(), referLocation.ToUrlEncode());
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Host = "ha.189.cn",
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/public/v4/common/ui/commonNav.jsp";
                postdata = string.Format("SERV_TYPE=FSE&SERV_KIND=FBU&SERV_TYPE_NAME=%E8%87%AA%E5%8A%A9%E6%9C%8D%E5%8A%A1&USER_FLAG=001");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Host = "ha.189.cn",
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/public/pages/service/bill/bill_tabs.jsp";
                postdata = string.Format("SERV_TYPE=FSE-2&AREA_CODE={0}&USER_FLAG=001&SIZE=12&COL_SIZE=4&SELECTED_SERV_NO=FSE-2-2", AreaCode);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Host = "ha.189.cn",
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = " http://ha.189.cn/service/bill/fycx/fyjg.jsp?DFLAG=3&SERV_NO=FSE-2-2";
                postdata = string.Format("SERV_NAME=%E8%AF%A6%E5%8D%95%E6%9F%A5%E8%AF%A2");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Host = "ha.189.cn",
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string PROD_TYPE = CommonFun.GetMidStr(httpResult.Html, "<script type=\"text/javascript\">gotoUrl", "</script>");
                Regex regex = new Regex(@"[0-9][0-9]*");
                MatchCollection matches = regex.Matches(PROD_TYPE);
                if (matches.Count == 2)
                {
                    PROD_TYPE = matches[1].Value;
                }
                Url = string.Format("http://ha.189.cn/service/bill/fycx/inxx.jsp?SERV_NO=FSE-2-2&_=1447136253781&ACC_NBR={0}&PROD_TYPE={1}&ACCTNBR97=", mobileReq.Mobile, PROD_TYPE);
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-2",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //重登陆结束

                //FIND_TYPE: 1长途清单,2市话清单,3上网,4短信,5 SP详单
                //QRY_FLAG: 1按帐期查询 ,0 按时间查询
                //ACCT_DATE:起始时间 201511-201506
                Url = "http://ha.189.cn/service/bill/fycx/inxxall.jsp";
                postdata = string.Format("ACC_NBR={0}&PROD_TYPE={1}&BEGIN_DATE=&END_DATE=&SERV_NO={2}&ValueType=1&REFRESH_FLAG=1&FIND_TYPE={3}&radioQryType=on&QRY_FLAG={4}&ACCT_DATE={5}&ACCT_DATE_1={5}", mobileReq.Mobile, PROD_TYPE, "FSE-2-2", "1", "1", DateTime.Now.ToString("yyyyMM"));
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //发送短信验证码
                Url = "http://ha.189.cn/service/bill/getRand.jsp";
                postdata = string.Format("PRODTYPE={0}&RAND_TYPE=002&BureauCode={1}&ACC_NBR={2}&PROD_TYPE={0}&PROD_PWD=&REFRESH_FLAG=1&BEGIN_DATE=&END_DATE=&ACCT_DATE={3}&FIND_TYPE=1&SERV_NO=FSE-2-2&QRY_FLAG=1&ValueType=4&MOBILE_NAME={2}&OPER_TYPE=CR1&PASSWORD=", PROD_TYPE, AreaCode, mobileReq.Mobile, DateTime.Now.ToString("yyyyMM"));
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<flag>", "</flag>").Trim();
                if (errorMsg != "0")
                {
                    Res.StatusDescription = "验证码发送失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
                Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河南电信手机验证码发送异常";
                Log4netAdapter.WriteError("河南电信手机验证码发送异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_SendSMS)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }
        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                //校验短信验证码
                Url = "http://service.sx.10086.cn/enhance/operate/pwdModify/randomPwdCheck.action";
                postdata = string.Format("randomPwd={0}", mobileReq.Smscode);
                httpItem = new HttpItem()
                {
                    Accept = "application/json, text/javascript, */*; q=0.01",
                    URL = Url,
                    Host = "service.sx.10086.cn",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (!httpResult.Html.StartsWith("{") && !httpResult.Html.EndsWith("{"))
                {
                    Res.StatusDescription = "短信验证码验证失败,刷新页面后重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string errorMsg = jsonParser.GetResultFromMultiNode(httpResult.Html, "retMsg");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.StatusDescription = "河南电信手机验证码验证成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Query;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "河南电信手机验证码验证异常";
                Log4netAdapter.WriteError("河南电信手机验证码验证异常", e);
                appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
                {
                    CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                    StatusCode = ServiceConsts.StatusCode_error,
                    Description = e.Message
                });
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                if (appLog.LogDtlList.Count < 1)
                {
                    appLog.LogDtlList.Add(new ApplyLogDtl(ServiceConsts.Step_CheckSMS)
                    {
                        CreateTime = DateTime.Now.ToString(Consts.DateFormatString9),
                        StatusCode = Res.StatusCode,
                        Description = Res.StatusDescription
                    });
                }
                logMongo.Save(appLog);
            }
            return Res;
        }
        /// <summary>
        /// 抓取数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            CrawlerData crawler = new CrawlerData() { Token = mobileReq.Token, IdentityCard = mobileReq.IdentityCard, Name = mobileReq.Name, Mobile = mobileReq.Mobile, UserType = "1" };
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                }
                #region  历史账单
                DateTime dtNow = DateTime.Now;
                Url = "http://ha.189.cn/service/bill/fycx/inzd.jsp";
                postdata = string.Format("ACC_NBR={0}&SERV_NO=FSE-2-3&REFRESH_FLAG=1&BillingCycle={1}&operateType=1&operateType=1", mobileReq.Mobile, dtNow.AddMonths(-1).ToString("yyyyMM"));
                httpItem = new HttpItem
                {
                    URL = Url,
                    Host = "ha.189.cn",
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Regex regex = new Regex(@"[0-9][0-9]*");
                string AreaCode = string.Empty;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[2]/td[3]/a", "href");
                if (results.Count > 0)
                {
                    MatchCollection matchs = regex.Matches(results[0]);
                    if (matchs.Count == 5)
                    {
                        AreaCode = matchs[2].Value;
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    decimal shippingCosts = 0; //优惠费用
                    decimal monthlyBasicFee = 0;//月基本费
                    MonthBill monthBill = new MonthBill();
                    monthBill.BillCycle = dtNow.AddMonths(-(i + 1)).ToString("yyyy-MM") + "-01";//计费周期
                    //usertype 1:用户账单,2:客户账单
                    Url = string.Format("http://ha.189.cn/service/bill/fycx/zd.jsp?ACC_NBR={0}&DATE={1}&AreaCode={2}&usertype={3}", mobileReq.Mobile, dtNow.AddMonths(-(i + 1)).ToString("yyyyMM"), AreaCode, "1");
                    httpItem = new HttpItem
                    {
                        URL = Url,
                        Host = "ha.189.cn",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "BillListInfo" + monthBill.BillCycle, CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                }
                #endregion
                #region 基本信息
                Url = "http://ha.189.cn/service/manage/";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-3",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "http://ha.189.cn/service/manage/my_selfinfo.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://ha.189.cn/service/manage/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "basicInfo", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                //积分
                Url = "http://ha.189.cn/public/find_bunus.jsp";
                httpItem = new HttpItem
                {
                    URL = Url,
                    //Referer = "http://ha.189.cn/service/account/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "IntegralInfo", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });
                //身份证
                Url = "http://ha.189.cn/service/transaction/";
                httpItem = new HttpItem
                {
                    URL = Url,
                    Referer = "http://ha.189.cn/service/bill/index.jsp?SERV_NO=FSE-2-3",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = "http://ha.189.cn/service/transaction/tel.jsp?SERV_NO=1A001";
                postdata = string.Format("SERV_TYPE=&SERV_NO={0}&SERV_KIND={1}&FUN_ID=&SUB_FUN_ID=&TARGET_FLAG=", "1A001", "FSE");
                httpItem = new HttpItem
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "http://ha.189.cn/service/transaction/",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                crawler.DtlList.Add(new CrawlerDtlData() { CrawlerTitle = "IdcardInfo", CrawlerTxt = System.Text.Encoding.Default.GetBytes(httpResult.Html) });

                #endregion
                #region==========短信详单（须短信验证,无卡未做）==========

                Sms sms = null;
                #endregion
                #region==========流量详单（须短信验证,无卡未做）==========

                Net gprs = null;
                #endregion
                #region==========语音详单（须短信验证,无卡未做）==========
                Call call = null;
                #endregion
                //保存
                crawlerMobileMongo.SaveCrawler(crawler);
                Res.StatusDescription = "河南电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "河南电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("河南电信手机账单抓取异常", e);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
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
            ApplyLog appLog = new ApplyLog(DateTime.Now.ToString(Consts.DateFormatString9), ServiceConsts.SpiderType_Mobile);
            ApplyLogDtl logDtl = new ApplyLogDtl("");
            CrawlerData crawler = new CrawlerData();
            BaseRes Res = new BaseRes();
            Res.Token = mobileReq.Token;
            CrawlerMobileMongo crawlerMobileMongo = new CrawlerMobileMongo(appDate);
            MobileMongo mobileMongo = new MobileMongo(appDate);
            Basic mobile = new Basic();
            string result = string.Empty;
            List<string> results = new List<string>();
            Regex regex = new Regex(@"[\&nbsp;\s]");
            DateTime dtNow = DateTime.Now;
            try
            {
                crawler = crawlerMobileMongo.GetCrawler(mobileReq.Token, mobileReq.Mobile, appDate);
                #region 基本信息查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Infor_Analysis);
                mobile.Token = mobileReq.Token;
                mobile.BusName = mobileReq.Name;
                mobile.BusIdentityCard = mobileReq.IdentityCard;
                mobile.Mobile = mobileReq.Mobile;
                mobile.UpdateTime = crawler.CrawlerDate;

                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "basicInfo").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Name = results[0];//姓名
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[3]/td[1]/td[1]", "text");
                if (results.Count > 0)
                {
                    mobile.Idcard = results[0];//证件号
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Address = results[0].Trim();//客户住址
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[6]/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Postcode = results[0].Trim();//住址邮政编码
                }
                if (string.IsNullOrWhiteSpace(mobile.Address))
                {
                    results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[6]/td[2]", "text");
                    if (results.Count > 0)
                    {
                        mobile.Address = results[0];//客户联系住址
                    }
                }
                if (string.IsNullOrWhiteSpace(mobile.Postcode))
                {
                    results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[7]/td[2]", "text");
                    if (results.Count > 0)
                    {
                        mobile.Postcode = results[0];//联系住址邮政编码       
                    }
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Email = results[0];//邮箱
                }
                results = HtmlParser.GetResultFromParser(result, "//form[@id='frmModify']/div/table/tr[10]/td[2]", "text");
                if (results.Count > 0)
                {
                    mobile.Regdate = DateTime.Parse(results[0]).ToString(Consts.DateFormatString11);//入网时间
                }
                //身份证
                result = Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "IdcardInfo").FirstOrDefault().CrawlerTxt);
                results = HtmlParser.GetResultFromParser(result, "//input[@id='CARD_NO']", "value");
                if (results.Count > 0)
                {
                    if (!string.IsNullOrEmpty(results[0]))
                    {
                        mobile.Idcard = results[0];//证件号码 
                    }
                }
                //当前积分
                result = System.Text.Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "IntegralInfo").FirstOrDefault().CrawlerTxt);
                mobile.Integral = new Regex(@"[1-9.][0-9.]*").Match(result).Value;//可用积分

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "个人信息解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 账单查询

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Bill_Analysis);

                for (int i = 0; i < 6; i++)
                {
                    decimal shippingCosts = 0; //优惠费用
                    decimal monthlyBasicFee = 0;//月基本费
                    MonthBill monthBill = new MonthBill();
                    monthBill.BillCycle = DateTime.Parse(dtNow.AddMonths(-(i + 1)).ToString("yyyy-MM") + "-01").ToString(Consts.DateFormatString12);//计费周期
                    //usertype 1:用户账单,2:客户账单
                    result = Encoding.Default.GetString(crawler.DtlList.Where(x => x.CrawlerTitle == "BillListInfo" + monthBill.BillCycle).FirstOrDefault().CrawlerTxt);
                    results = HtmlParser.GetResultFromParser(result, "//div[@id='htmltitle']/table/tr[position()>1]", "inner");
                    foreach (var item in results)
                    {
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text");
                        if (tdRow.Count == 2)
                        {
                            if (tdRow[0] == "优惠费用")
                            {
                                shippingCosts += tdRow[1].ToDecimal(0);//包含符号（±号）
                            }
                            else if (tdRow[0] == "月基本费")
                            {
                                monthlyBasicFee += tdRow[1].ToDecimal(0);
                            }
                        }
                        if (tdRow[0].Contains("费用总计"))
                        {
                            monthBill.TotalAmt = new Regex(@"[1-9.][0-9.]*").Match(tdRow[0]).Value;//总费用
                        }
                    }
                    monthBill.PlanAmt = (shippingCosts + monthlyBasicFee).ToString(CultureInfo.InvariantCulture);//月基本费（月基本费减去优惠费用）
                    mobile.BillList.Add(monthBill);
                }

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "账单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 通话详单

                DateTime dateNow = DateTime.Now;//当前时间
                int NowMonth = dateNow.Month;//当前月份
                int endMonth = NowMonth - 5;//截止月份
                logDtl = new ApplyLogDtl(ServiceConsts.Step_Call_Analysis);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "通话详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 短信详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Sms_Analysis);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "短信详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                #region 上网详单

                logDtl = new ApplyLogDtl(ServiceConsts.Step_Net_Analysis);

                logDtl.StatusCode = ServiceConsts.StatusCode_success;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = "上网详单解析成功";
                appLog.LogDtlList.Add(logDtl);
                #endregion
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "山西电信手机账单解析成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = jsonParser.SerializeObject(mobile);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "山西电信手机账单解析异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("山西电信手机账单解析异常", e);

                logDtl.StatusCode = ServiceConsts.StatusCode_fail;
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.Description = e.Message;
                appLog.LogDtlList.Add(logDtl);
            }
            finally
            {
                appLog.Token = Res.Token;
                appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
                logMongo.Save(appLog);
            }
            return Res;
        }
    }
}
