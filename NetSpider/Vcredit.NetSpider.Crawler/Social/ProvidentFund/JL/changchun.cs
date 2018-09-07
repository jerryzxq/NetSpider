using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JL
{
    public class changchun : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.cczfgjj.gov.cn/";
        string fundCity = "jl_changchun";
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "website/trans/ValidateImg";
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
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = fundCity;
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.08;
            ProvidentFundDetail detail = null;
            int PaymentMonths = 0;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = "请输入查询密码";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (fundReq.LoginType == "2")
                {
                    if (String.IsNullOrWhiteSpace(fundReq.Identitycard))
                    {
                        Res.StatusDescription = "身份证号不能为空 ";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    fundReq.Username = string.Empty;
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(fundReq.Username))
                    {
                        Res.StatusDescription = "公积金账号不能为空 ";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    fundReq.Identitycard = string.Empty;
                }

                #region 第一步，登录
                //tranCode:'142805'
                string tranCode = string.Empty;
                tranCode = "142805";//js脚本写死为142805
                Url = baseUrl + "GJJQuery";
                postdata = String.Format("tranCode={3}&task=&accnum={4}&certinum={0}&pwd={1}&verify={2}", fundReq.Identitycard, fundReq.Password, fundReq.Vercode, tranCode, fundReq.Username);//accnum 公积金   certinum 身份证
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                //请求失败后返回
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "\"msg\":\"", "\"}");
                if (!errorInfo.IsEmpty() && !errorInfo.Contains("成功") && !httpResult.Html.Contains("success"))
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion
                #region 第二步，查询个人基本信息

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Url = baseUrl + "GJJQuery";
                tranCode = "112813";
                postdata = string.Format("tranCode={2}&task=&accnum={0}&certinum={1}", cookies[0].Value, cookies[2].Value, tranCode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.Contains("error"))
                {
                    string errorMsg = CommonFun.GetMidStr("", "{\"error\":\"", "\"}");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string JsonStr = "[" + httpResult.Html + "]";
                List<changChunBasicInfo> listData = jsonParser.DeserializeObject<List<changChunBasicInfo>>(JsonStr);
                Res.Name = listData[0].accname;//姓名
                Res.ProvidentFundNo = listData[0].accnum;//个人公积金账号
                Res.Status = listData[0].accname1;//账户状态
                Res.TotalAmount = listData[0].bal.ToDecimal(0);//总余额
                Res.SalaryBase = listData[0].basenum.ToDecimal(0);//缴存基数
                Res.IdentityCard = listData[0].certinum;//身份证号
                Res.PersonalMonthPayAmount = listData[0].indipayamt.ToDecimal(0);//个人缴存额
                // listData[0].indipaysum;//月缴存额
                Res.PersonalMonthPayRate = listData[0].indiprop.ToDecimal(0);//个人缴存比率
                //listData[0].instname;//所辖机构
                //listData[0].lasttransdate;//最后交易日期
                Res.LastProvidentFundTime = new Regex(@"[^0-9]*").Replace(listData[0].lpaym, "");//缴至年月
                Res.OpenTime = listData[0].opnaccdate == "1899-12-31" ? null : listData[0].opnaccdate;//开户日期
                Res.CompanyName = listData[0].unitaccname;//单位名称
                Res.CompanyNo = listData[0].unitaccnum;//单位账号
                Res.CompanyMonthPayAmount = listData[0].unitpayamt.ToDecimal(0);//单位缴存额
                Res.CompanyMonthPayRate = listData[0].unitprop.ToDecimal(0);//单位缴存比例
                #endregion


                #region 第三步，缴费明细查询(仅仅提供查询最近2年记录，更多历史明细查询，请到业务窗口办理)
                decimal perAccounting;//个人占比
                decimal comAccounting;//公司占比
                decimal totalRate;//总缴费比率
                if (listData[0].unitprop.ToDecimal(0) > 0 && listData[0].indiprop.ToDecimal(0) > 0)
                {
                    totalRate = listData[0].unitprop.ToDecimal(0) + listData[0].indiprop.ToDecimal(0);//总缴费比率（公司+个人）
                    perAccounting = (listData[0].indiprop.ToDecimal(0) / totalRate);
                    comAccounting = (listData[0].unitprop.ToDecimal(0) / totalRate);
                }
                else
                {
                    totalRate = (payRate) * 2;//0.16
                    perAccounting = comAccounting = (decimal)0.50;
                }
                //
                DateTime nowTime = DateTime.Now;
                Url = baseUrl + "GJJQuery";
                postdata = string.Format("tranCode=112814&task=ftp&accnum={0}&begdate={1}&enddate={2}", cookies[0].Value, nowTime.AddYears(-2).ToString("yyyy-MM-dd"), nowTime.ToString("yyyy-MM-dd"));
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Postdata = postdata,
                    Method = "Post",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string JsonStrDetails = httpResult.Html; ;
                if (httpResult.Html.Contains("error"))
                {
                    string errorMsg = CommonFun.GetMidStr("", "{\"error\":\"", "\"}");
                    Res.StatusDescription = errorMsg;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                List<changChunDetails> detailList = jsonParser.DeserializeObject<List<changChunDetails>>(JsonStrDetails);
                foreach (var item in detailList)
                {
                    detail = new ProvidentFundDetail();
                    detail.Description = item.ywtype;//描述
                    detail.PayTime = item.trandate.ToDateTime();//缴费年月
                    detail.ProvidentFundTime = Convert.ToDateTime(item.trandate).ToString("yyyyMM");//new Regex(@"[^0-9]*").Replace(item.yearmonth, "")
                    if (item.ywtype.Trim() == "汇缴")
                    {
                        detail.CompanyPayAmount = item.amt.ToDecimal(0) * comAccounting;//单位缴费
                        detail.PersonalPayAmount = item.amt.ToDecimal(0) * perAccounting;//个人缴费
                        detail.ProvidentFundBase = (item.amt.ToDecimal(0) / totalRate).ToString("f2").ToDecimal(0);//缴费基数
                        detail.CompanyName = item.unitaccname;//单位名称
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else if (item.ywtype.Contains("支取") || item.ywtype.Contains("提取"))
                    {
                        detail.PersonalPayAmount = item.amt.ToDecimal(0) * perAccounting;//个人缴费
                        detail.CompanyName = item.unitaccname;//单位名称
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Draw;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Draw;
                        Res.Description = "有支取，请人工校验。";  //缴费详单存在支取
                    }
                    else
                    {//（补缴，结息etc，数据不精确，只做参考用）
                        detail.PersonalPayAmount = item.amt.ToDecimal(0);
                        detail.CompanyName = item.unitaccname;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion


                #region ********贷款信息需要合同编号**********

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
    /// <summary>
    /// 基本信息字段
    /// </summary>
    public class changChunBasicInfo
    {
        /// <summary>
        /// 最后交易日期
        /// </summary>
        public string lasttransdate { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string certinum { get; set; }
        /// <summary>
        /// 开户日期
        /// </summary>
        public string opnaccdate { get; set; }
        /// <summary>
        /// 单位缴存额
        /// </summary>
        public string unitpayamt { get; set; }
        /// <summary>
        /// 个人公积金账号
        /// </summary>
        public string accnum { get; set; }
        /// <summary>
        /// 账户状态
        /// </summary>
        public string accname1 { get; set; }
        /// <summary>
        /// 单位缴存比例
        /// </summary>
        public string unitprop { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string unitaccname { get; set; }
        /// <summary>
        /// 单位账号
        /// </summary>
        public string unitaccnum { get; set; }
        /// <summary>
        /// 个人缴存额
        /// </summary>
        public string indipayamt { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string accname { get; set; }
        /// <summary>
        /// 个人缴存比例
        /// </summary>
        public string indiprop { get; set; }
        /// <summary>
        /// 月缴存额
        /// </summary>
        public string indipaysum { get; set; }
        /// <summary>
        /// 总余额
        /// </summary>
        public string bal { get; set; }
        /// <summary>
        /// 所辖机构
        /// </summary>
        public string instname { get; set; }
        /// <summary>
        /// 缴至年月
        /// </summary>
        public string lpaym { get; set; }
        /// <summary>
        /// 缴存基数
        /// </summary>
        public string basenum { get; set; }

    }
    /// <summary>
    /// 缴费明细字段
    /// </summary>
    public class changChunDetails
    {
        /// <summary>
        /// 用不到的隐藏字段（明细）
        /// </summary>
        public string yearmonth { get; set; }
        /// <summary>
        /// 公积金账号（明细）
        /// </summary>
        public string accnum { get; set; }
        /// <summary>
        /// 发生额（明细）
        /// </summary>
        public string amt { get; set; }
        /// <summary>
        /// 业务类型（明细）
        /// </summary>
        public string ywtype { get; set; }
        /// <summary>
        /// 单位名称（明细）
        /// </summary>
        public string unitaccname { get; set; }
        /// <summary>
        /// 交易日期（明细）
        /// </summary>
        public string trandate { get; set; }
        /// <summary>
        /// 单位账号（明细）
        /// </summary>
        public string unitaccnum { get; set; }
        /// <summary>
        /// 姓名（明细）
        /// </summary>
        public string accname { get; set; }
        /// <summary>
        /// 余额（明细）
        /// </summary>
        public string bal { get; set; }
    }
}
