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

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SN
{
    public class xian : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.xazfgjj.gov.cn/";
        string fundCity = "sn_xian";
        #endregion

        #region 该城市临时变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string EmployeeNo = string.Empty;
        string IdentityCard = string.Empty;
        string Name = string.Empty;
        string Url = string.Empty;
        string postdata = string.Empty;
        List<string> results = new List<string>();
        int PaymentMonths = 0;
        string errorMsg = string.Empty;
        decimal PesonPayRate = 0;//个人缴费比率
        decimal ComPayRate = 0;//公司缴费比率
        #endregion

        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = baseUrl + "system/resource/creategjjcheckimg.jsp";
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
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;

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

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;

            ProvidentFundDetail detail = null;

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() || fundReq.Name.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "姓名、身份证号或密码不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }


                #region  第一步 登陆
                Url = baseUrl + "gjjcx_dl.jsp?urltype=tree.TreeTempUrl&wbtreeid=1172";
                //postdata=string.Format("csrftoken=40257&wbidcard=610102199001131575&cxydmc=%E5%BD%93%E5%89%8D%E5%B9%B4%E5%BA%A6&flag=login&wbzhigongname=%E7%8E%8B%E8%B7%83%E5%87%AF&wbrealmima=111111&wbmima=111111&surveyyanzheng={0}&x=80&y=22",fundReq.Vercode);
                postdata = string.Format("csrftoken=40257&wbidcard={0}&cxydmc=%E5%BD%93%E5%89%8D%E5%B9%B4%E5%BA%A6&flag=login&wbzhigongname={1}&wbrealmima={2}&wbmima={3}&surveyyanzheng={4}&x={5}&y={6}", fundReq.Identitycard, fundReq.Name.Trim().ToUrlEncode(), fundReq.Password, fundReq.Password, fundReq.Vercode.ToUrlEncode(Encoding.GetEncoding("utf-8")), DateTime.Now.Millisecond, DateTime.Now.Second);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                errorMsg = CommonFun.GetMidStr(httpResult.Html, "JavaScript'>alert('", "');");
                if (!string.IsNullOrEmpty(errorMsg) && !errorMsg.Contains("<td>"))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第二步  公积金信息查询
                Url = baseUrl + "gjjcx_gjjxxcx.jsp?urltype=tree.TreeTempUrl&wbtreeid=1178";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='divz']//tr[position()>3]/td", "", true);
                if (results.Count < 38)
                {
                    Res.StatusDescription = "无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                Res.Name = results[1];
                Res.ProvidentFundNo = results[3];
                Res.IdentityCard = results[5];
                Res.SalaryBase = results[7].ToDecimal(0);
                Res.CompanyNo = results[9];
                Res.CompanyName = results[13];
                Res.Bank = results[15];
                Res.OpenTime = results[17];
                Res.Status = results[19];
                Res.PersonalMonthPayRate = results[21].Replace("%", "").ToDecimal(0) / 100 > 0 ? results[21].Replace("%", "").ToDecimal(0) / 100 : payRate;
                Res.CompanyMonthPayRate = results[23].Replace("%", "").ToDecimal(0) / 100 > 0 ? results[23].Replace("%", "").ToDecimal(0) / 100 : payRate;
                Res.PersonalMonthPayAmount = Res.SalaryBase * Res.PersonalMonthPayRate;
                Res.CompanyMonthPayAmount = Res.SalaryBase * Res.CompanyMonthPayRate;
                Res.TotalAmount = results[37].ToDecimal(0);
                string[] idcard = Res.IdentityCard.Split('*');
                if (fundReq.Name.Contains(Res.Name.Replace("*", "")) && fundReq.Identitycard.Contains(idcard[0]) && fundReq.Identitycard.Contains(idcard[idcard.Length - 1]))
                {
                    Res.Name = fundReq.Name;
                    Res.IdentityCard = fundReq.Identitycard;
                }
                #endregion

                #region 获取公积金明细
                Url = baseUrl + "gjjcx_gjjmxcx.jsp?urltype=tree.TreeTempUrl&wbtreeid=1177";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='divz']/table/tr[position()>3]", "");
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 4)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.PayTime = DateTime.Parse(HtmlParser.GetResultFromParser(tdRow[0], "//strong", "")[0].ToString());
                    if (tdRow[1].IndexOf("汇缴") > -1)
                    {
                        detail.ProvidentFundTime = detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "汇缴", "公积金"); ;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = (tdRow[2].ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.PersonalMonthPayRate;//金额
                        detail.CompanyPayAmount = (tdRow[2].ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate)) * Res.CompanyMonthPayRate; ;//金额

                        detail.ProvidentFundBase = (tdRow[3].ToDecimal(0) / (Res.PersonalMonthPayRate + Res.CompanyMonthPayRate));//缴费基数
                        PaymentMonths++;
                    }
                    else if (tdRow[1].IndexOf("补缴") > -1)
                    {
                        detail.ProvidentFundTime = detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "补缴", "公积金"); ;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0);
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];

                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                Res.PaymentMonths = PaymentMonths;
                #endregion
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
    }
}
