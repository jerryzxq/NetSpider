using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.Common.Constants;
using Newtonsoft.Json.Linq;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.LN
{

    public class dalian : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://bg.gjj.dl.gov.cn/";
        string fundCity = "ln_dalian";
        #endregion
        #region 私有变量
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        List<string> results = new List<string>();
        private decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        string Url = string.Empty;
        string postdata = string.Empty;
        private Regex reg = new Regex(@"[\s;\,\&nbsp;\-;]");
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://bg.gjj.dl.gov.cn/person/logon/showValidImage.act?t=Fri%20Mar%2011%202016%2011:13:02%20GMT+0800";//验证码地址
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
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "联名卡号或密码为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (!new Regex(@"[0-9][1-9]*").IsMatch(fundReq.Password))
                {
                    Res.StatusDescription = "密码只能为数字";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                Url = baseUrl + "person/logon/logon.act";
                postdata = string.Format("usercode={0}&passwd={1}&validimage={2}&_remember=", fundReq.Username, fundReq.Password, fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 第二步,查询基本信息
                //账号基本信息
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "person/logon/showHomePage.act";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.Contains("error"))
                {
                    Res.StatusDescription = "登陆失败,请重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='x_content']/div[@class='row']/div[@class='col-xs-12 col-sm-4 col-md-4']/div", "");
                if (results.Count > 0)
                {
                    Res.Name = results[0].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");  //姓名
                    Res.Sex = results[1].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");  //性别
                    Res.CompanyName = results[4].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Split('<')[0];  //公司名称
                    Res.EmployeeNo = results[5].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");  //个人账号
                    Res.CompanyNo = results[9].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");  //单位账号
                    Res.IdentityCard = results[11].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");  //身份证号
                    Res.Phone = results[12].Split('：')[1].Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");  //手机号码
                }

                //缴存基本信息
                Url = baseUrl + "person/C0151/list.act";
                postdata = string.Format("zjhm1={0}", Res.IdentityCard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.Contains("error"))
                {
                    Res.StatusDescription = "系统加载详细信息失败,请您稍后登陆重试";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='ssyhmc']", "value");
                if (results.Count > 0)
                {
                    Res.Bank = results[0];  //所属银行
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lmkh']", "value");
                if (results.Count > 0)
                {
                    Res.BankCardNo = results[0];  //银行卡号
                }


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='grzhztms']", "value");
                if (results.Count > 0)
                {
                    Res.Status = results[0];  //个人状态
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='khrq']", "value");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0];  //开户日期
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='grbl']", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToDecimal(0);  //个人缴费比例
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwbl']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToDecimal(0);  //单位缴费比例
                }

                var totalPayRate = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='blzj']", "value")[0].ToDecimal(0);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='yhjehj']", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = (Res.PersonalMonthPayRate / totalPayRate) * results[0].ToDecimal(0);  //个人缴费
                    Res.CompanyMonthPayAmount = (Res.CompanyMonthPayRate / totalPayRate) * results[0].ToDecimal(0);  //公司缴费
                }

                Res.SalaryBase = Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate;  //基本薪资


                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dwzhhjy']", "value");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];  //最后汇缴时间
                }

                #endregion

                #region 第三步,公积金缴费明细(设置为每次查询50000条数据，并且设置查询起始日期为1990年)
                Url = baseUrl + "person/C0165/list.act";
                var year = DateTime.Now.Year;
                var month = DateTime.Now.Month;
                var day = DateTime.Now.Day;
                postdata = string.Format("_gt_json=%7B%22recordType%22%3A%22object%22%2C%22pageInfo%22%3A%7B%22pageSize%22%3A50000%2C%22pageNum%22%3A1%2C%22totalRowNum%22%3A-1%2C%22totalPageNum%22%3A0%2C%22startRowNum%22%3A1%2C%22endRowNum%22%3A0%7D%2C%22columnInfo%22%3A%5B%7B%22id%22%3A%22jyrq%22%2C%22header%22%3A%22%E4%BA%A4%E6%98%93%E6%97%A5%E6%9C%9F%22%2C%22fieldName%22%3A%22jyrq%22%2C%22fieldIndex%22%3A%22jyrq%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22jyms%22%2C%22header%22%3A%22%E6%91%98%E8%A6%81%22%2C%22fieldName%22%3A%22jyms%22%2C%22fieldIndex%22%3A%22jyms%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22jffse%22%2C%22header%22%3A%22%E5%80%9F%E6%96%B9%E5%8F%91%E7%94%9F%E9%A2%9D%22%2C%22fieldName%22%3A%22jffse%22%2C%22fieldIndex%22%3A%22jffse%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22dffse%22%2C%22header%22%3A%22%E8%B4%B7%E6%96%B9%E5%8F%91%E7%94%9F%E9%A2%9D%22%2C%22fieldName%22%3A%22dffse%22%2C%22fieldIndex%22%3A%22dffse%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22yue%22%2C%22header%22%3A%22%E4%BD%99%E9%A2%9D%22%2C%22fieldName%22%3A%22yue%22%2C%22fieldIndex%22%3A%22yue%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22dwmc1%22%2C%22header%22%3A%22%E5%8D%95%E4%BD%8D%E5%90%8D%E7%A7%B0%22%2C%22fieldName%22%3A%22dwmc1%22%2C%22fieldIndex%22%3A%22dwmc1%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22qsrq%22%2C%22header%22%3A%22%E8%B5%B7%E5%A7%8B%E6%97%A5%E6%9C%9F%22%2C%22fieldName%22%3A%22qsrq%22%2C%22fieldIndex%22%3A%22qsrq%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22zzrq%22%2C%22header%22%3A%22%E7%BB%88%E6%AD%A2%E6%97%A5%E6%9C%9F%22%2C%22fieldName%22%3A%22zzrq%22%2C%22fieldIndex%22%3A%22zzrq%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22jylxms%22%2C%22header%22%3A%22%E4%BA%A4%E6%98%93%E7%8A%B6%E6%80%81%22%2C%22fieldName%22%3A%22jylxms%22%2C%22fieldIndex%22%3A%22jylxms%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22qdlyms%22%2C%22header%22%3A%22%E5%8A%9E%E7%90%86%E6%96%B9%E5%BC%8F%22%2C%22fieldName%22%3A%22qdlyms%22%2C%22fieldIndex%22%3A%22qdlyms%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%5D%2C%22sortInfo%22%3A%5B%5D%2C%22filterInfo%22%3A%5B%5D%2C%22remotePaging%22%3Atrue%2C%22remoteSort%22%3Afalse%2C%22parameters%22%3A%7B%22grzh%22%3A%22113157853533%22%2C%22gjjzy%22%3A%22%22%2C%22qsrq%22%3A%221990%5Cu002d01%5Cu002d01%22%2C%22zzrq%22%3A%22{0}%5Cu002d{1}%5Cu002d{2}%22%7D%2C%22action%22%3A%22load%22%7D", year, month, day);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    ContentType = "application/x-www-form-urlencoded",
                    Accept = "text/javascript, text/html, application/xml,application/json, text/xml, */*",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                var list = jsonParser.GetArrayFromParse(httpResult.Html, "data");
                foreach (var item in list)
                {
                    var tdRow = JObject.Parse(item);
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    if (tdRow.Count > 0)
                    {
                        detail.Description = tdRow["jyms"].ToString();  //描述
                        detail.PayTime = tdRow["jyrq"].ToString().ToDateTime();  //交易日期
                        detail.CompanyName = tdRow["dwmc1"].ToString();  //单位名称
                        if (detail.Description.Contains("汇缴"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = tdRow["dffse"].ToString().ToDecimal(0) * (Res.PersonalMonthPayRate / totalPayRate);  //个人缴费
                            detail.CompanyPayAmount = tdRow["dffse"].ToString().ToDecimal(0) * (Res.CompanyMonthPayRate / totalPayRate);  //单位缴费
                            PaymentMonths++;
                        }
                        else if (detail.Description.Contains("提取"))
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Draw; ;
                            detail.PersonalPayAmount = tdRow["dffse"].ToString().ToDecimal(0) * (Res.PersonalMonthPayRate / totalPayRate);  //个人缴费
                            Res.Description = "有支取，请人工校验。";
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.PersonalPayAmount = tdRow["dffse"].ToString().ToDecimal(0);  //其他方式将公积金发生额当作个人缴费存入表中
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }

                #endregion

                #region 第四步，查询贷款基本信息
                ProvidentFundLoanRes Res_Loan = new ProvidentFundLoanRes();
                Url = "http://bg.gjj.dl.gov.cn/person/C0156/jkhthlist.act";
                postdata = "_gt_json=%7B%22recordType%22%3A%22object%22%2C%22pageInfo%22%3A%7B%22pageSize%22%3A10%2C%22pageNum%22%3A1%2C%22totalRowNum%22%3A-1%2C%22totalPageNum%22%3A0%2C%22startRowNum%22%3A1%2C%22endRowNum%22%3A0%7D%2C%22columnInfo%22%3A%5B%7B%22id%22%3A%22id%22%2C%22header%22%3A%22id%22%2C%22fieldName%22%3A%22id%22%2C%22fieldIndex%22%3A%22id%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Atrue%2C%22exportable%22%3Afalse%2C%22printable%22%3Afalse%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22jkhtbh%22%2C%22header%22%3A%22%E5%80%9F%E6%AC%BE%E5%90%88%E5%90%8C%E5%8F%B7%22%2C%22fieldName%22%3A%22jkhtbh%22%2C%22fieldIndex%22%3A%22jkhtbh%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22opt%22%2C%22header%22%3A%22%E6%93%8D%E4%BD%9C%22%2C%22fieldName%22%3A%22opt%22%2C%22fieldIndex%22%3A%22opt%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Afalse%2C%22printable%22%3Afalse%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%5D%2C%22sortInfo%22%3A%5B%5D%2C%22filterInfo%22%3A%5B%5D%2C%22remotePaging%22%3Atrue%2C%22remoteSort%22%3Afalse%2C%22parameters%22%3A%7B%22issearch%22%3A%22%22%2C%22lmkkh%22%3A%22" + Res.BankCardNo + "%22%7D%2C%22action%22%3A%22load%22%7D";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                Url = baseUrl + "person/C0156/getpage.act?jkhth=0";
                postdata = string.Format("issearch=&lmkkh={0}", Res.BankCardNo);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Res_Loan.ProvidentFundNo = Res.BankCardNo;
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='jkhth']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Con_No = results[0];  //合同编号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='jkr2xm']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Con_No = results[0];  //姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='jkr1xm']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Con_No = results[0];  //配偶姓名
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='fkrq']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Start_Date = results[0];  //放款日期（暂设为贷款开始日期）
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dked']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Credit = results[0].ToDecimal(0);  //贷款额度
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dkye']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Loan_Balance = results[0].ToDecimal(0);  //贷款余额
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='gfdz']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.House_Purchase_Address = results[0];  //购房地址
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dkqx']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Period_Total = results[0];  //贷款期限
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='yhhkzh']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Account_Repay = results[0];  //银行还款账号
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='dsrdkbzms']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Description = results[0];  //描述
                }
                #endregion


                #region 第五步，查询贷款详细信息(官网只能查询近一年还款情况)
                Url = baseUrl + "person/C0157/list.act";
                postdata = "_gt_json=%7B%22recordType%22%3A%22object%22%2C%22pageInfo%22%3A%7B%22pageSize%22%3A10%2C%22pageNum%22%3A1%2C%22totalRowNum%22%3A-1%2C%22totalPageNum%22%3A0%2C%22startRowNum%22%3A1%2C%22endRowNum%22%3A0%7D%2C%22columnInfo%22%3A%5B%7B%22id%22%3A%22id%22%2C%22header%22%3A%22id%22%2C%22fieldName%22%3A%22id%22%2C%22fieldIndex%22%3A%22id%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Atrue%2C%22exportable%22%3Afalse%2C%22printable%22%3Afalse%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22jkhth%22%2C%22header%22%3A%22%E5%80%9F%E6%AC%BE%E5%90%88%E5%90%8C%E5%8F%B7%22%2C%22fieldName%22%3A%22jkhth%22%2C%22fieldIndex%22%3A%22jkhth%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22jkr1xm%22%2C%22header%22%3A%22%E7%AC%AC%E4%B8%80%E5%80%9F%E6%AC%BE%E4%BA%BA%E5%A7%93%E5%90%8D%22%2C%22fieldName%22%3A%22jkr1xm%22%2C%22fieldIndex%22%3A%22jkr1xm%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22gydkje%22%2C%22header%22%3A%22%E8%B4%B7%E6%AC%BE%E9%87%91%E9%A2%9D%22%2C%22fieldName%22%3A%22gydkje%22%2C%22fieldIndex%22%3A%22gydkje%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22dkye%22%2C%22header%22%3A%22%E8%B4%B7%E6%AC%BE%E4%BD%99%E9%A2%9D%22%2C%22fieldName%22%3A%22dkye%22%2C%22fieldIndex%22%3A%22dkye%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22dkffrq%22%2C%22header%22%3A%22%E5%80%9F%E6%AC%BE%E8%B5%B7%E5%A7%8B%E6%97%A5%E6%9C%9F%22%2C%22fieldName%22%3A%22dkffrq%22%2C%22fieldIndex%22%3A%22dkffrq%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22spnx%22%2C%22header%22%3A%22%E5%80%9F%E6%AC%BE%E6%9C%9F%E9%99%90%22%2C%22fieldName%22%3A%22spnx%22%2C%22fieldIndex%22%3A%22spnx%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22opt%22%2C%22header%22%3A%22%E6%93%8D%E4%BD%9C%22%2C%22fieldName%22%3A%22opt%22%2C%22fieldIndex%22%3A%22opt%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Afalse%2C%22printable%22%3Afalse%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%5D%2C%22sortInfo%22%3A%5B%5D%2C%22filterInfo%22%3A%5B%5D%2C%22remotePaging%22%3Atrue%2C%22remoteSort%22%3Afalse%2C%22parameters%22%3A%7B%22zjlx%22%3A%2201%22%2C%22zjhm%22%3A%22" + Res.IdentityCard + "%22%2C%22jkhth%22%3A%22%22%2C%22issearch%22%3A%22%22%2C%22lmkkh%22%3A%22" + Res.BankCardNo + "%22%7D%2C%22action%22%3A%22load%22%7D";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                //贷款基本信息
                Url = baseUrl + string.Format("person/C0157/nextpage.act?zjlx=01&zjhm={0}&jkhth=0&issearch=&lmkkh={1}", Res.IdentityCard, Res.BankCardNo);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='bjhj']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Principal_Payed = results[0].ToDecimal(0);  //已还本金合计
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lxhj']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Interest_Payed = results[0].ToDecimal(0);  //已还利息合计
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='lxhj']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Interest_Payed = results[0].ToDecimal(0);  //已还利息合计 Interest_Penalty
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='fxje']", "value");
                if (results.Count > 0)
                {
                    Res_Loan.Interest_Penalty = results[0].ToDecimal(0);  //罚息 
                }

                //贷款详细信息
                Url = baseUrl + "person/C0157/nextlist.act";
                postdata = "_gt_json=%7B%22recordType%22%3A%22object%22%2C%22pageInfo%22%3A%7B%22pageSize%22%3A50000%2C%22pageNum%22%3A1%2C%22totalRowNum%22%3A-1%2C%22totalPageNum%22%3A0%2C%22startRowNum%22%3A1%2C%22endRowNum%22%3A0%7D%2C%22columnInfo%22%3A%5B%7B%22id%22%3A%22hkqh%22%2C%22header%22%3A%22%E8%BF%98%E6%AC%BE%E6%9C%9F%E5%8F%B7%22%2C%22fieldName%22%3A%22hkqh%22%2C%22fieldIndex%22%3A%22hkqh%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22yhkrq%22%2C%22header%22%3A%22%E5%BA%94%E8%BF%98%E6%AC%BE%E6%97%A5%E6%9C%9F%22%2C%22fieldName%22%3A%22yhkrq%22%2C%22fieldIndex%22%3A%22yhkrq%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22bqyhbj%22%2C%22header%22%3A%22%E6%9C%AC%E6%9C%9F%E5%BA%94%E8%BF%98%E6%9C%AC%E9%87%91%22%2C%22fieldName%22%3A%22bqyhbj%22%2C%22fieldIndex%22%3A%22bqyhbj%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22bqyhlx%22%2C%22header%22%3A%22%E6%9C%AC%E6%9C%9F%E5%BA%94%E8%BF%98%E5%88%A9%E6%81%AF%22%2C%22fieldName%22%3A%22bqyhlx%22%2C%22fieldIndex%22%3A%22bqyhlx%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22bqyhbxe%22%2C%22header%22%3A%22%E6%9C%AC%E6%9C%9F%E5%BA%94%E8%BF%98%E6%9C%AC%E6%81%AF%E9%A2%9D%22%2C%22fieldName%22%3A%22bqyhbxe%22%2C%22fieldIndex%22%3A%22bqyhbxe%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22shkrq%22%2C%22header%22%3A%22%E5%AE%9E%E8%BF%98%E6%AC%BE%E6%97%A5%E6%9C%9F%22%2C%22fieldName%22%3A%22shkrq%22%2C%22fieldIndex%22%3A%22shkrq%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%2C%7B%22id%22%3A%22shbj%22%2C%22header%22%3A%22%E5%AE%9E%E8%BF%98%E6%9C%AC%E9%87%91%22%2C%22fieldName%22%3A%22shbj%22%2C%22fieldIndex%22%3A%22shbj%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22shlx%22%2C%22header%22%3A%22%E5%AE%9E%E8%BF%98%E5%88%A9%E6%81%AF%22%2C%22fieldName%22%3A%22shlx%22%2C%22fieldIndex%22%3A%22shlx%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22null%22%7D%2C%7B%22id%22%3A%22hkfs%22%2C%22header%22%3A%22%E8%BF%98%E6%AC%BE%E7%8A%B6%E6%80%81%22%2C%22fieldName%22%3A%22hkfs%22%2C%22fieldIndex%22%3A%22hkfs%22%2C%22sortOrder%22%3Anull%2C%22hidden%22%3Afalse%2C%22exportable%22%3Atrue%2C%22printable%22%3Atrue%2C%22mappingItem%22%3A%22%22%2C%22keyVals%22%3A%22%22%2C%22valFormat%22%3A%22%22%7D%5D%2C%22sortInfo%22%3A%5B%5D%2C%22filterInfo%22%3A%5B%5D%2C%22remotePaging%22%3Atrue%2C%22remoteSort%22%3Afalse%2C%22parameters%22%3A%7B%22jkhth%22%3A%220%22%2C%22%22%3A%22%22%7D%2C%22action%22%3A%22load%22%7D";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                list = jsonParser.GetArrayFromParse(httpResult.Html, "data");
                foreach (var item in list)
                {
                    ProvidentFundLoanDetail Res_LoanDetail = new ProvidentFundLoanDetail();
                    var tdRow = JObject.Parse(item);
                    if (tdRow.Count > 0)
                    {
                        Res_LoanDetail.Description = tdRow["hkfs"].ToString();  //描述
                        Res_LoanDetail.Record_Date = tdRow["yhkrq"].ToString(); //记账日期
                        Res_LoanDetail.Record_Month = tdRow["shkrq"].ToString(); //还款日期
                        Res_LoanDetail.Record_Period = tdRow["hkqh"].ToString(); //还款期数
                        if (Res_LoanDetail.Description.Contains("正常"))
                        {
                            Res_LoanDetail.Principal = tdRow["bqyhbj"].ToString().ToDecimal(0);  //还款本金
                            Res_LoanDetail.Interest = tdRow["shbj"].ToString().ToDecimal(0);  //还款本金
                            Res_LoanDetail.Interest_Penalty_ToPay = tdRow["bqyhbxe"].ToString().ToDecimal(0);  //还款本息

                        }
                        else
                        {
                            Res_LoanDetail.Overdue_Principal = tdRow["bqyhbj"].ToString().ToDecimal(0);  //逾期本金
                            Res_LoanDetail.Overdue_Interest = tdRow["shbj"].ToString().ToDecimal(0);  //逾期利息
                            Res_LoanDetail.Interest_Penalty = tdRow["shbj"].ToString().ToDecimal(0) - tdRow["bqyhlx"].ToString().ToDecimal(0);  //罚息（用实际缴纳利息减去应该缴纳利息）
                            Res_LoanDetail.Interest_Penalty_ToPay = tdRow["bqyhbxe"].ToString().ToDecimal(0);  //还款本息
                        }
                        Res_Loan.ProvidentFundLoanDetailList.Add(Res_LoanDetail);
                    }
                }


                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.ProvidentFundLoanRes = Res_Loan;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }


        /// <summary>
        /// 解决：基础连接已经关闭: 未能为SSL/TLS 安全通道建立信任关系
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
