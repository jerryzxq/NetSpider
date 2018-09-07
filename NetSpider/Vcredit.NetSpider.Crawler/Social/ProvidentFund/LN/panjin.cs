using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.LN
{
    public class panjin : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.pjgjj.com/";
        string fundCity = "ln_panjin";
        int PaymentMonths = 0;
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        private Regex reg = new Regex(@"[\&nbsp;\s;\,;\%;\-;]*");
        decimal payRate = (decimal)0.08;
        List<string> results = new List<string>();
        string Url = string.Empty;
        string postdata = string.Empty;
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
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
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            List<string> results = new List<string>();
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
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                if (string.IsNullOrEmpty(fundReq.Identitycard) || !regex.IsMatch(fundReq.Identitycard))
                {
                    Res.StatusDescription = "身份证号不能为空或身份证号格式不正确";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登录
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                var errormsg = CommonFun.GetMidStr(httpResult.Html, "alert('", "');");
                if (!errormsg.IsEmpty() && !errormsg.Contains("盘锦"))
                {
                    Res.StatusDescription = errormsg;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                #endregion

                #region 第二步，获取公积金基本信息
                //第一步登录成功后已经获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@width='960']/tbody/tr[2]/td/table[@align='center']/tbody/tr/td", "");
                if (results.Count > 0)
                {
                    Res.Name = results[1];//姓名
                    Res.ProvidentFundNo = results[3];//职工账号
                    Res.CompanyNo = results[5];//单位账号
                    Res.Name = results[7];//单位名称
                    Res.IdentityCard = results[9];//身份证号
                    Res.ProvidentFundNo = results[11];//查询卡卡号
                    Res.Status = results[12];//状态
                    Res.TotalAmount = results[15].ToDecimal(0);//余额
                    Res.OpenTime = results[21];//开户日期
                    Res.LastProvidentFundTime = results[23];//最后缴至年月
                    Res.CompanyMonthPayRate = results[25].ToDecimal(0);  //单位缴费比例
                    Res.PersonalMonthPayRate = results[27].ToDecimal(0);  //个人缴费比例
                    Res.CompanyMonthPayAmount = results[29].ToDecimal(0);  //单位缴费
                    Res.PersonalMonthPayAmount = results[31].ToDecimal(0);  //个人缴费

                }
                #endregion

                #region 第三步,查询缴费明细
                //在上一步中可以直接获取详细表格总页数
                int pagecount = 0;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@cellspacing='5']/tbody/tr/td", "");
                if (results.Count > 0)
                {
                    pagecount = CommonFun.GetMidStr(results[0], "当前页：1/", "").ToInt(0);
                }
                for (int i = 1; i <= pagecount; i++)
                {
                    //http://www.pjgjj.com/PersonViewServlet?pageNumber=1
                    Url = baseUrl + string.Format("PersonViewServlet?pageNumber={0}", i);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='prtable']/thead/tbody/tr", "");
                    foreach (var item in results)
                    {
                        detail = new ProvidentFundDetail();
                        var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                        detail.PayTime = tdRow[0].ToDateTime(); //缴存时间
                        detail.Description = tdRow[2];  //摘要
                        detail.CompanyName = Res.CompanyName;
                        if (detail.Description.Contains("汇缴"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;  //缴费标志
                            detail.PaymentType= ServiceConsts.ProvidentFund_PaymentType_Normal;  //缴费类型
                            detail.PersonalPayAmount = tdRow[1].ToDecimal(0)/2;  //个人缴费
                            detail.CompanyPayAmount = tdRow[1].ToDecimal(0) / 2;  //单位缴费缴费
                            PaymentMonths++;
                        }
                        else if (detail.Description.Contains("提取"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                            detail.PersonalPayAmount = tdRow[1].ToDecimal(0) / 2;  //提取将缴费金额设为个人缴费
                            Res.Description = "有支取，请人工校验。";  //缴费详单存在支取
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = tdRow[1].ToDecimal(0) / 2;  //其他缴费类型将缴费金额设为个人缴费
                        }

                        Res.ProvidentFundDetailList.Add(detail);

                    }
                }

             
                #endregion

                #region  *******暂无贷款合同号无法查询贷款信息*******

                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
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
