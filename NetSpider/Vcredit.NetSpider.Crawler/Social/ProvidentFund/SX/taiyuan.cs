using System;
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
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SX
{
    public class taiyuan : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.tygjj.com:8080/wt-web/";
        string fundCity = "sx_taiyuan";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string cxyd = string.Empty;
        string cxydmc = string.Empty;
        string dbname = string.Empty;
        string _item = string.Empty;
        string EmployeeNo = string.Empty;
        string IdentityCard = string.Empty;
        string Name = string.Empty;
        string Url = string.Empty;
        string postdata = string.Empty;
        List<string> results = new List<string>();
        int PaymentMonths = 0;
        private decimal payRate = (decimal)0.08;
        private Regex reg = new Regex(@"[\s;\t;\&nbsp;\,;\u5143]*");
        decimal perAccounting;//个人占比
        decimal comAccounting;//公司占比
        decimal totalRate;//总缴费比率
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            Res.Token = token;
            try
            {

                httpItem = new HttpItem()
                {
                    URL = "http://www.tygjj.com:8080/wt-web/login",
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "captcha";
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
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入正确的15位或18位身份证号码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                //获取加密信息
                Url = baseUrl + "login";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//0089ec3a334550b561257dc41dc15e749d
                var modulus = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='modulus']", "value")[0];
                var exponent = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='exponent']", "value")[0];
                var password = RSAHelper.EncryptStringByRsaJS(fundReq.Password, modulus, exponent, "20");

                Url = baseUrl + "login";
                postdata = string.Format("username={0}&password={1}&captcha={2}&modulus={3}&exponent={4}", fundReq.Identitycard, password, fundReq.Vercode, modulus, exponent);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Allowautoredirect = false,
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errorMsg = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@id='error']/i", "inner");
                if (errorMsg.Count > 0)
                {
                    Res.StatusDescription = errorMsg[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollectionEQ(cookies, httpResult.CookieCollection);

                Url = "http://www.tygjj.com:8080/wt-web/home";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    Referer = "http://www.tygjj.com:8080/wt-web/logout",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                #endregion

                #region 基本信息
                //获取个人基本信息
                Url = "http://www.tygjj.com:8080/wt-web/person/jbxx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Host = "www.tygjj.com:8080",
                    Referer = "http://www.tygjj.com:8080/wt-web/home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var infoList = jsonParser.GetArrayFromParse(httpResult.Html, "results");
                if (infoList.Count > 0)
                {
                    Res.Name = jsonParser.GetResultFromParser(infoList[0], "a002");  //姓名
                    Res.EmployeeNo = jsonParser.GetResultFromParser(infoList[0], "a001");  //雇员编号
                    Res.IdentityCard = jsonParser.GetResultFromParser(infoList[0], "a008");  //身份证号
                    Res.CompanyNo = jsonParser.GetResultFromParser(infoList[0], "a003");  //公司编号
                    Res.CompanyName = jsonParser.GetResultFromParser(infoList[0], "a004");  //公司编号
                    Res.OpenTime = jsonParser.GetResultFromParser(infoList[0], "a013");  //开户日期
                    Res.Phone = jsonParser.GetResultFromParser(infoList[0], "yddh");  //电话
                    Res.BankCardNo = jsonParser.GetResultFromParser(infoList[0], "yhkh");  //银行卡号
                    Res.Status = jsonParser.GetResultFromParser(infoList[0], "mc");  //状态
                }

                //获取账户基本信息
                Url = "http://www.tygjj.com:8080/wt-web/person/jcqqxx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Host = "www.tygjj.com:8080",
                    Referer = "http://www.tygjj.com:8080/wt-web/home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var infoSting = jsonParser.GetResultFromParser(httpResult.Html, "data");
                if (!string.IsNullOrEmpty(infoSting))
                {
                    Res.SalaryBase = jsonParser.GetResultFromParser(infoSting, "a011").ToDecimal(0);  //基本薪资
                    Res.TotalAmount = jsonParser.GetResultFromParser(infoSting, "a044").ToDecimal(0);  //账户总额

                    var bl = jsonParser.GetResultFromParser(infoSting, "a097");  //缴费比例
                    var companybl = CommonFun.GetMidStr(bl, "单位比例", "个人比例").ToDecimal(0);
                    var personbl = CommonFun.GetMidStr(bl, "个人比例", "").ToDecimal(0);
                    Res.PersonalMonthPayRate = personbl / (companybl + personbl);  //个人月缴费比例
                    Res.CompanyMonthPayRate = companybl / (companybl + personbl);  //单位月缴费比例
                    Res.PersonalMonthPayAmount = jsonParser.GetResultFromParser(infoSting, "a034").ToDecimal(0);  //个人月缴费
                    Res.CompanyMonthPayAmount = jsonParser.GetResultFromParser(infoSting, "a035").ToDecimal(0);  //单位月缴费
                }
                #endregion

                #region 获取详细信息(只能查询三年之内)
                var nowdate = DateTime.Now.ToString("yyyy-MM-dd");
                var olddate = DateTime.Now.AddYears(-3).ToString("yyyy-MM-dd");
                Url = string.Format("http://www.tygjj.com:8080/wt-web/personal/jcmxlist?beginDate={1}&endDate={0}&UserId=1&pageNum=1&pageSize=100&_=1456364967776", nowdate, olddate);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Host = "www.tygjj.com:8080",
                    Referer = "http://www.tygjj.com:8080/wt-web/home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var detailList = jsonParser.GetArrayFromParse(httpResult.Html, "results");

                foreach (var item in detailList)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();

                    var dataobj = JObject.Parse(item);

                    if (dataobj.Count > 0)
                    {
                        detail.Description = dataobj["zy"].ToString();//描述
                        detail.PayTime = dataobj["rq"].ToString().ToDateTime();//缴费年月
                        if (dataobj["zy"].ToString().Trim().IndexOf("汇缴", StringComparison.Ordinal) > -1)
                        {
                            detail.ProvidentFundTime = Regex.Replace(dataobj["zy"].ToString(), @"[^\d]*", ""); //应属年月
                            detail.CompanyName = Res.CompanyName;  //单位名称
                            detail.PersonalPayAmount = dataobj["dfje"].ToString().ToDecimal(0) * Res.PersonalMonthPayRate;  //个人缴费
                            detail.CompanyPayAmount = dataobj["dfje"].ToString().ToDecimal(0) * Res.CompanyMonthPayRate;  //公司缴费
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal; //缴费类型
                            PaymentMonths++;
                        }
                        else if (dataobj["zy"].ToString().Trim().IndexOf("提取", StringComparison.Ordinal) > -1 || dataobj["zy"].ToString().Trim().IndexOf("支取", StringComparison.Ordinal) > -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw; //缴费类型
                            Res.Description = "有支取，请人工校验。";
                        }
                        else//（补缴，结息etc，数据不精确，只做参考用）
                        {
                            detail.PersonalPayAmount = dataobj["dfje"].ToString().ToDecimal(0);//个人缴费金额
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                        }

                    }

                    Res.ProvidentFundDetailList.Add(detail);
                }

                #endregion


                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        /// <summary>
        /// 处理该年度数据
        /// </summary>
        /// <param name="_HttpResult">该年度首页数据</param>
        /// <param name="totalPages">该年度总数据页数</param>
        /// <returns></returns>
        private Entity.Service.ProvidentFundQueryRes ProcessingInfo(HttpResult _HttpResult, int totalPages)
        {
            if (_HttpResult != null && totalPages > 0)
            {
                results = HtmlParser.GetResultFromParser(_HttpResult.Html, "//table[2]/tr[1]/td[2]/table[4]/tr[1]/td/table/tr[position()>1]", "inner");
                if (results.Count > 0)
                {
                    SaveInfo(results);
                }
            }
            if (totalPages > 1)
            {
                //请求剩余页码数据
                for (int i = 2; i <= totalPages; i++)
                {
                    Url = baseUrl + "zfbzgl/gjjmxcx/gjjmx_cx.jsp";
                    postdata = string.Format("cxydone={0}&cxydtwo={1}&yss={6}&totalpages={7}&cxyd=null&zgzh={2}&sfzh={3}&zgxm={4}&dwbm=null&dbname={5}", _item, _item, EmployeeNo, IdentityCard, Name.ToUrlEncode(Encoding.GetEncoding("GBK")), dbname, i, totalPages);
                    httpResult = HttpResult(Url, postdata, "Post", "gbk", cookies);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[1]/td[2]/table[4]/tr[1]/td/table/tr[position()>1]", "inner");
                    if (results.Count > 0)
                    {
                        SaveInfo(results);
                    }
                }

            }
            return Res;
        }
        /// <summary>
        /// 保存明细数据
        /// </summary>
        /// <param name="_Results">获得的公积金明细列表</param>
        private void SaveInfo(List<string> _Results)
        {
            foreach (var item in _Results)
            {
                ProvidentFundDetail detail = new ProvidentFundDetail();
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                if (tdRow.Count != 6)
                {
                    continue;
                }
                detail.Description = tdRow[5];//描述
                if (tdRow[5].Trim().IndexOf("上年", StringComparison.Ordinal) < 0 && tdRow[5].Trim().IndexOf("本年", StringComparison.Ordinal) < 0)
                {
                    detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                }
                if (tdRow[5].Trim().IndexOf("汇缴", StringComparison.Ordinal) > -1)
                {
                    detail.ProvidentFundTime = Regex.Replace(tdRow[5], @"[^\d]*", ""); ;//应属年月
                    detail.PersonalPayAmount = (reg.Replace(tdRow[2], "").ToDecimal(0) * perAccounting).ToString("f2").ToDecimal(0);//个人缴费金额
                    detail.CompanyPayAmount = (reg.Replace(tdRow[2], "").ToDecimal(0) * comAccounting).ToString("f2").ToDecimal(0);//企业缴费金额
                    detail.ProvidentFundBase = (reg.Replace(tdRow[2], "").ToDecimal(0) / (totalRate)).ToString("f2").ToDecimal(0);//基本薪资
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal; //缴费类型
                    PaymentMonths++;
                }
                else
                {//（补缴，结息etc，数据不精确，只做参考用）
                    detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0) + reg.Replace(tdRow[2], "").ToDecimal(0);//个人缴费金额
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                }
                Res.ProvidentFundDetailList.Add(detail);
            }
        }
        /// <summary>
        /// 页面请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postdata">提交数据</param>
        /// <param name="method">请求方法</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="cookies">cookies</param>
        /// <returns>Http返回参数</returns>
        private Vcredit.Common.Helper.HttpResult HttpResult(string url, string postdata, string method, string encoding, CookieCollection cookies)
        {
            httpItem = new HttpItem()
            {
                URL = url,
                Method = method,
                Postdata = postdata,
                Encoding = Encoding.GetEncoding(encoding),
                CookieCollection = cookies,
                ResultCookieType = ResultCookieType.CookieCollection
            };
            httpResult = httpHelper.GetHtml(httpItem);
            return httpResult;
        }
    }
}
