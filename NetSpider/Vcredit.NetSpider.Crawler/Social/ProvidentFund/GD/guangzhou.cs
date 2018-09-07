using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.GD
{
    /// <summary>
    /// 网站验证码指定URL失败,导致解析验证码失败
    /// </summary>
    public class guangzhou : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = new HttpResult();
        HttpItem httpItem = new HttpItem();
        string baseUrl = "https://gzgjj.gov.cn/wsywgr/";
        string fundCity = "gd_guangzhou";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        private ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        private List<string> _results = new List<string>();
        string _postData = string.Empty;//post参数
        private int PaymentMonths = 0;//连续缴费月数
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                //string rdm = DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds.ToString("F0");
                Url = baseUrl + "CheckAction!createYZM.action";
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
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
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
            try
            {    //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (string.IsNullOrEmpty(fundReq.Identitycard) || string.IsNullOrEmpty(fundReq.Password)||string.IsNullOrEmpty(fundReq.Username))
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆（参数自定义加密）

                Url = baseUrl + "LoginAction!login.action";
                _postData = string.Format("radiobutton=radiobutton&certno=undefined&zjh=Aj%2BtfLaVO6VePDT1dpuLeA37GizElkKsumpZIMUOv9eUt0R%2FEx6eaiJBF%2BLZ82K7P5wY6kqwwdcbSOvfAxIH2Eto%2BNgOwJIfh6s2j2%2BhNY0KLFJfpfEkZAUR18Gy6477h3OdITPLg%2FHLvvMTRj1NIRqa7AJUmarSm6aDHcMWNpCGAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAApXHHRFApk8Z1o03o7aD4bJYVnnJJKYSY%3D&name=%E5%88%98%E8%90%8D&password=Al1Ubl778tOwEw51ETzix9opcY4rwVVNV%2FZaOWht0nLbXIvLBDIMT1sqnZ%2BA6%2FCwkqHeeS8opmKTV5ZTGeuvYrJU9TXqraeFao%2B%2FosrflN%2B6JAA7TWUa5rItcp1U1lmo01NXs8sMIba7PDt8Ljb5zdsmKk7ArH2N3dma3q%2FdsvA0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABSG0RsnGsdCQ%3D%3D&captcha={0}", fundReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = _postData,
                    Method = "Post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (!httpResult.Html.Contains("退出系统"))
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_InitFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步,获取基本信息
                Url = baseUrl + "person/personQuery!personInfo.do?aId=&areaCode=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Encoding=Encoding.GetEncoding("GBK"),
                    Referer = baseUrl + "personapply/index.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[1]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.ProvidentFundNo = _results[0];//审批单号
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[2]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Name = _results[0];//姓名
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[3]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.IdentityCard = _results[0];//身份证号码
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[4]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyName = _results[0];//开设单位
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[7]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.OpenTime = _results[0];//实际开设日期
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[8]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.Status = _results[0];//个人明细状态
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[9]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.TotalAmount = _results[0].ToDecimal(0);//公积金余额
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[10]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.SalaryBase = _results[0].ToDecimal(0);//计缴公积金工资基数
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[12]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = Res.SalaryBase * _results[0].ToTrim("%").ToDecimal(0)*(decimal)0.01;
                    Res.CompanyMonthPayRate = _results[0].ToTrim("%").ToDecimal(0);//单位缴存比例
                }
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tr[13]/td[2]", "text");
                if (_results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = Res.SalaryBase * _results[0].ToTrim("%").ToDecimal(0) * (decimal)0.01;
                    Res.PersonalMonthPayRate = _results[0].ToTrim("%").ToDecimal(0);//个人缴存比例
                }
                #endregion

                #region 第三步,获取缴费详细
                Url = baseUrl + "person/personQuery!moneyseq.do?aId=&areaCode=";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    Encoding = Encoding.GetEncoding("GBK"),
                    Referer = baseUrl + "personapply/index.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                _results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='tree']/tbody/tr", "inner", true);
                Regex reg = new Regex(@"[^0-9]*");
                foreach (string item in _results)
                {
                    ProvidentFundDetail detail = new ProvidentFundDetail();
                    List<string> tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 4)
                    {
                        continue;
                    }
                    detail.PayTime = tdRow[0].ToDateTime(Consts.DateFormatString2);//缴费年月
                    detail.ProvidentFundTime = reg.Replace(tdRow[3], "");//应属年月
                    detail.Description = tdRow[3];//描述
                    if (tdRow[3].IndexOf("汇缴", StringComparison.Ordinal) > -1)
                    {
                        detail.ProvidentFundBase = Res.SalaryBase;//缴费基数
                        detail.PersonalPayAmount = tdRow[1].ToDecimal(0) / 2;//个人缴费金额
                        detail.CompanyPayAmount = tdRow[1].ToDecimal(0) / 2;//企业缴费金额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;//缴费标志
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;//缴费类型
                        PaymentMonths++;
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = tdRow[1].ToDecimal(0) ;//个人缴费金额
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费标志
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;//缴费类型
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
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }
    }
}
