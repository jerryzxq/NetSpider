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
    public class LN : ChinaNet
    {
        public override VerCodeRes MobileSendSms(MobileReq mobileReq)
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
            //    Res.StatusDescription = "辽宁电信手机验证码发送异常";
            //    Log4netAdapter.WriteError("辽宁电信手机验证码发送异常", e);
            //}
            return Res;
        }

        public override BaseRes MobileCheckSms(MobileReq mobileReq)
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
            //    Res.StatusDescription = "辽宁电信手机验证码验证成功";
            //    Res.StatusCode = ServiceConsts.StatusCode_success;
            //    Res.nextProCode = ServiceConsts.NextProCode_Query;

            //    CacheHelper.SetCache(mobileReq.Token, cookies);

            //}
            //catch (Exception e)
            //{
            //    Res.StatusCode = ServiceConsts.StatusCode_error;
            //    Res.StatusDescription = "辽宁电信手机验证码验证异常";
            //    Log4netAdapter.WriteError("辽宁电信手机验证码验证异常", e);
            //}
            return Res;
        }

        public override BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate)
        {
            BaseRes Res = new BaseRes();
            Basic mobile = new Basic();
            MonthBill bill = null;
            string Url = string.Empty;
            string postdata = string.Empty;
            DateTime date = DateTime.Now;
            List<JObject> results = new List<JObject>();
            List<string> infos = new List<string>();
            try
            {
                if (CacheHelper.GetCache(mobileReq.Token) != null)
                    cookies = (CookieCollection)CacheHelper.GetCache(mobileReq.Token);

                #region 用户基本信息

                Url = "http://ln.189.cn/group/UamLoginAction/getUrl.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = "DrUrl=http%3A%2F%2Fwww.189.cn%2Fln%2Fservice%2F",
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

                Url = "http://www.189.cn/dqmh/ssoLink.do?method=linkTo&platNo=10005&toStUrl=http://ln.189.cn/group/bill/bill_owed.action?rand=1423637750376";
                httpItem = new HttpItem()
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


                Url = "http://ln.189.cn/group/info/info_view.do";
                httpItem = new HttpItem()
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

                Url = "http://ln.189.cn/getSessionInfo.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (!string.IsNullOrEmpty(jsonParser.GetResultFromParser(httpResult.Html, "userName")))
                {
                    mobile.Name = jsonParser.GetResultFromParser(httpResult.Html, "userName");  //姓名
                }

                if (!string.IsNullOrEmpty(jsonParser.GetResultFromParser(httpResult.Html, "indentNbrType")))
                {
                    mobile.Idtype = jsonParser.GetResultFromParser(httpResult.Html, "indentNbrType"); ; //证件类型
                }

                if (!string.IsNullOrEmpty(jsonParser.GetResultFromParser(httpResult.Html, "indentCode")))
                {
                    mobile.Idcard = jsonParser.GetResultFromParser(httpResult.Html, "indentCode");  //证件号码
                }

                if (!string.IsNullOrEmpty(jsonParser.GetResultFromParser(httpResult.Html, "userAddress")))
                {
                    mobile.Address = jsonParser.GetResultFromParser(httpResult.Html, "userAddress"); //家庭住址
                }

                if (!string.IsNullOrEmpty(jsonParser.GetResultFromParser(httpResult.Html, "acceptDate")))
                {
                    mobile.Regdate = jsonParser.GetResultFromParser(httpResult.Html, "acceptDate"); //入网时间
                }



                //套餐品牌
                var packbrand = jsonParser.GetResultFromParser(httpResult.Html, "productCollection");
                if (!string.IsNullOrEmpty(packbrand))
                {
                    packbrand = packbrand.Replace("[", "").Replace("]", "");
                    packbrand = jsonParser.GetResultFromParser(packbrand, "custInfo");
                    mobile.PackageBrand = jsonParser.GetResultFromParser(packbrand, "BrandIndDesc");
                }


                //主套餐

                var package = jsonParser.GetResultFromParser(httpResult.Html, "productCollection");
                if (!string.IsNullOrEmpty(package))
                {
                    package = package.Replace("[", "").Replace("]", "");
                    package = jsonParser.GetResultFromParser(package, "userInfo");
                    mobile.Package = jsonParser.GetResultFromParser(package, "ProdOfferName");
                }
                #endregion


                #region 积分
                Url = "http://ln.189.cn/group/integral/integral_integral.do";
                httpItem = new HttpItem()
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


                Url = "http://ln.189.cn/group/integral/infoQuery_queryintegralInfor.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (!string.IsNullOrEmpty(jsonParser.GetResultFromParser(httpResult.Html, "pointValue")))
                {
                    mobile.Integral = jsonParser.GetResultFromParser(httpResult.Html, "pointValue");  //积分
                }

                #endregion

                #region 月消费情况
                Url = "http://ln.189.cn/group/integral/integral_integral.do";
                httpItem = new HttpItem()
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


                //6个月账单
                for (int i = 0; i < 6; i++)
                {
                    date = date.AddMonths(-i);
                    Url = string.Format("http://ln.189.cn/chargeQuery/chargeQuery_queryCustBill.action?billingCycleId={1}&queryFlag=1&productId=8&accNbr={0}", mobileReq.Mobile, date.ToString("yyyyMM"));
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    infos = HtmlParser.GetResultFromParser(httpResult.Html, "//script");
                    if (infos.Count > 0)
                    {
                        var billInfo = infos[4].Split(';')[0].Split('=')[1];

                        billInfo = jsonParser.GetResultFromParser(billInfo, "BillQueryResponse");
                        billInfo = jsonParser.GetResultFromParser(billInfo, "billInfoGroup");
                        var billList = billInfo.Split('}');
                        if (billList.Count() > 0)
                        {
                            bill = new MonthBill();
                            foreach (var item in billList)
                            {
                                if (item.Contains("本项小计"))
                                {
                                    bill.TotalAmt = item.Split(',')[2].Split(':')[1].Replace("\"", "");
                                }

                                if (item.Contains("基本月租费"))
                                {
                                    bill.PlanAmt = item.Split(',')[2].Split(':')[1].Replace("\"", "");
                                }
                            }
                            bill.BillCycle = date.ToString(Consts.DateFormatString12);
                            mobile.BillList.Add(bill);
                        }
                    }

                }

                #endregion

                #region 注释内容
                //#region 详单查询

                //Url = "http://www.189.cn/dqmh/my189/checkMy189Session.do";
                //postdata = "fastcode=00380407";
                //httpItem = new HttpItem()
                //{
                //    URL = Url,
                //    Method = "POST",
                //    CookieCollection = cookies,
                //    Postdata = postdata,
                //    ResultCookieType = ResultCookieType.CookieCollection
                //};
                //httpResult = httpHelper.GetHtml(httpItem);
                //if (httpResult.StatusCode != HttpStatusCode.OK)
                //{
                //    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                //// 短信详单
                //GetDeatilsCall("2", "移动短信详单", mobileReq, mobile);

                //// 上网详单
                //GetDeatilsCall("3", "移动上网详单", mobileReq, mobile);

                //// 话费详单
                //GetDeatilsCall("1", "移动语音详单", mobileReq, mobile);

                //#endregion

                #endregion

                //保存
                MobileMongo mobileMongo = new MobileMongo(appDate);
                mobileMongo.SaveBasic(mobile);
                Res.StatusDescription = "辽宁电信手机账单抓取成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusDescription = "辽宁电信手机账单抓取异常";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteError("辽宁电信手机账单抓取异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 解析抓取的原始数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="crawlerDate"></param>
        /// <returns></returns>
        public override BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate)
        {
            throw new NotImplementedException();
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
