using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HL
{
    public class yichun : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.gjj.yc.gov.cn/";
        string fundCity = "hl_yichun";
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        ProvidentFundDetail detail = null;
        List<string> results = new List<string>();
        List<string> result = new List<string>();

        #endregion
        #region 私有变量
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        Regex reg = new Regex(@"[\s;\&nbsp;\,;\%]");
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
           
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //if (fundReq.Identitycard.IsEmpty())
                //{
                //    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (regex.IsMatch(fundReq.Identitycard) == false)
                {
                    Res.StatusDescription = "请输入有效的15位或18位身份证号";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                Url = baseUrl + "PersonLoginServlet";
                postdata = string.Format("gcode={0}", fundReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
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
                string errorMsg = CommonFun.GetMidStr(httpResult.Html, "<script>alert('", "');location='personal_searchnew.jsp';</script>");
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='main_con']/table/tbody/tr[2]/td/table/tbody", "inner");
              
                #endregion
                #region 第二步,获取基本信息
                result = HtmlParser.GetResultFromParser(results[0], "//tr[1]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.Name = result[0]; //姓名
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[1]/td[4]", "text");
                if (result.Count > 0)
                {
                    Res.EmployeeNo = result[0]; //职工账号
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[2]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.CompanyNo = result[0]; //单位账号
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[2]/td[4]", "text");
                if (result.Count > 0)
                {
                    Res.CompanyName = result[0]; //单位名称
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[3]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.IdentityCard = result[0]; //身份证号
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[3]/td[4]", "text");
                if (result.Count > 0)
                {
                    string datatime = result[0];
                    datatime = datatime.Insert(4, "-").Insert(7, "-");
                    Res.OpenTime = datatime; 
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[4]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.Status = result[0]; //账户状态
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[4]/td[4]", "text");
                if (result.Count > 0)
                {
                    Res.TotalAmount = result[0].ToDecimal(0); //当前余额
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[6]/td[4]", "text");
                if (result.Count > 0)
                {
                    Res.LastProvidentFundTime = result[0]; //单位缴至年月/员工缴至年月
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[7]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.CompanyMonthPayRate = result[0].ToDecimal(0); //单位缴率
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[7]/td[4]", "text");
                if (result.Count > 0)
                {
                    Res.PersonalMonthPayRate = result[0].ToDecimal(0); //个人缴率
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[8]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.CompanyMonthPayAmount = result[0].ToDecimal(0); //单位缴额
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[8]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.PersonalMonthPayAmount = result[0].ToDecimal(0); //职工缴额
                }
                result = HtmlParser.GetResultFromParser(results[0], "//tr[9]/td[2]", "text");
                if (result.Count > 0)
                {
                    Res.SalaryBase = result[0].ToDecimal(0); //工资基数
                }
                //result = HtmlParser.GetResultFromParser(results[0], "//tr[9]/td[4]", "text");
                //if (result.Count > 0)
                //{
                //    Res.MonthPayAmount = result[0].ToDecimal(0); //月缴存额
                //}

                
                #endregion
                #region 第三步，查询缴费明细
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='main_con']/table/tbody/tr[3]/td/table/thead/tbody/tr[position()>0]", "inner");
                foreach (string item in results)
                {
                    detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }
                    detail.Description = tdRow[2];//描述
                    detail.PayTime = tdRow[0].Insert(4,"-").Insert(7,"-").ToDateTime();//缴费年月
                    if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                    {
                        detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);//应属年月
                        detail.PersonalPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0) * perAccounting;//个人缴费金额
                        detail.CompanyPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0) * comAccounting;//企业缴费金额
                        detail.ProvidentFundBase = Math.Round(reg.Replace(tdRow[3], "").ToDecimal(0) / (totalRate * (decimal)0.01),2);//缴费基数
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                    }
                     else if (tdRow[2].IndexOf("个人补缴", System.StringComparison.Ordinal) > -1)
                     {
                         detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);//应属年月
                         detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //个人缴费金额
                         detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust; //缴费标志
                         detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                         detail.Description = tdRow[2];
                     }
                     else if (tdRow[2].IndexOf("结息") > -1)
                     {
                         detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0,6);
                         detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //个人缴费金额
                         detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust; //缴费标志
                         detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                         detail.Description = tdRow[2];
                     }
                     else if (tdRow[2].IndexOf("单位补缴", System.StringComparison.Ordinal) > -1)
                     {
                         detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);
                         detail.CompanyPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //单位缴费金额
                         detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal; //缴费标志
                         detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal; //缴费类型
                         detail.Description = tdRow[2];
                         PaymentMonths++;
                     }
                     else
                     {
//（补缴，结息etc，数据不精确，只做参考用）
                         detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //个人缴费金额
                         detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust; //缴费标志
                         detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                         detail.Description = tdRow[2];
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
        /// 处理缴费明细
        /// </summary>
        /// <param name="httpResult">http返回类</param>
        /// <returns>Res</returns>
        private Entity.Service.ProvidentFundQueryRes GetNowPageDetails(HttpResult httpResult)
        {
            results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='myTable']/tr[position()>1]", "inner");
            foreach (string item in results)
            {
                detail = new ProvidentFundDetail();
                List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                if (tdRow.Count != 5)
                {
                    continue;
                }
                detail.Description = tdRow[1];//描述
                detail.PayTime = tdRow[0].ToDateTime();//缴费年月
                if (tdRow[1].IndexOf("汇缴", System.StringComparison.Ordinal) > -1)
                {
                    detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);//应属年月
                    detail.PersonalPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0) ;//个人缴费金额
                    detail.CompanyPayAmount = reg.Replace(tdRow[3], "").ToDecimal(0) ;//企业缴费金额
                    detail.ProvidentFundBase = Math.Round(reg.Replace(tdRow[3], "").ToDecimal(0) / Res.PersonalMonthPayRate,2);//缴费基数
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                    PaymentMonths++;
                }
                else if (tdRow[2].IndexOf("个人补缴", System.StringComparison.Ordinal) > -1)
                {
                    detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);//应属年月
                    detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //个人缴费金额 //缴费基数
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust; //缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                    detail.Description = tdRow[2];
                }
                else if (tdRow[2].IndexOf("结息") > -1)
                {
                    detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);
                    detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //个人缴费金额

                    detail.ProvidentFundBase = reg.Replace(tdRow[1], "").ToDecimal(0) / Res.PersonalMonthPayRate *
                                               (decimal)0.01; //缴费基数
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust; //缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                    detail.Description = tdRow[2];
                }
                else if (tdRow[2].IndexOf("单位补缴", System.StringComparison.Ordinal) > -1)
                {
                    detail.ProvidentFundTime = new Regex(@"[^0-9;1-9]*").Replace(tdRow[0], "").Substring(0, 6);
                    detail.CompanyPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //单位缴费金额

                    detail.ProvidentFundBase = reg.Replace(tdRow[1], "").ToDecimal(0) / Res.CompanyMonthPayRate *
                                               (decimal)0.01; //缴费基数
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal; //缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal; //缴费类型
                    detail.Description = tdRow[2];
                    PaymentMonths++;
                }
                else
                {
                    //（补缴，结息etc，数据不精确，只做参考用）
                    detail.PersonalPayAmount = reg.Replace(tdRow[1], "").ToDecimal(0); //个人缴费金额
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust; //缴费标志
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust; //缴费类型
                    detail.Description = tdRow[2];
                }
                Res.ProvidentFundDetailList.Add(detail);
            }
            return Res;
        }
    }
}
