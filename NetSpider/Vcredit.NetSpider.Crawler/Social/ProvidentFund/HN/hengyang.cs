using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web.UI;
using Vcredit.Common;
using Vcredit.Common.Ext;
using System.Collections;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using System.Web;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HN
{
    /// <summary>
    /// 未提供公积金明细查询,密码为: 1' or '1' = '1(最前面的1之前有个空格)
    /// </summary>
    public class hengyang : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://219.72.229.198:8080/wscx/zfbzgl/zfbzsq/";
        string fundCity = "hn_hengyang";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "所选城市无需初始化";
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            List<string> results = new List<string>();
            Res.ProvidentFundCity = fundCity;
            string url = string.Empty;
            ProvidentFundDetail detail = null;
            string postData = string.Empty;
            decimal payRate = (decimal)0.08;
            int PaymentMonths = 0;

            #region 定义变量
            var zgzh = string.Empty;
            var sfzh = string.Empty;
            var zgxm = string.Empty;
            var dwbm = string.Empty;
            var zgzt = string.Empty;
            var providentfundBase = string.Empty;
            #endregion
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Identitycard.IsEmpty() )
                {
                    Res.StatusDescription = "请输入身份证号码！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "请输入密码！";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,验证登陆
                url = baseUrl + string.Format("login_hidden.jsp?password={1}&sfzh={0}&cxyd=&dbname=wasys350_hnhy_kf&dlfs=0", fundReq.Identitycard, fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var errormsg = CommonFun.GetMidStr(httpResult.Html,"alert(\"","\");");
                if (!string.IsNullOrEmpty(errormsg))
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_error;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryError;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                var list = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr/td/input", "value");
                if (list.Count > 0)
                {
                    zgzh = list[0];
                    sfzh = list[1];
                    zgxm = list[2];
                    dwbm = list[3];
                    zgzt = list[4];
                }

                #endregion

                #region 第二步,获取基本信息
                url = baseUrl + string.Format("main_menu.jsp?zgzh={0}&sfzh={1}&passtwo={2}&flagp=0&zgxm={3}&dwbm={4}&zgzt={5}&cxyd=&dbname=wasys350_hnhy_kf", zgzh, sfzh, fundReq.Password, zgxm.ToUrlEncode(), dwbm, zgzt.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = url,
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='980']/tr/td[@align='left']", "inner");
                if (results.Count > 0)
                {

                    Res.Name = results[0].Split(';')[1];  //职工姓名
                    Res.BankCardNo = results[1].Split(';')[1];  //银行账号
                    Res.IdentityCard = results[2].Split(';')[1];  //身份证
                    Res.EmployeeNo = results[3].Split(';')[1];  //职工账号
                    Res.CompanyName = results[4].Split(';')[1];  //所在单位
                    Res.OpenTime = results[6].Split(';')[1];  //开户时间
                    Res.Status = results[7].Split(';')[1];  //当前状态
                    providentfundBase = results[8].Split(';')[1].TrimEnd('元');  //缴费基数
                    Res.PersonalMonthPayRate = results[9].Split(';')[1].Split('/')[0].TrimEnd('%').ToDecimal(0);  //个人缴存比例
                    Res.CompanyMonthPayRate = results[9].Split(';')[1].Split('/')[1].TrimEnd('%').ToDecimal(0);  //公司缴存比例
                    Res.CompanyMonthPayAmount = results[13].Split(';')[1].TrimEnd('元').ToDecimal(0);  //公司月缴费
                    Res.PersonalMonthPayAmount = results[16].Split(';')[1].TrimEnd('元').ToDecimal(0);  //个人月缴费
                    Res.TotalAmount = results[20].Split(';')[1].TrimEnd('元').ToDecimal(0);  //账户余额
                    Res.LastProvidentFundTime = results[22].Split(';')[1];  //账户余额
                    Res.SalaryBase = Math.Round(Res.PersonalMonthPayAmount / (Res.PersonalMonthPayRate / 100));          //薪资

                }
                #endregion


                #region ===第三步，获取详细信息===
                url = "http://219.72.229.198:8080/wscx/zfbzgl/gjjmxcx/gjjmx_cx.jsp?" + string.Format("sfzh={0}&zgxm={1}&zgzh={2}&dwbm={3}&cxyd=&zgzt={4}&passs={5}", sfzh, zgxm.ToUrlEncode(), zgzh, dwbm, zgzt.ToUrlEncode(), fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //获取年份
                var yearList = HtmlParser.GetResultFromParser(httpResult.Html, "//option", "");
                yearList.RemoveAt(yearList.Count - 1);

                foreach (var item in yearList) //循环年份
                {
                    for (int i = 1; i <= 3; i++)  //循环列表页数（最大为三页）
                    {
                        url = "http://219.72.229.198:8080/wscx/zfbzgl/gjjmxcx/gjjmx_cx.jsp?" + string.Format("cxydone={0}&cxydtwo={1}&yss={2}&totalpages=3&cxyd=&zgzh={3}&sfzh={4}&zgxm={5}&dwbm={6}&dbname=wasys350_hnhy_kf", item, item, i, zgzh, sfzh, zgxm.ToUrlEncode(), dwbm);
                        httpItem = new HttpItem()
                        {
                            URL = url,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);

                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//tr[@class='jtpsofta']", "");
                        foreach (var infoString in results)  //循环缴费条数
                        {
                            detail = new ProvidentFundDetail();
                            var tdRow = HtmlParser.GetResultFromParser(infoString, "//td", "text", true);
                            if (tdRow.Count != 5)
                            {
                                continue;
                            }
                            detail.PayTime = tdRow[0].ToDateTime();  //缴费年月
                            detail.Description = tdRow[4];  //描述
                            if (tdRow[4].Contains("汇缴"))
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                                detail.PersonalPayAmount = tdRow[2].ToDecimal(0) / 2;//个人缴款
                                detail.CompanyPayAmount = tdRow[2].ToDecimal(0) / 2;//单位缴款
                                detail.ProvidentFundBase = providentfundBase.ToDecimal(0);//缴费基数
                                detail.CompanyName = Res.CompanyName;  //单位名称
                                PaymentMonths++;
                            }
                            else if (tdRow[4].Contains("提取") || tdRow[4].Contains("支取"))
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                                detail.PersonalPayAmount = tdRow[2].ToDecimal(0) / 2;//个人缴款
                                detail.CompanyName = Res.CompanyName;  //单位名称
                                Res.Description = "有支取，请人工校验。";
                            }
                            else
                            {
                                detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                                detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                                detail.PersonalPayAmount = tdRow[2].ToDecimal(0);
                            }

                            Res.ProvidentFundDetailList.Add(detail);
                        }
                    }
                }
                #endregion


                #region 第四步，贷款信息  *******所给账号暂无贷款信息*******

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

        /// <summary>
        /// 格式化时间戳
        /// </summary>
        private static string ReplaceString(string str, int a, int b)
        {
            List<string> oldStr = str.Split(' ').ToList();
            List<string> newStr = str.Split(' ').ToList();
            newStr[a] = oldStr[b];
            newStr[b] = oldStr[a];
            string s1 = string.Join("%20", newStr.ToArray());
            return s1;
        }
    }
}
