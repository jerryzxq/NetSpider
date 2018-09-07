using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.Processor.Operation
{
    internal class PhoneOpr
    {
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #region 上海移动
        public VerCodeRes Shanghai_PhoneLogin(string PhoneNo, string Passoword, string VerCode, string SmsCode)
        {

            VerCodeRes baseRes = new VerCodeRes();
            string carrier = string.Empty;//手机号归属地
            string Url = string.Empty;
            string Uid = string.Empty;
            try
            {
                if (CacheHelper.GetCache(PhoneNo) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(PhoneNo);
                }
                if (PhoneNo.IsEmpty())
                {
                    baseRes.StatusDescription = "手机号不能为空";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                if (VerCode.IsEmpty() && SmsCode.IsEmpty())
                {
                    //第一步，初始化登录页面
                    httpItem = new HttpItem()
                    {
                        URL = "https://sh.ac.10086.cn/login",
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //第二步，获取验证码
                    httpItem = new HttpItem()
                    {
                        URL = "https://sh.ac.10086.cn/validationCode",
                        ResultType = ResultType.Byte,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //保存验证码图片在本地
                    FileOperateHelper.WriteVerCodeImage(PhoneNo, httpResult.ResultByte);
                    baseRes.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                    baseRes.VerCodeUrl = AppSettings.localUrl + "/vercodeimg.aspx?vercode="+PhoneNo;
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else if (!VerCode.IsEmpty() && !Passoword.IsEmpty() && SmsCode.IsEmpty())
                {
                    
                    //第三步，
                    httpItem = new HttpItem()
                    {
                        URL = string.Format("https://sh.ac.10086.cn/loginex?act=2&telno={0}&password={1}&authLevel=2&validcode={2}&ctype=1", PhoneNo, Passoword, VerCode),
                        Method = "POST",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Uid = jsonParser.GetResultFromParser(httpResult.Html, "uid");

                    //第四步，
                    httpItem = new HttpItem()
                    {
                        URL = string.Format("http://www.sh.10086.cn/sh/wsyyt/ac/forward.jsp?uid={0}&tourl=http://www.sh.10086.cn/sh/service/", Uid),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    //第五步，发手机验证码
                    httpItem = new HttpItem()
                    {
                        URL = string.Format("https://sh.ac.10086.cn/loginex?iscb=1&act=1&telno={0}", PhoneNo),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    baseRes.StatusDescription = "输入手机验证码，重新调用接口并传参";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                else if (!SmsCode.IsEmpty())
                {
                    //第六步，验证手机验证码
                    httpItem = new HttpItem()
                    {
                        URL = string.Format("https://sh.ac.10086.cn/loginex?iscb=1&act=2&telno={0}&password={1}&authLevel=1&validcode=", PhoneNo, SmsCode),
                        Referer = "http://www.sh.10086.cn/sh/wsyyt/ac/loginbox.jsp?al=1&telno=" + PhoneNo,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    Uid = CommonFun.GetMidStr(httpResult.Html, "ssoLoginCallback(", ")");
                    Uid = jsonParser.GetResultFromParser(Uid, "uid");

                    httpItem = new HttpItem()
                    {
                        URL = string.Format("http://www.sh.10086.cn/sh/wsyyt/busi.json?sid=WF000022"),
                        Postdata = "uid=" + Uid,
                        Method = "POST",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    baseRes.StatusDescription = "登录验证成功";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                }
                CacheHelper.SetCache(PhoneNo, cookies);

            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("手机账单查询登录异常", e);
            }
            return baseRes;
        }
        public MobileRes Shanghai_GetPhoneInfo(string PhoneNo)
        {
            MobileRes phoneRes = new MobileRes();
            MobileCall phoneCall = null;
            MobileGPRS phoneGPRS = null;
            MobileSMS phoneSMS = null;
            string carrier = string.Empty;//手机号归属地
            string Url = string.Empty;
            string Uid = string.Empty;
            string PhoneCostStr = string.Empty;
            string year = string.Empty;
            string filterfield = string.Empty;
            List<string[]> results = new List<string[]>();
            DateTime first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime last = DateTime.Now;
            try
            {
                if (CacheHelper.GetCache(PhoneNo) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(PhoneNo);
                }


                //个人信息
                httpItem = new HttpItem()
                {
                    URL = "http://www.sh.10086.cn/sh/wsyyt/action?act=myarea.getinfoManage",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string PersonInfo = jsonParser.GetResultFromParser(httpResult.Html, "value");
                if (!PersonInfo.IsEmpty())
                {
                    phoneRes.Idtype = jsonParser.GetResultFromParser(PersonInfo, "zjType");
                    phoneRes.Idcard = jsonParser.GetResultFromParser(PersonInfo, "zjNum");
                    phoneRes.PhoneNum = jsonParser.GetResultFromParser(PersonInfo, "telNo");
                    phoneRes.Name = jsonParser.GetResultFromParser(PersonInfo, "name");
                    phoneRes.Address = jsonParser.GetResultFromParser(PersonInfo, "address");
                    phoneRes.Email = jsonParser.GetResultFromParser(PersonInfo, "email");
                    phoneRes.Postcode = jsonParser.GetResultFromParser(PersonInfo, "postcode");
                    phoneRes.PUK = jsonParser.GetResultFromParser(PersonInfo, "puk");
                }

                #region 话费详单


                //当月详单
                httpItem = new HttpItem()
                {
                    URL = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getOneBillDetailAjax",
                    Referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp",
                    Method = "POST",
                    Postdata = string.Format("billType=NEW_GSM&startDate={0}&endDate={1}&jingque=&searchStr=-1&index=0&isCardNo=0&gprsType=", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd")),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "value =\"", "\";members");
                results.AddRange(jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr));

                //前五个月详单

                for (int i = 1; i <= 5; i++)
                {
                    first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                    last = first.AddMonths(1).AddDays(-1);
                    year = first.Year.ToString();

                    httpItem = new HttpItem()
                    {
                        URL = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getFiveBillDetailAjax",
                        Referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp",
                        Method = "POST",
                        Postdata = string.Format("billType=NEW_GSM&startDate={0}&endDate={1}&&filterfield=输入对方号码：&filterValue=&searchStr=-1&index=0&isCardNo=0&gprsType=", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd")),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "var value = [];value = ", ";members");
                    results.AddRange(jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr));
                }
                foreach (string[] arrItem in results)
                {
                    phoneCall = new MobileCall();
                    phoneCall.StartTime = DateTime.Parse(year + "-" + arrItem[1]);
                    phoneCall.CallPlace = arrItem[2];
                    phoneCall.InitType = arrItem[3];
                    phoneCall.OtherCallPhone = arrItem[4];
                    phoneCall.UseTime = arrItem[5];
                    phoneCall.CallType = arrItem[6];
                    phoneCall.SubTotal = arrItem[8].ToDecimal(0);
                    phoneRes.PhoneCallList.Add(phoneCall);
                }
                #endregion

                #region 短信详单
                results.Clear();
                first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                last = DateTime.Now;

                //当月详单
                httpItem = new HttpItem()
                {
                    URL = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getOneBillDetailAjax",
                    Referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp",
                    Method = "POST",
                    Postdata = string.Format("billType=NEW_SMS&startDate={0}&endDate={1}&jingque=&searchStr=-1&index=0&isCardNo=0&gprsType=", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd")),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "value =\"", "\";members");
                results.AddRange(jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr));

                //前五个月详单
                for (int i = 1; i <= 5; i++)
                {
                    first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                    last = first.AddMonths(1).AddDays(-1);
                    year = first.Year.ToString();

                    httpItem = new HttpItem()
                    {
                        URL = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getFiveBillDetailAjax",
                        Referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp",
                        Method = "POST",
                        Postdata = string.Format("billType=NEW_SMS&startDate={0}&endDate={1}&&filterfield=输入对方号码：&filterValue=&searchStr=-1&index=0&isCardNo=0&gprsType=", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd")),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "var value = [];value = ", ";members");
                    results.AddRange(jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr));
                }
                foreach (string[] arrItem in results)
                {
                    phoneSMS = new MobileSMS();
                    phoneSMS.StartTime = DateTime.Parse(year + "-" + arrItem[1]);
                    phoneSMS.SmsPlace = arrItem[2];
                    phoneSMS.OtherSmsPhone = arrItem[3];
                    phoneSMS.InitType = arrItem[4];
                    phoneSMS.SmsType = arrItem[5];
                    phoneSMS.SubTotal = arrItem[8].ToDecimal(0);
                    phoneRes.PhoneSMSList.Add(phoneSMS);
                }

                #endregion

                #region 上网详单
                results.Clear();
                first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                last = DateTime.Now;

                //当月详单
                httpItem = new HttpItem()
                {
                    URL = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getOneBillDetailAjax",
                    Referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp",
                    Method = "POST",
                    Postdata = string.Format("billType=NEW_GPRS&startDate={0}&endDate={1}&jingque=&searchStr=-1&index=0&isCardNo=0&gprsType=", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd")),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "value =\"", "\";members");
                results.AddRange(jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr));

                //前五个月详单
                for (int i = 1; i <= 5; i++)
                {
                    first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                    last = first.AddMonths(1).AddDays(-1);
                    year = first.Year.ToString();

                    httpItem = new HttpItem()
                    {
                        URL = "http://www.sh.10086.cn/sh/wsyyt/busi/historySearch.do?method=getFiveBillDetailAjax",
                        Referer = "http://www.sh.10086.cn/sh/wsyyt/busi/2002_14.jsp",
                        Method = "POST",
                        Postdata = string.Format("billType=NEW_GPRS&startDate={0}&endDate={1}&&filterfield=输入对方号码：&filterValue=&searchStr=-1&index=0&isCardNo=0&gprsType=", first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd")),
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    PhoneCostStr = CommonFun.GetMidStr(httpResult.Html, "var value = [];value = ", ";members");
                    results.AddRange(jsonParser.DeserializeObject<List<string[]>>(PhoneCostStr));
                }
                foreach (string[] arrItem in results)
                {
                    phoneGPRS = new MobileGPRS();
                    phoneGPRS.StartTime = DateTime.Parse(year + "-" + arrItem[1]);
                    phoneGPRS.Place = arrItem[2];
                    phoneGPRS.PhoneNetType = arrItem[3];
                    phoneGPRS.UseTime = arrItem[4];
                    phoneGPRS.SubFlow = arrItem[5];
                    phoneGPRS.SubTotal = arrItem[7].ToDecimal(0);
                    phoneGPRS.NetType = arrItem[8];
                    phoneRes.PhoneGPRSList.Add(phoneGPRS);
                }
                #endregion

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("手机账单查询异常", e);
            }
            return phoneRes;
        }
        #endregion
    }
}
