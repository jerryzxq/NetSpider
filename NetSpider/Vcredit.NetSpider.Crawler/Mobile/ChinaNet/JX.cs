using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Newtonsoft.Json.Linq;
using Vcredit.NetSpider.DataAccess.Mongo;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using System.Web;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Crawler.Mobile.ChinaNet
{
    public class JX : IMobileCrawler
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

        public VerCodeRes MobileInit(MobileReq mobileReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //第一步，初始化登录页面
                Url = "http://login.189.cn/login";
                httpItem = new HttpItem()
                {
                    URL = Url
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_Login;


                Res.StatusDescription = "江西电信初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江西电信初始化异常";
                Log4netAdapter.WriteError("江西电信初始化异常", e);
            }


            return Res;
        }

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
                //校验参数
                if (mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //第三步
                Url = "http://login.189.cn/login";
                var key = AES.GetMD5("login.189.cn");
                AES.Key = key;
                AES.IV = "1234567812345678";
                mobileReq.Password = AES.AESEncrypt(mobileReq.Password);
                postdata = string.Format("Account={0}&UType=201&ProvinceID=15&AreaCode=&CityNo=&RandomFlag=0&Password={1}&Captcha=", mobileReq.Mobile, mobileReq.Password.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
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
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
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

                Res.StatusDescription = "登录成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.nextProCode = ServiceConsts.NextProCode_SendSMS;

                CacheHelper.SetCache(mobileReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "江西电信登录异常";
                Log4netAdapter.WriteError("江西电信登录异常", e);
            }

            return Res;
        }

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            //string Url = string.Empty;
            //string postdata = string.Empty;
            //DateTime date = DateTime.Now;
            //try
            //{
            //    //获取缓存
            //    if (CacheHelper.GetCache(mobileReq.Token) != null)
            //        cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
            //    //校验参数
            //    if (mobileReq.Mobile.IsEmpty())
            //    {
            //        Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
            //    postdata = string.Format("fastcode=00380407");
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        Postdata = postdata,
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            //    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/public/postValidCode.jsp?NUM={0}&AREA_CODE=188&LOGIN_TYPE=21&OPER_TYPE=CR0&RAND_TYPE=002";
            //    Url = string.Format(Url , mobileReq.Mobile);
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "Post",
            //        CookieCollection = cookies,
            //        Referer="http://he.189.cn/service/bill/feeQuery_iframe.jsp?SERV_NO=SHQD1",
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    if (httpResult.Html == "本次请求并未返回任何数据")
            //    {
            //        Res.StatusDescription = "本次请求并未返回任何数据";
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }

            //    string res = getStringByElementName(httpResult.Html, "actionFlag");
            //    if (res!="0")
            //    {
            //        Res.StatusDescription = "短信发送失败";
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }

            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            //    Res.StatusDescription = "输入手机验证码，调用手机验证码验证接口";
            //    Res.StatusCode = ServiceConsts.StatusCode_success;
            //    Res.nextProCode = ServiceConsts.NextProCode_CheckSMS;

            //    CacheHelper.SetCache(mobileReq.Token, cookies);
            //}
            //catch (Exception e)
            //{
            //    Res.StatusCode = ServiceConsts.StatusCode_error;
            //    Res.StatusDescription = "江西电信手机验证码发送异常";
            //    Log4netAdapter.WriteError("江西电信手机验证码发送异常", e);
            //}
            return Res;
        }
        public string getStringByElementName(string soure, string elementname)
        {
            string resultStr = String.Empty;
            using (StringReader strRdr = new StringReader(soure))
            {
                //通过XmlReader.Create静态方法创建XmlReader实例
                using (XmlReader rdr = XmlReader.Create(strRdr))
                {
                    //循环Read方法直到文档结束
                    while (rdr.Read())
                    {
                        //如果是开始节点
                        if (rdr.NodeType == XmlNodeType.Element)
                        {
                            string elementName = rdr.Name;
                            if (elementName == elementname)
                            {
                                //读取到节点内文本内容
                                if (rdr.Read())
                                {
                                    //通过rdr.Value获得文本内容
                                    resultStr = HttpUtility.HtmlDecode(rdr.Value);
                                    break;

                                }
                            }
                        }
                    }
                }
            }

            return resultStr;
        }
        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            //string Url = string.Empty;
            //string postdata = string.Empty;
            //DateTime date = DateTime.Now;
            //try
            //{
            //    //获取缓存
            //    if (CacheHelper.GetCache(mobileReq.Token) != null)
            //        cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);
            //    //校验参数
            //    if (mobileReq.Mobile.IsEmpty() || mobileReq.Smscode.IsEmpty())
            //    {
            //        Res.StatusDescription = ServiceConsts.Mobile_MobileOrPasswordEmpty;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    //http://he.189.cn/public/pwValid.jsp
            //    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/public/pwValid.jsp?_FUNC_ID_=WB_VALID_RANDPWD&MOBILE_CODE={0}&ACC_NBR={1}&AREA_CODE=188&LOGIN_TYPE=21&MOBILE_FLAG=&MOBILE_LOGON_NAME=&MOBILE_CODE={2}";
            //    Url = string.Format(Url, mobileReq.Smscode, mobileReq.Mobile, mobileReq.Smscode);
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        Referer = "http://he.189.cn/service/bill/feeQuery_iframe.jsp?SERV_NO=SHQD1",
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    if (httpResult.Html == "本次请求并未返回任何数据")
            //    {
            //        Res.StatusDescription = "本次请求并未返回任何数据";
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }

            //    string res = getStringByElementName(httpResult.Html, "actionFlag");
            //    if (res!="0")
            //    {
            //        Res.StatusDescription = getStringByElementName(httpResult.Html, "actionMsg");
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }

            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            //    Res.StatusDescription = "江西电信手机验证码验证成功";
            //    Res.StatusCode = ServiceConsts.StatusCode_success;
            //    Res.nextProCode = ServiceConsts.NextProCode_Query;

            //    CacheHelper.SetCache(mobileReq.Token, cookies);

            //}
            //catch (Exception e)
            //{
            //    Res.StatusCode = ServiceConsts.StatusCode_error;
            //    Res.StatusDescription = "江西电信手机验证码验证异常";
            //    Log4netAdapter.WriteError("江西电信手机验证码验证异常", e);
            //}
            return Res;
        }

        public BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            //Basic mobile = new Basic();
            //Call call = null;
            //Net gprs = null;
            //Sms sms = null;
            //MonthBill bill = null;
            //string Url = string.Empty;
            //string postdata = string.Empty;
            //DateTime date = DateTime.Now;
            //List<JObject> results = new List<JObject>();
            //List<string> infos = new List<string>();
            //try
            //{
            //    if (CacheHelper.GetCache(mobileReq.Token) != null)
            //        cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

            //    #region 用户基本信息
            //    Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
            //    postdata = string.Format("fastcode=	00420429");
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        CookieCollection = cookies,
            //        Postdata = postdata,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            //    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/manage/index_iframe.jsp?FLAG=1";
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    infos = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td");
            //    if (infos.Count != 0)
            //    {
            //        mobile.Name = infos[0];
            //        mobile.StarLevel = infos[3];
            //        mobile.Idtype = infos[4];
            //        mobile.Idcard = infos[5];
            //        mobile.Address = HtmlParser.GetResultFromParser(infos[6], "//span")[0]; ;
            //    }
            //    #endregion

            //    #region 套餐
            //    Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
            //    postdata = string.Format("fastcode=00420427");
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        CookieCollection = cookies,
            //        Postdata = postdata,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


            //    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/manage/mySelfProdIndex_iframe.jsp";
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


            //    httpResult = httpHelper.GetHtml(httpItem);
            //    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/manage/prod_baseinfo_iframe.jsp";
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "Post",
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    var tableid = "tab_prodinfo_" + mobileReq.Mobile;
            //    infos = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='" + tableid + "']/tr/td");
            //    if (infos.Count != 0)
            //    {
            //        mobile.Package = infos[5];
            //        mobile.PackageBrand = infos[3];
            //        mobile.Regdate = infos[9];
            //    }

            //    mobile.Token = mobileReq.Token;
            //    mobile.BusName = mobileReq.Name;
            //    mobile.BusIdentityCard = mobileReq.IdentityCard;

            //    #endregion

            //    #region 积分
            //    Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
            //    postdata = string.Format("fastcode=00410426");
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        Postdata = postdata,
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            //    Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/club/mainclub/pointServ_iframe.jsp?DFlag=0";
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    infos = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='sum'][1]");
            //    if (infos.Count != 0)
            //    {
            //        mobile.Integral = infos[0]; //积分
            //    }
            //    #endregion

            //    #region 月消费情况

            //    string AREA_CODE = string.Empty;
            //    string NUM = string.Empty;
            //    string PROD_NO = string.Empty;
            //    string ServiceKind = string.Empty;
            //    string USER_ID = string.Empty;
            //    string USER_NAME = string.Empty;

            //    //当月账单
            //    Url = string.Format("http://www.189.cn/dqmh/my189/checkMy189Session.do");
            //    postdata = string.Format("fastcode=00380405");
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        CookieCollection = cookies,
            //        Postdata = postdata,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

            //    Url = " http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/feeQuery_iframe.jsp?SERV_NO=sshf";
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
            //    infos = HtmlParser.GetResultFromParser(httpResult.Html, "//script[2]");
            //    if (infos.Count>0)
            //    {
            //        NUM = infos[1].Split(',')[0].Replace("'", "").Split('(')[1];
            //        AREA_CODE = infos[1].Split(',')[1].Replace("'", "");
            //        PROD_NO = infos[1].Split(',')[2].Replace("'", "");
            //        USER_NAME = infos[1].Split(',')[3].Replace("'", "");
            //        ServiceKind = infos[1].Split(',')[4].Replace("'", "");
            //        USER_ID = infos[1].Split(',')[5].Replace("'", "").Split(')')[0];
            //    }
            //    Url = " http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/action/ifr_rt_iframe.jsp?NUM={0}&AREA_CODE={1}&PROD_NO={2}&USER_NAME={3}&ServiceKind={4}&USER_ID={5}";
            //    Url = string.Format(Url, NUM, AREA_CODE, PROD_NO, USER_NAME.ToUrlEncode(), ServiceKind, USER_ID);
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        CookieCollection = cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    infos = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='bill_content']");


            //    if (infos.Count != 0)
            //    {
            //        bill = new MonthBill();
            //        bill.BillCycle = date.ToString(Consts.DateFormatString12);
            //        bill.PlanAmt = HtmlParser.GetResultFromParser(infos[0], "//table/tr[2]/td[2]")[0];
            //        bill.TotalAmt = HtmlParser.GetResultFromParser(infos[0], "//p/span[1]")[0];
            //        mobile.BillList.Add(bill);
            //    }


            //    //前五个月账单
            //    for (int i = 1; i <= 5; i++)
            //    {
            //        date = date.AddMonths(-1);
            //        Url = string.Format("http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/action/bill_month_list_detail_iframe.jsp?ACC_NBR={0}&SERVICE_KIND=8&feeDate={1}&retCode=0000",mobileReq.Mobile,date.ToString("yyyyMM"));
            //        httpItem = new HttpItem()
            //        {
            //            URL = Url,
            //            Method = "POST",
            //            CookieCollection = cookies,
            //            ResultCookieType = ResultCookieType.CookieCollection
            //        };
            //        httpResult = httpHelper.GetHtml(httpItem);
            //        infos = HtmlParser.GetResultFromParser(httpResult.Html, "//table");
            //        if (infos.Count>0)
            //        {
            //            bill = new MonthBill();
            //            bill.BillCycle = date.ToString(Consts.DateFormatString12);
            //            bill.TotalAmt = HtmlParser.GetResultFromParser(infos[4], "//tr[1]/th/div")[0].Split('：')[1].Replace("元","");
            //            //该账号历史账单没有产生费用，故套餐金额暂时无法获取
            //            if (HtmlParser.GetResultFromParser(infos[3], "//tr[2]/td[2]").Count>0)
            //            {
            //                //bill.PlanAmt
            //            }

            //            mobile.BillList.Add(bill);
            //        }
            //    }

            //    #endregion


            //    #region 详单查询

            //    Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
            //    postdata = "fastcode=00380407";
            //    httpItem = new HttpItem()
            //    {
            //        URL = Url,
            //        Method = "POST",
            //        CookieCollection = cookies,
            //        Postdata = postdata,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    httpResult = httpHelper.GetHtml(httpItem);
            //    if (httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        Res.StatusCode = ServiceConsts.StatusCode_fail;
            //        return Res;
            //    }
            //    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


            //    // 短信详单
            //    GetDeatilsCall("2", "移动短信详单", mobileReq, mobile);

            //    // 上网详单
            //    GetDeatilsCall("3", "移动上网详单", mobileReq, mobile);

            //    // 话费详单
            //    GetDeatilsCall("1", "移动语音详单", mobileReq, mobile);

            //    #endregion 

            //    //保存
            //    MobileMongo mobileMongo = new MobileMongo();
            //    mobileMongo.SaveBasic(mobile);
            //    Res.StatusDescription = "江西电信手机账单抓取成功";
            //    Res.StatusCode = ServiceConsts.StatusCode_success;

            //}
            //catch (Exception e)
            //{
            //    Res.StatusDescription = "江西电信手机账单抓取异常";
            //    Res.StatusCode = ServiceConsts.StatusCode_fail;
            //    Log4netAdapter.WriteError("江西电信手机账单抓取异常", e);
            //}
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

        /// <summary>
        /// 抓取手机详单
        /// </summary>
        /// <param name="queryType">1:通话；2:短信；3:上网</param>
        /// <param name="mobileReq">手机号</param>
        /// <returns></returns>
        public void GetDeatilsCall(string queryType, string queryName, MobileReq mobileReq, Basic mobile)
        {
            string Url = string.Empty;
            DateTime date = DateTime.Now;
            var startDate = String.Empty;
            var endDate = String.Empty;
            for (int i = 0; i <= 5; i++)
            {
                date = date.AddMonths(-i);
                startDate = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");
                endDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                Url = string.Format("http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10006&toStUrl=http://he.189.cn/service/bill/action/ifr_bill_detailslist_em_new_iframe.jsp?ACC_NBR={0}&CITY_CODE=188&BEGIN_DATE={1}&END_DATE={2}&FEE_DATE={3}&SERVICE_KIND=8&retCode=0000&QUERY_TYPE_NAME={4}&QUERY_TYPE={5}&radioQryType=on&QRY_FLAG=1&ACCT_DATE={6}&ACCT_DATE_1={7}&openFlag=1", mobileReq.Mobile, startDate + "+00%3A00%3A00", endDate + "+23%3A59%3A59", date.ToString("yyyyMM"), queryName.ToUrlEncode(), queryType, date.ToString("yyyyMM"), date.ToString("yyyyMM"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var infos = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='details_table']/tbody/tr", "inner");
                foreach (var item in infos)
                {
                    //短信详单
                    if (queryType == "2")
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Sms sms = new Sms();
                        if (tdRow.Count > 0)
                        {
                            sms.OtherSmsPhone = tdRow[1];
                            sms.InitType = tdRow[2];
                            sms.StartTime = tdRow[3];
                            sms.SubTotal = tdRow[4].ToDecimal(0);
                            mobile.SmsList.Add(sms);
                        }
                    }
                    else if (queryType == "3")    //上网详单
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Net gprs = new Net();
                        if (tdRow.Count > 0)
                        {
                            gprs.StartTime = tdRow[1];
                            gprs.Place = tdRow[5];
                            gprs.NetType = tdRow[4];
                            gprs.SubTotal = tdRow[7].ToDecimal(0);
                            gprs.SubFlow = tdRow[3];
                            gprs.UseTime = tdRow[2];
                            gprs.PhoneNetType = tdRow[6];
                            mobile.NetList.Add(gprs);
                        }
                    }
                    else      //通话详单
                    {
                        List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        Call call = new Call();
                        if (tdRow.Count > 0)
                        {
                            call.StartTime = tdRow[3];
                            call.OtherCallPhone = tdRow[1];
                            call.UseTime = tdRow[4];
                            call.CallType = tdRow[9];
                            call.InitType = tdRow[2];
                            call.SubTotal = tdRow[10].ToDecimal(0);
                            mobile.CallList.Add(call);
                        }
                    }
                }

            }

        }

    }
}
