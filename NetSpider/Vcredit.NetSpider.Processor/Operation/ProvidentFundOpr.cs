using System.Net;
using System.Text;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity;
using System;
using Vcredit.NetSpider.Processor.Operation.JsonModel.ProvidentFund;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vcredit.NetSpider.Processor.Operation
{
    internal class ProvidentFundOpr
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

        #region 青岛
        /// <summary>
        /// 青岛公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Qingdao_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://219.147.7.52:89/Controller/Image.aspx";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "青岛公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "青岛公积金查询初始化异常";
                Log4netAdapter.WriteError("青岛公积金查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 青岛公积金登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public ProvidentFundQueryRes Qingdao_GetProvidentFund(string username, string password, string token, string vercode)
        {

            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = "青岛";//公积金所在城市
            string Url = string.Empty;
            string postdata = string.Empty;
            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录青岛公积金
                Url = "http://219.147.7.52:89/Controller/login.ashx";
                postdata = String.Format("name={0}&password={1}&yzm={2}&logintype=0&usertype=10&dn=&signdata=&1=y", username, password, vercode);
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
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (!httpResult.Html.Contains("登录成功"))
                {
                    //goto start;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = "登录失败";
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询工作单位基本信息
                Url = "http://219.147.7.52:89/Controller/GR/gjcx/dwjbxx.ashx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                Res.CompanyOpenTime = jsonParser.GetResultFromParser(httpResult.Html, "clrq");//成立日期
                Res.CompanyName = jsonParser.GetResultFromParser(httpResult.Html, "hm");//单位名称
                Res.CompanyAddress = jsonParser.GetResultFromParser(httpResult.Html, "dz");//单位地址
                Res.CompanyNo = jsonParser.GetResultFromParser(httpResult.Html, "khh");//单位编号
                Res.CompanyDistrict = jsonParser.GetResultFromParser(httpResult.Html, "szqs");//所在市区
                Res.CompanyLicense = jsonParser.GetResultFromParser(httpResult.Html, "yyzz");//营业执照编号
                #endregion

                #region 第三步，查询个人基本信息
                Url = "http://219.147.7.52:89/Controller/GR/gjcx/gjjzlcx.ashx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = jsonParser.GetResultFromParser(httpResult.Html, "hm");//职工姓名
                if (Res.Name.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.EmployeeNo = jsonParser.GetResultFromParser(httpResult.Html, "khh");//职工编号
                Res.Status = jsonParser.GetResultFromParser(httpResult.Html, "zt");//账户状态
                Res.IdentityCard = jsonParser.GetResultFromParser(httpResult.Html, "sfz");//身份证
                Res.Phone = jsonParser.GetResultFromParser(httpResult.Html, "sjhm");//手机号码
                Res.OpenTime = jsonParser.GetResultFromParser(httpResult.Html, "khrq");//开户日期
                Res.BankCardOpenTime = jsonParser.GetResultFromParser(httpResult.Html, "djrq");//登记日期
                Res.Bank = jsonParser.GetResultFromParser(httpResult.Html, "hb");//发卡行
                Res.BankCardNo = jsonParser.GetResultFromParser(httpResult.Html, "kh");//联名卡号
                Res.CompanyMonthPayRate = jsonParser.GetResultFromParser(httpResult.Html, "dwjcbl").ToDecimal(0);//单位比例
                Res.CompanyMonthPayAmount = jsonParser.GetResultFromParser(httpResult.Html, "dwyhjje").ToDecimal(0);//单位月汇缴额
                Res.PersonalMonthPayRate = jsonParser.GetResultFromParser(httpResult.Html, "grjcbl").ToDecimal(0);//个人比例
                Res.PersonalMonthPayAmount = jsonParser.GetResultFromParser(httpResult.Html, "gryhjje").ToDecimal(0);//单位月汇缴额
                Res.SalaryBase = jsonParser.GetResultFromParser(httpResult.Html, "gze").ToDecimal(0);//月工资额
                Res.TotalAmount = jsonParser.GetResultFromParser(httpResult.Html, "zhye").ToDecimal(0);//账户余额
                //Res.MonthPayAmount = Res.PersonalMonthPayAmount + Res.CompanyMonthPayAmount;

                #endregion

                #region 第四步，查询缴费明细总共记录数
                Url = "http://219.147.7.52:89/Controller/GR/gjcx/gjjmx.ashx?transDateBegin=2005-01-01&transDateEnd=2014-12-31&page=1&rows=30&sort=mxbc&order=desc";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 第四步，查询缴费明细总共记录数
                Url = "http://219.147.7.52:89/Controller/GR/gjcx/gjcx.ashx";
                string nowdate = DateTime.Now.ToString("yyyy-MM-dd");
                string rows = "1";
                postdata = String.Format("m=grjcmx&start=2005-01-01&end={0}&page=1&rows={1}&sort=csrq&order=desc", nowdate, rows);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                rows = jsonParser.GetResultFromParser(httpResult.Html, "total");
                #endregion

                #region 第五步，查询缴费明细
                postdata = String.Format("m=grjcmx&start=2005-01-01&end={0}&page=1&rows={1}&sort=csrq&order=desc", nowdate, rows);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string JsonStr = CommonFun.GetMidStrByRegex(httpResult.Html, "\"rows\":", "]") + "]";
                var detailList = jsonParser.DeserializeObject<List<QingDaoDetail>>(JsonStr);

                ProvidentFundDetail detail = null;
                int PaymentMonths = 0;
                foreach (var detailItem in detailList)
                {
                    detail = new ProvidentFundDetail();
                    detail.CompanyName = detailItem.hm;
                    detail.CompanyPayAmount = detailItem.dwje.ToDecimal(0);
                    detail.PaymentFlag = detailItem.ztname;
                    detail.PaymentType = detailItem.jjyyname;
                    detail.PayTime = detailItem.csrq.ToDateTime();
                    detail.ProvidentFundTime = detailItem.ssny;
                    detail.PersonalPayAmount = detailItem.grje.ToDecimal(0);
                    if (detailItem.jjyyname == "正常汇缴")
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 上海
        /// <summary>
        /// 上海公积金，页面初始化 
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Shanghai_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "https://persons.shgjj.com/SsoLogin?url=https://persons.shgjj.com/MainServlet?ID=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://persons.shgjj.com/VerifyImageServlet";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.Number);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "上海公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "上海公积金查询初始化异常";
                Log4netAdapter.WriteError("上海公积金查询初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 上海公积金登录、查询
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="token">会话token</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public ProvidentFundQueryRes Shanghai_GetProvidentFund(string username, string password, string token, string vercode)
        {

            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = "上海";
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.07;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录

                Url = "https://persons.shgjj.com/SsoLogin";
                string passwordMD5 = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5"); ;
                postdata = String.Format("username={0}&password={1}&imagecode={2}&password_md5={3}&ID=0", username, password, vercode, passwordMD5);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='para1']", "value");//解析html
                if (results.Count == 0)
                {
                    //goto Lable_Start;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Res.StatusDescription = "登录失败";
                    return Res;
                }
                #endregion

                #region 第二步，进入公积金SSO页面，初始化页面（主要是为获取cookie和下一步URL）

                Url = "http://bbs.shgjj.com/sso/sso.php?url=https://persons.shgjj.com/MainServlet?ID=1";
                postdata = String.Format("para1={0}&para2={1}", username, passwordMD5);
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies
                #endregion

                #region 第三步，进入公积金SSO导航页
                Url = HtmlParser.GetResultFromParser(httpResult.Html, "//script[1]", "src")[0]; ;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);//合并cookies

                #endregion

                #region 第四步，进入公积金首页 解析html，并对解析后的数据进行整理
                Url = "https://persons.shgjj.com/MainServlet?ID=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                Res.Name = CommonFun.GetMidStrByRegex(httpResult.Html, "姓 名</div></td><td width=\"751\">", "<strong>");//姓名

                if (Res.Name.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.OpenTime = (DateTime.ParseExact(CommonFun.GetMidStrByRegex(httpResult.Html, "开户日期</div></td><td>", "</td>"), Consts.DateFormatString4, null)).ToString();//开户日期
                Res.CompanyName = CommonFun.ClearFlag(CommonFun.GetMidStrByRegex(httpResult.Html, "所属单位</div></td><td>", "</td>")).ToTrim();//所属单位
                Res.TotalAmount = CommonFun.GetMidStrByRegex(httpResult.Html, "账户余额</div></td><td>", "</td>").Replace("元", "").ToDecimal(0);//账户余额
                Res.Status = CommonFun.GetMidStrByRegex(httpResult.Html, "当前账户状态</div></td><td>", "<input").Replace(" ", "");//当前账户状态
                Res.PersonalMonthPayAmount = CommonFun.GetMidStrByRegex(httpResult.Html, "月缴存额</div></td><td>", "</td>").Replace("元", "").ToDecimal(0) / 2;//月缴存额
                Res.LastProvidentFundTime = CommonFun.GetMidStrByRegex(httpResult.Html, "末次缴存年月</div></td><td>", "</td>").Replace("年", "").Replace("月", "");//月缴存额

                Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                //Res.MonthPayAmount = Res.PersonalMonthPayAmount + Res.CompanyMonthPayAmount;
                Res.SalaryBase = Res.CompanyMonthPayAmount / payRate;
                #endregion

                #region 第五步，进入公积金缴费详情页，解析html，并对解析后的数据进行整理
                Url = "https://persons.shgjj.com/MainServlet?ID=11";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
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

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='table']/tr[position() >2]", "");
                ProvidentFundDetail detail = null;

                int PaymentMonths = 0;
                foreach (string strItem in results)
                {
                    var strDetail = HtmlParser.GetResultFromParser(strItem, "//div", "");
                    if (strDetail.Count < 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.CompanyName = strDetail[1];//缴费单位
                    detail.PayTime = DateTime.ParseExact(strDetail[0], Consts.DateFormatString4, null);//发生日期
                    if (strDetail[3].IndexOf("汇缴") != -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.ProvidentFundTime = strDetail[3].Substring(2, 4) + strDetail[3].Substring(7, 2);
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else if (strDetail[3].IndexOf("font") != -1)
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = HtmlParser.GetResultFromParser(strItem, "//font", "")[0];
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                    }
                    else
                    {
                        detail.PaymentFlag = strDetail[3];
                        detail.PaymentType = strDetail[3];
                        detail.PersonalPayAmount = strDetail[2].ToDecimal(0);//金额
                    }

                    Res.ProvidentFundDetailList.Add(detail);
                }
                Res.PaymentMonths = PaymentMonths;
                #endregion

                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 成都
        /// <summary>
        /// 成都公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Chengdu_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.cdzfgjj.gov.cn/api.php?op=checkcode&code_len=4&font_size=20&width=130&height=50";
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
                Res.VerCode = secParser.GetVerCode(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "成都公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "成都公积金初始化异常";
                Log4netAdapter.WriteError("成都公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 成都公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Chengdu_GetProvidentFund(string username, string password, string token, string vercode)
        {

            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = "成都";
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=login";
                postdata = String.Format("cardNo={0}&password={1}&verifyCode={2}", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='content guery']", "");

                if (httpResult.StatusCode != HttpStatusCode.OK || results.Count > 0)
                {
                    Res.StatusDescription = "登录失败," + results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    CacheHelper.RemoveCache(token);
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询个人基本信息
                Url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=info";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='form-table']/tr/td[@class='c']", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                    Res.Name = results[1];
                    Res.IdentityCard = results[3];
                    Res.Phone = results[5];
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #endregion

                #region 第三步，查询公司信息、公积金信息
                Url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=account";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='form-table']/tr/td[@class='c']", "");
                if (results.Count > 0)
                {
                    Res.CompanyNo = results[0];
                    Res.CompanyName = results[1];
                    //if (!results[2].IsEmpty())
                    //{
                    //    Res.EndTime = (results[2].ToDateTime(Consts.DateFormatString7);
                    //}
                    if (!results[3].IsEmpty())
                    {
                        Res.SalaryBase = results[3].ToDecimal(0);
                    }
                    if (!results[4].IsEmpty())
                    {
                        Res.CompanyMonthPayAmount = results[4].ToDecimal(0);
                    }
                    if (!results[5].IsEmpty())
                    {
                        Res.PersonalMonthPayAmount = results[5].ToDecimal(0);
                    }
                    if (!results[6].IsEmpty())
                    {
                        Res.TotalAmount = results[6].ToDecimal(0);
                    }
                    Res.Status = results[7];
                }
                #endregion

                #region 第四步，查询缴费明细
                string stime = "2000-01-01";
                string etime = DateTime.Now.ToString(Consts.DateFormatString2);
                Url = "http://www.cdzfgjj.gov.cn/index.php?m=content&c=gjj&a=detailquery";
                postdata = string.Format("startDate={0}&endDate={1}", stime, etime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//tbody/tr", "");
                ProvidentFundDetail detail = null;
                int PaymentMonths = 0;
                foreach (string item in results)
                {
                    var strDetail = HtmlParser.GetResultFromParser(item, "//td", "");
                    if (strDetail.Count > 0)
                    {
                        detail = new ProvidentFundDetail();
                        detail.CompanyName = strDetail[1];
                        detail.PayTime = DateTime.ParseExact(strDetail[2], "yyyyMMdd", null);
                        if (strDetail[3].IndexOf("汇缴") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = strDetail[4].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundTime = CommonFun.GetMidStr(strDetail[3], "汇缴", "");
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = strDetail[3];
                            detail.PersonalPayAmount = strDetail[4].ToDecimal(0);
                        }
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 北京
        /// <summary>
        /// 北京公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Beijing_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.bjgjj.gov.cn/wsyw/servlet/PicCheckCode1";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "北京公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "北京公积金初始化异常";
                Log4netAdapter.WriteError("北京公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 北京公积金登录、查询
        /// </summary>
        /// <param name="username">联名卡号</param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Beijing_GetProvidentFund(string username, string password, string token, string vercode)
        {

            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = "北京";
            string Url = string.Empty;
            string postdata = string.Empty;
            decimal payRate = (decimal)0.12;//个人缴费费率
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                string Key1 = "pdcss123";
                string Key2 = "css11q1a";
                string Key3 = "co1qacq11";
                #region 第一步，初始化登录页面
                Url = "http://www.bjgjj.gov.cn/wsyw/wscx/gjjcx-login.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                #endregion

                #region 第二步，获取动态变量lk

                Url = "http://www.bjgjj.gov.cn/wsyw/wscx/asdwqnasmdnams.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "http://www.bjgjj.gov.cn/wsyw/wscx/gjjcx-login.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                #endregion

                #region 第三步，登录系统
                string lk = CommonFun.ClearFlag(httpResult.Html);
                lk = lk.Substring(4, lk.Length - 4);


                Url = "http://www.bjgjj.gov.cn/wsyw/wscx/gjjcx-choice.jsp";
                postdata = String.Format("lb=5&bh={0}&mm={1}&gjjcxjjmyhpppp={2}&lk={3}", MultiKeyDES.EncryptDES(username, Key1, Key2, Key3), MultiKeyDES.EncryptDES(password, Key1, Key2, Key3), vercode, lk);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//td[@class='title-zcfgmc']/div/table/tr[last()]/td[2]", "");

                if (httpResult.StatusCode != HttpStatusCode.OK || results.Count == 0)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    CacheHelper.RemoveCache(token);
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = CommonFun.GetMidStr(results[0], "window.open(\"", "\",");

                if (Url.IsEmpty())
                {
                    Res.StatusDescription = "密码错误或无公积金信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    CacheHelper.RemoveCache(token);
                    return Res;
                }
                #endregion

                #region 第四步，查询个人基本信息

                Url = "http://www.bjgjj.gov.cn/wsyw/wscx/" + Url;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                //分析个人基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[3]/td[1]/table[1]/tr[1]/td[1]/div[1]/table[1]/tr[1]/td[1]/div[1]/div[2]/table[1]/tr/td", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = HttpUtility.HtmlDecode(results[1]);
                    Res.ProvidentFundNo = results[3];
                    Res.IdentityCard = results[7];
                    Res.CompanyNo = results[9];
                    Res.CompanyName = HttpUtility.HtmlDecode(results[11]);
                    Res.TotalAmount = results[17].Replace("元", "").ToDecimal(0);
                    Res.Status = HttpUtility.HtmlDecode(results[19]);
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //今年缴费明细
                List<string> detailResults = HtmlParser.GetResultFromParser(httpResult.Html, "//table[2]/tr[3]/td[1]/table[1]/tr[1]/td[1]/div[1]/table[1]/tr[1]/td[1]/div[3]/div[1]/table[1]/tr[position()>1]", "inner", true);

                Url = CommonFun.GetMidStr(httpResult.Html, "gjj_cxls.jsp?", "&#32564;&#23384;");
                #endregion

                #region 第五步，缴费明细
                Url = "http://www.bjgjj.gov.cn/wsyw/wscx/gjj_cxls.jsp?" + Url + "缴存";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[2]/tr[1]/td[1]/table[2]/tr[1]/td[1]/form[1]/div[1]/table[1]/tr[position()>1]", "");

                ProvidentFundDetail detail = null;
                int PaymentMonths = 0;
                //历史缴费信息
                foreach (string item in results)
                {
                    var strDetail = HtmlParser.GetResultFromParser(item.Replace("&nbsp;", ""), "//td", "text", true);
                    if (strDetail.Count == 6)
                    {
                        detail = new ProvidentFundDetail();

                        detail.PayTime = DateTime.ParseExact(strDetail[0], "yyyyMMdd", null);
                        detail.ProvidentFundTime = strDetail[1];
                        if (strDetail[2].IndexOf("&#27719;&#32564;") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = HttpUtility.HtmlDecode(strDetail[2]);
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0);
                        }
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                //今年缴费信息
                foreach (string item in detailResults)
                {
                    var strDetail = HtmlParser.GetResultFromParser(item.Replace("&nbsp;", ""), "//td", "inner", true);
                    if (strDetail.Count == 6)
                    {
                        //查询月份是否已统计过
                        var query = Res.ProvidentFundDetailList.Where(o => o.ProvidentFundTime == strDetail[1]).FirstOrDefault();
                        if (query != null)
                        {
                            continue;
                        }
                        detail = new ProvidentFundDetail();

                        detail.PayTime = DateTime.ParseExact(strDetail[0], "yyyyMMdd", null);
                        detail.ProvidentFundTime = strDetail[1];
                        if (strDetail[2].IndexOf("&#27719;&#32564;") != -1)
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.Description = HttpUtility.HtmlDecode(strDetail[2]);
                            detail.PersonalPayAmount = strDetail[3].ToDecimal(0);
                        }
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 南京
        /// <summary>
        /// 南京公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Nanjing_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.njgjj.com/vericode.jsp";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "南京公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "南京公积金初始化异常";
                Log4netAdapter.WriteError("南京公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 南京公积金登录、查询
        /// </summary>
        /// <param name="username">身份证</param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Nanjing_GetProvidentFund(string username, string password, string token, string vercode)
        {

            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            Res.ProvidentFundCity = "南京";
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = "http://www.njgjj.com/per.login";
                postdata = String.Format("certinum={0}&perpwd={1}&vericode={2}", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='WTLoginError']/ul/li[@class='text']", "text");

                if (httpResult.StatusCode != HttpStatusCode.OK || results.Count > 0)
                {
                    Res.StatusDescription = "登录失败," + results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    CacheHelper.RemoveCache(token);
                    return Res;
                }

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.njgjj.com/init.summer?_PROCID=80000003";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                string jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                jsonStr = "{" + jsonStr + "}";
                IDictionary<string, string> dict = jsonParser.GetStringDictFromParser(jsonStr);

                string accname = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='accname']", "value")[0];
                string certinum = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='certinum']", "value")[0];
                string accnum = dict["_ACCNUM"];
                if (accname.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                postdata = "";
                foreach (KeyValuePair<string, string> item in dict)
                {
                    postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }
                postdata += "accname=" + HttpUtility.UrlEncode(accname);
                postdata += "&prodcode=1";
                postdata += "&accnum=" + accnum;

                Url = "http://www.njgjj.com/command.summer";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ContentType = "application/x-www-form-urlencoded; charset=utf-8",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion

                #region 分析个人基本信息

                jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                Res.Name = accname;//姓名
                Res.ProvidentFundNo = accnum;//公积金账号
                Res.IdentityCard = certinum;//身份证
                Res.CompanyNo = jsonParser.GetResultFromParser(jsonStr, "unitaccnum");//公司编号
                Res.CompanyName = HttpUtility.HtmlDecode(jsonParser.GetResultFromParser(jsonStr, "unitaccname"));//公司名称
                Res.OpenTime = jsonParser.GetResultFromParser(jsonStr, "opnaccdate");//开户时间
                Res.TotalAmount = jsonParser.GetResultFromParser(jsonStr, "amt1").ToDecimal(0);//账号余额
                Res.PersonalMonthPayRate = jsonParser.GetResultFromParser(jsonStr, "indiprop").ToDecimal(0);//个人比例：
                Res.PersonalMonthPayAmount = jsonParser.GetResultFromParser(jsonStr, "amt2").ToDecimal(0) / 2;//个人缴费金额
                Res.CompanyMonthPayRate = jsonParser.GetResultFromParser(jsonStr, "unitprop").ToDecimal(0);//单位比例
                Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;//单位缴费金额
                Res.Bank = jsonParser.GetResultFromParser(jsonStr, "opnaccnet");//开户行
                if (Res.PersonalMonthPayRate != 0)
                {
                    Res.SalaryBase = Res.PersonalMonthPayAmount / Res.PersonalMonthPayRate;
                }
                //账户状态
                switch (jsonParser.GetResultFromParser(jsonStr, "indiaccstate"))
                {
                    case "0": Res.Status = "正常"; break;
                    case "1": Res.Status = "封存"; break;
                    case "3": Res.Status = "空账"; break;
                    case "9": Res.Status = "销户"; break;
                }
                #endregion

                #region 第三步，缴费明细
                Url = "http://www.njgjj.com/init.summer?_PROCID=70000002";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                jsonStr = CommonFun.GetMidStr(httpResult.Html, "poolSelect = {", "};");
                jsonStr = "{" + jsonStr + "}";
                dict = jsonParser.GetStringDictFromParser(jsonStr);
                string DATAlISTGHOST = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='DATAlISTGHOST']", "", true)[0];
                string _DATAPOOL_ = HtmlParser.GetResultFromParser(httpResult.Html, "//textarea[@name='_DATAPOOL_']", "", true)[0];

                postdata = "";
                foreach (KeyValuePair<string, string> item in dict)
                {
                    postdata += HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value.ToString()) + "&";
                }
                postdata += "&begdate=2005-01-01";
                postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                postdata += "&accname=" + accname.ToUrlEncode();
                postdata += "&accnum=" + accnum;
                Url = "http://www.njgjj.com/command.summer";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);


                int currentPage = 0;
                int pageCount = 0;
                ProvidentFundDetail detail = null;
                int PaymentMonths = 0;
                string ProvidentFundTime = string.Empty;
                do
                {
                    postdata = "dynamicTable_id=datalist2";
                    postdata += "&dynamicTable_currentPage=" + currentPage;
                    postdata += "&dynamicTable_pageSize=10";
                    postdata += "&dynamicTable_nextPage=" + (currentPage + 1);
                    postdata += "&dynamicTable_page=%2Fydpx%2F70000002%2F700002_01.ydpx";
                    postdata += "&dynamicTable_paging=true";
                    postdata += "&dynamicTable_configSqlCheck=0";
                    postdata += "&errorFilter=1%3D1";
                    postdata += "&begdate=2005-01-01";
                    postdata += "&enddate=" + DateTime.Now.ToString(Consts.DateFormatString2);
                    postdata += "&accnum=" + accnum;
                    postdata += "&accname=" + accname.ToUrlEncode();
                    postdata += "&_APPLY=0&_CHANNEL=1&_PROCID=70000002";
                    postdata += "&DATAlISTGHOST=" + DATAlISTGHOST.ToUrlEncode();
                    postdata += "&_DATAPOOL_=" + _DATAPOOL_.ToUrlEncode();
                    currentPage++;
                    Url = "http://www.njgjj.com/dynamictable";

                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    jsonStr = jsonParser.GetResultFromParser(httpResult.Html, "data");
                    currentPage = jsonParser.GetResultFromParser(jsonStr, "currentPage").ToInt(0);
                    pageCount = jsonParser.GetResultFromParser(jsonStr, "pageCount").ToInt(0);

                    jsonStr = jsonParser.GetResultFromParser(jsonStr, "data");
                    var details = jsonParser.DeserializeObject<List<NanJingDetail>>(jsonStr);
                    //缴费信息
                    foreach (var item in details)
                    {
                        detail = new ProvidentFundDetail();

                        detail.PayTime = item.transdate.ToDateTime();
                        detail.CompanyName = item.unitaccname;
                        if (item.reason.IndexOf("汇缴") != -1)
                        {
                            var  ptimes = CommonFun.GetMidStr(item.reason.ToTrim(), "汇缴[", "]").Split('-');
                            if (ptimes.Length==2)
                            {
                                int pmonth=ptimes[1].ToInt(0);
                                ProvidentFundTime = ptimes[0] + (pmonth >= 10 ? pmonth.ToString() : "0" + pmonth);
                            }
                            detail.ProvidentFundTime = ProvidentFundTime;
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                            detail.PersonalPayAmount = item.basenum.ToDecimal(0) / 2;//金额
                            detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                            detail.ProvidentFundBase = (detail.PersonalPayAmount / Res.PersonalMonthPayRate);
                            PaymentMonths++;
                        }
                        else
                        {
                            detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                            detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                            detail.Description = item.reason;
                            detail.PersonalPayAmount = item.payvouamt.ToDecimal(0);
                        }
                        Res.ProvidentFundDetailList.Add(detail);
                    }
                }
                while (currentPage != pageCount);
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 重庆
        /// <summary>
        /// 重庆公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Chongqing_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (httpResult.Html.Contains("function JumpSelf()"))
                {
                    string JumpSelf = CommonFun.GetMidStr(httpResult.Html, "self.location=\"/", "\";}");
                    Url = "http://www.cqgjj.cn/" + JumpSelf;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                    Url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=gr";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                }

                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true);
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }

                Url = "http://www.cqgjj.cn/Code.aspx";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "重庆公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("VIEWSTATE", VIEWSTATE);
                dics.Add("EVENTVALIDATION", EVENTVALIDATION);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重庆公积金初始化异常";
                Log4netAdapter.WriteError("重庆公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 重庆公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Chongqing_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "重庆";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.12;
            List<string> results = new List<string>();
            try
            {
                string VIEWSTATE = "/wEPDwUJNjk1ODM2NjQzZGS4KEILRkVouLSImNydGVX1TvutWw==";
                string EVENTVALIDATION = "/wEWBQLJvLzQBQKo/f6DBgK6m6qxCQKrm6aZAgKrjrnoB3KATar2K/NRjf0Ift9NUZrvF5Eg";

                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    VIEWSTATE = dics["VIEWSTATE"].ToString();
                    EVENTVALIDATION = dics["EVENTVALIDATION"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(token);
                }

                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录系统
                Url = "http://www.cqgjj.cn/Member/UserLogin.aspx?type=null";
                postdata = String.Format("__VIEWSTATE={3}&__EVENTVALIDATION={4}&txt_loginname={0}&txt_pwd={1}&txt_code={2}&but_send=", username, password, vercode, VIEWSTATE, EVENTVALIDATION);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.StatusCode != HttpStatusCode.OK || httpResult.Html.IndexOf("公积金用户登录成功") == -1)
                {
                    Res.StatusDescription = "登录失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.cqgjj.cn/Member/gr/gjjyecx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                //姓名
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_name']", "text", true);
                if (results.Count > 0)
                {
                    Res.Name = results[0].ToTrim();
                }
                else
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //身份证号码：
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_Label1']", "text", true);
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].ToTrim();
                }
                //单位名称：
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwmc']", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                //个人月缴交额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_grjje']", "text", true);
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0);
                }

                //单位月缴交额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwyje']", "text", true);
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayAmount = results[0].ToDecimal(0);
                }
                //个人公积金帐号：
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_grjjjzh']", "text", true);
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                //当前余额
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dqye']", "text", true);
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                //当前状态
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_ctl0_ContentPlaceHolder1_lb_dwzcs']", "text", true);
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }

                Res.SalaryBase = Res.PersonalMonthPayAmount / payRate;
                #endregion

                #region 第三步，缴费明细
                Url = "http://www.cqgjj.cn/Member/gr/gjjmxcx.aspx";

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='listinfo']/tbody/tr", "", true);

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }

                    detail = new ProvidentFundDetail();

                    detail.PayTime = tdRow[0].ToDateTime();
                    if (tdRow[1].IndexOf("汇缴") != -1)
                    {
                        detail.ProvidentFundTime = CommonFun.GetMidStr(tdRow[1], "汇缴[", "]").Replace("-", "");
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0);//金额
                        detail.CompanyPayAmount = tdRow[3].ToDecimal(0);//金额
                        detail.ProvidentFundBase = (detail.PersonalPayAmount / payRate);//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];
                        detail.PersonalPayAmount = 0;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 厦门
        /// <summary>
        /// 厦门公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Xiamen_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://222.76.242.141:8888/codeImage.shtml";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "厦门公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "厦门公积金初始化异常";
                Log4netAdapter.WriteError("厦门公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 厦门公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Xiamen_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "厦门";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                #region 第一步，登录系统
                Url = "http://222.76.242.141:8888/login.shtml";
                postdata = string.Format("username={0}&password={1}&securityCode2={2}&securityCode={2}", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='err_area']", "text", true);

                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://222.76.242.141:8888/queryZgzh.shtml";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Name = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:custName");

                if (Res.Name.IsEmpty())
                {
                    Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.Bank = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:aaa103");
                Res.EmployeeNo = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:custAcct");
                Res.Status = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:acctStatus") == "0" ? "正常" : "非正常";
                Res.OpenTime = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:openDate");
                Res.IdentityCard = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:idNo");
                Res.TotalAmount = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:bal").ToDecimal(0);
                string custAcct = jsonParser.GetResultFromMultiNode(httpResult.Html, "acVwPerson:custAcct");


                #endregion

                string startTime = "20000101";
                string endTime = DateTime.Now.ToString(Consts.DateFormatString5);
                #region 第三步，缴费明细
                Url = "http://222.76.242.141:8888/queryGrzhxxJson.shtml";
                postdata = string.Format("custAcct={0}&startDate={1}&endDate={2}", custAcct, startTime, endTime);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                List<XiamenDetail> details = jsonParser.DeserializeObject<List<XiamenDetail>>(jsonParser.GetResultFromParser(httpResult.Html, "list"));


                foreach (var item in details)
                {
                    detail = new ProvidentFundDetail();

                    detail.PayTime = item.centDealDate.ToDateTime(Consts.DateFormatString5);
                    if (item.centSumy == "汇缴")
                    {
                        detail.ProvidentFundTime = item.centDealDate.Substring(0, 6);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = item.creditAmt.ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = item.centSumy;
                        detail.PersonalPayAmount = 0;
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 宁波
        /// <summary>
        /// 宁波公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Ningbo_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.nbgjj.com/cha/Index_gr.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__VIEWSTATE']", "value", true);
                if (results.Count > 0)
                {
                    VIEWSTATE = results[0];
                }
                else
                {
                    Res.StatusDescription = ServiceConsts.ProvidentFund_QueryFail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='__EVENTVALIDATION']", "value", true);
                if (results.Count > 0)
                {
                    EVENTVALIDATION = results[0];
                }


                Url = "http://www.nbgjj.com/cha/AuthCode_Image.ashx?type=image&ControlID=AuthCode1&ImageStyle=ImgBgColor%3dWhite%2cImgBorderColor%3dDeepSkyBlue%2cImgNoiseColor%3dGreenYellow%2cTextColor1%3dDeepSkyBlue%2cTextColor2%3dBlueViolet%2cTextFontSize%3d17%2cWidth%3d0%2cHeight%3d0&CodeStringLength=6&ImageType=SimpleNoiseLine&CodeStringType=Number&r=814399";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "宁波公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("VIEWSTATE", VIEWSTATE);
                dics.Add("EVENTVALIDATION", EVENTVALIDATION);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "宁波公积金初始化异常";
                Log4netAdapter.WriteError("宁波公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 宁波公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Ningbo_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "宁波";
            string Url = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.08;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string VIEWSTATE = string.Empty;
                string EVENTVALIDATION = string.Empty;

                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    VIEWSTATE = dics["VIEWSTATE"].ToString();
                    EVENTVALIDATION = dics["EVENTVALIDATION"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(token);
                }

                #region 第一步，登录系统
                Url = "http://www.nbgjj.com/cha/Index_gr.aspx";
                postdata = String.Format("__VIEWSTATE={3}&__VIEWSTATEGENERATOR=A5BAA5F1&&__EVENTVALIDATION={4}&TextBox1=&TextBox2={0}&TextBox3={1}&__Vincent.AutoAuthCode.TextBoxID_AuthCode1={2}&Button1=%E7%99%BB++%E5%BD%95", username, password, vercode, VIEWSTATE.ToUrlEncode(), EVENTVALIDATION.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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
                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "<script>alert(", "');</script>");

                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.nbgjj.com/cha/grcx/grzhcx.aspx";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox1']", "value");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox2']", "value");
                if (results.Count > 0)
                {
                    Res.Status = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox3']", "value");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox12']", "value");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].ToDateTime(Consts.DateFormatString5).ToString();
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox11']", "value");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox5']", "value");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='TextBox4']", "value");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='Top1_Label2']", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                #endregion

                #region 第三步，缴费明细
                Url = "http://www.nbgjj.com/cha/grcx/grdzd.aspx?num=2&type=01";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@id='DataGrid1']/tr[position()>1]", "");

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "", true);
                    if (tdRow.Count != 5)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[3].ToDateTime(Consts.DateFormatString5);
                    if (tdRow[1] == "一般入账")
                    {
                        detail.ProvidentFundTime = tdRow[3].Substring(0, 6);
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        //detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[1];
                        detail.PersonalPayAmount = tdRow[2].ToDecimal(0) / 2;//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 杭州
        /// <summary>
        /// 杭州公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Hangzhou_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/pages/per/perLogin.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//form[@name='userLoginForm']", "action");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "杭州公积金页面初始化失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }


                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/codeMaker";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "杭州公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                //Dictionary<string, object> dics = new Dictionary<string, object>();
                //dics.Add("cookie", cookies);
                //dics.Add("url", results[0]);
                //CacheHelper.SetCache(token, dics);
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "杭州公积金初始化异常";
                Log4netAdapter.WriteError("杭州公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 杭州公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Hangzhou_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "杭州";
            string Url = string.Empty;
            string postUrl = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.12;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }
                //if (CacheHelper.GetCache(token) != null)
                //{
                //    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                //    postUrl = dics["url"].ToString();
                //    cookies = (CookieCollection)dics["cookie"];
                //    CacheHelper.RemoveCache(token);
                //}

                #region 第一步，登录系统
                Url = String.Format("http://www.hzgjj.gov.cn:8080/WebAccounts/userLogin.do?cust_no={0}&password={1}&validate_code={2}&cust_type=2&user_type=1", username, password, vercode);
                postdata = String.Format("cust_no={0}", username);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                if (httpResult.Html != "1")
                {
                    if (httpResult.Html == "2")
                    {
                        Res.StatusDescription = "验证码不正确,请重新录入";
                    }
                    if (httpResult.Html == "-1")
                    {
                        Res.StatusDescription = "输入登录名或密码不正确,请重新录入";
                    }
                    if (httpResult.Html == "3")
                    {
                        Res.StatusDescription = "用户被停用，若有疑问请咨询公积金管理中心";
                    }
                    if (httpResult.Html == "5")
                    {
                        Res.StatusDescription = "您市民邮箱的个人信息与中心不符";
                    }
                    if (httpResult.Html == "6")
                    {
                        Res.StatusDescription = "市民邮箱接口错误";
                    }
                    if (httpResult.Html == "7")
                    {
                        Res.StatusDescription = "输入的市民邮箱或密码错误,不能登录";
                    }
                    if (httpResult.Html == "8")
                    {
                        Res.StatusDescription = "您市民邮箱的个人姓名不正确";
                    }
                    if (httpResult.Html == "9")
                    {
                        Res.StatusDescription = "您市民邮箱的个人身份证号码不正确";
                    }
                    if (httpResult.Html == "10")
                    {
                        Res.StatusDescription = "该用户在业务系统中存在重复注册的情况，请选择其他方式登录";
                    }
                    if (httpResult.Html == "11")
                    {
                        Res.StatusDescription = "尊敬的客户，您的用户名有重复，请以公积金个人客户号登录，并修改用户名，谢谢！";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/userLogin.do";
                postdata = String.Format("cust_type=2&flag=1&user_type_2=1&user_type=1&cust_no={0}&password={1}&validate_code={2}&checkbox=", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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
                Res.Name = CommonFun.GetMidStr(httpResult.Html, "欢迎您，", "（客户号");
                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perComInfo.do?flag=1";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table[1]/tr[2]/td[4]/div", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToDecimal(0);
                    Res.IdentityCard = CommonFun.GetMidStr(httpResult.Html, "cert_code=", "&");
                }

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/perComInfo.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='BStyle_TB']/tr[position()>2]", "inner");
                if (results.Count == 0)
                {
                    Res.StatusDescription = "无缴费信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                else
                {
                    var Rows = HtmlParser.GetResultFromParser(results[0], "//td", "text", true);
                    if (Rows.Count > 8)
                    {
                        Res.ProvidentFundNo = Rows[1];
                        Res.CompanyName = Rows[3];
                        Res.CompanyMonthPayAmount = Rows[4].ToDecimal(0);
                        Res.PersonalMonthPayAmount = Rows[5].ToDecimal(0);
                        Res.Status = Rows[7];
                    }
                }
                postUrl = CommonFun.GetMidStrByRegex(httpResult.Html, "/WebAccounts/comPerInfo.do?", "\">查看");

                #endregion

                #region 第三步，缴费明细
                int pagenum = 1;
                bool isFinish = false;
                results.Clear();
                do
                {
                    Url = "http://www.hzgjj.gov.cn:8080/WebAccounts/comPerInfo.do?pagesize=100&pagenum=" + pagenum + "&" + postUrl;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK || httpResult.Html.Contains("无满足条件的缴存信息"))
                    {
                        isFinish = true;
                    }
                    else
                    {
                        results.AddRange(HtmlParser.GetResultFromParser(httpResult.Html, "//table[@class='BStyle_TB']/tr[position()>2]", "inner"));
                        pagenum++;
                    }
                }
                while (!isFinish);

                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 8)
                    {
                        continue;
                    }
                    detail = new ProvidentFundDetail();
                    detail.PayTime = tdRow[1].ToDateTime(Consts.DateFormatString7);
                    if (tdRow[2] == "汇缴")
                    {
                        detail.ProvidentFundTime = tdRow[1];
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0);//金额
                        detail.CompanyPayAmount = tdRow[4].ToDecimal(0);//金额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[2];
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) / 2;//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 无锡
        /// <summary>
        /// 无锡公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Wuxi_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "http://www.wxgjj.com.cn/jcaptcha";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "无锡公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "无锡公积金初始化异常";
                Log4netAdapter.WriteError("无锡公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 无锡公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Wuxi_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "无锡";
            string Url = string.Empty;
            string postUrl = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.12;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }

                #region 第一步，登录系统
                Url = "http://www.wxgjj.com.cn/logon.do";
                postdata = String.Format("logontype=2&loginname={0}&type=person&password={1}&_login_checkcode={2}&x=36&y=9", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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

                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "出现错误！", "，请核实！");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.wxgjj.com.cn/phoneNum.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }

                Url = "http://www.wxgjj.com.cn/zg_info.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToTrim("(元)").ToTrim().ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToTrim("(元)").ToTrim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToTrim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToTrim("%").ToTrim().ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToTrim("%").ToTrim().ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].ToDateTime(Consts.DateFormatString4).ToString();
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                #endregion

                #region 第三步，缴费明细
                Url = "http://www.wxgjj.com.cn/mx_info.do";
                postdata = "zjlx=1&hjstatus=%D5%FD%B3%A3%BB%E3%BD%C9&submit=%B2%E9++%D1%AF";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table/tr/td[2]/div/table/tr[position()>1]", "");

                string payTime = string.Empty;
                string fundTime = string.Empty;
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }
                    payTime = CommonFun.GetMidStr(tdRow[1], "cc = \"", "\";");
                    fundTime = CommonFun.GetMidStr(tdRow[2], "bb = \"", "\";");
                    detail = new ProvidentFundDetail();
                    detail.PayTime = payTime.ToDateTime(Consts.DateFormatString5);
                    if (tdRow[3] == "正常汇缴")
                    {
                        detail.ProvidentFundTime = fundTime;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[4].ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[2];
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) / 2;//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 苏州
        /// <summary>
        /// 苏州公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Suzhou_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "https://gr.szgjj.gov.cn/retail/validateCodeServlet";
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
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "苏州公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "苏州公积金初始化异常";
                Log4netAdapter.WriteError("苏州公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 苏州公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Suzhou_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "苏州";
            string Url = string.Empty;
            string postUrl = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.12;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }

                string custacno = username;
                string logontype = "1";
                string paperid = password;

                if (custacno.IndexOf("-") != -1)
                {
                    custacno = custacno.Substring(0, custacno.IndexOf('-'));
                }
                if (custacno.Length < 10)
                {
                    custacno = "0000000000" + custacno;
                    custacno = custacno.Substring(custacno.Length - 10);
                }
                if (custacno.Length > 11)
                {
                    logontype = "2";
                }
                #region 第一步，登录系统
                Url = "https://gr.szgjj.gov.cn/retail/service";
                postdata = String.Format("service=com.jbsoft.i2hf.retail.services.UserLogon.unRegUserLogon&custacno={0}&paperid={1}&paperkind=A&logontype={3}&validateCode={2}", custacno, paperid, vercode, logontype);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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

                string errorInfo = CommonFun.GetMidStrByRegex(httpResult.Html, "错误信息</font>:", "<br />");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                string sid = CommonFun.GetMidStr(httpResult.Html, "window.parent.location='internet?sid=", "' + '");
                #endregion

                #region 第二步，查询个人基本信息

                Url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid + "&service=com.jbsoft.i2hf.retail.services.UserAccService.getBaseAccountInfo";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].ToTrim("&nbsp;");
                }
                else
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0].ToTrim("&nbsp;");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToTrim("&nbsp;").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Status = results[0].ToTrim("&nbsp;");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0].ToTrim("&nbsp;");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToTrim("&nbsp;").ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToTrim("&nbsp;").ToDecimal(0) / 100;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[1]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0].ToTrim("&nbsp;");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToTrim("&nbsp;").ToDecimal(0)/2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[3]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0].ToTrim("&nbsp;").ToTrim("-");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[4]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].ToTrim("&nbsp;");
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/div[2]/table/tr[8]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToTrim("&nbsp;").ToDecimal(0) / 100;
                }

                #endregion

                #region 第三步，缴费明细
                string acdateFrom = string.Empty;
                string acdateTo = string.Empty;
                int page = 0;
                int pages = 0;
                DateTime nowDate = DateTime.Now;
                List<SuzhouDetail> details = new List<SuzhouDetail>();
                //for (int i = 0; i <= 3; i++)
                //{
                //    acdateTo = nowDate.AddYears(-i).ToString(Consts.DateFormatString5) + " 00:00:00";
                //    acdateFrom = nowDate.AddYears(-i - 1).ToString(Consts.DateFormatString5) + " 23:59:59";
                //    page = 0;
                //    do
                //    {
                //        page++;
                //        Url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid;
                //        postdata = string.Format("service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfoJSON&acdateFrom={0}&acdateTo={1}&busidetailtype=101&page={2}", acdateFrom, acdateTo,page);
                //        httpItem = new HttpItem()
                //        {
                //            URL = Url,
                //            Method = "post",
                //            Postdata = postdata,
                //            CookieCollection = cookies,
                //            ResultCookieType = ResultCookieType.CookieCollection
                //        };
                //        httpResult = httpHelper.GetHtml(httpItem);
                //        pages = jsonParser.GetResultFromMultiNode(httpResult.Html, "info:pages").ToInt(0);
                //        details.AddRange(jsonParser.DeserializeObject<List<SuzhouDetail>>(jsonParser.GetResultFromParser(httpResult.Html, "recoreds")));
                //    }
                //    while (page<pages);
                //}
                acdateFrom = nowDate.AddYears(-3).ToString(Consts.DateFormatString5) + " 00:00:00";
                acdateTo = nowDate.ToString(Consts.DateFormatString5) + " 23:59:59";
                page = 0;
                do
                {
                    page++;
                    Url = "https://gr.szgjj.gov.cn/retail/internet?sid=" + sid;
                    postdata = string.Format("service=com.jbsoft.i2hf.retail.services.UserAccService.getDetailAccountInfoJSON&acdateFrom={0}&acdateTo={1}&busidetailtype=101&page={2}", acdateFrom, acdateTo, page);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        Postdata = postdata,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    pages = jsonParser.GetResultFromMultiNode(httpResult.Html, "info:pages").ToInt(0);
                    details.AddRange(jsonParser.DeserializeObject<List<SuzhouDetail>>(jsonParser.GetResultFromParser(httpResult.Html, "recoreds")));
                }
                while (page < pages);

                foreach (var item in details)
                {
                    detail = new ProvidentFundDetail();
                    detail.PayTime = item.acdate.ToDateTime(Consts.DateFormatString2);

                    detail.ProvidentFundTime = item.savemonth.ToTrim("-");
                    detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                    detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                    detail.PersonalPayAmount = item.amt.ToDecimal(0) / 2;//金额
                    detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                    //detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                    PaymentMonths++;

                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

        #region 苏州工业园区
        /// <summary>
        /// 苏州工业园区公积金，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Suzhou_GYYQ_Init()
        {

            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = "http://www.sipspf.org.cn/person_online/emp/loginend.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string sessionid = CommonFun.GetMidStrByRegex(httpResult.Html, "var sessionid = \"", "\";");

                Url = "http://www.sipspf.org.cn/person_online/service/identify.do?sessionid=" + sessionid;
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

                Url = "http://www.sipspf.org.cn/person_online/service/problem.do?sessionid=" + sessionid;
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
                string answer = string.Empty;
                int num1 = 0;
                int num2 = 0;
                if (httpResult.Html.Contains("加"))
                {
                    num1 = CommonFun.GetMidStrByRegex(httpResult.Html, "", "加").ToInt(0);
                    num2 = CommonFun.GetMidStrByRegex(httpResult.Html, "加", "等于多少?").ToInt(0);
                    answer = (num1 + num2).ToString();
                }
                if (httpResult.Html.Contains("减"))
                {
                    num1 = CommonFun.GetMidStrByRegex(httpResult.Html, "", "减").ToInt(0);
                    num2 = CommonFun.GetMidStrByRegex(httpResult.Html, "减", "等于多少?").ToInt(0);
                    answer = (num1 - num2).ToString();
                }
                if (httpResult.Html.Contains("乘"))
                {
                    num1 = CommonFun.GetMidStrByRegex(httpResult.Html, "", "乘").ToInt(0);
                    num2 = CommonFun.GetMidStrByRegex(httpResult.Html, "乘", "等于多少?").ToInt(0);
                    answer = (num1 * num2).ToString();
                }
                if (httpResult.Html.Contains("除以"))
                {
                    num1 = CommonFun.GetMidStrByRegex(httpResult.Html, "", "除以").ToInt(0);
                    num2 = CommonFun.GetMidStrByRegex(httpResult.Html, "除以", "等于多少?").ToInt(0);
                    answer = (num1 / num2).ToString();
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.All);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusCode = ServiceConsts.StatusCode_success;


                Res.StatusDescription = "苏州工业园区公积金查询已初始化";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("answer", answer);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "苏州工业园区公积金初始化异常";
                Log4netAdapter.WriteError("苏州工业园区公积金初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 苏州工业园区公积金登录、查询
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public ProvidentFundQueryRes Suzhou_GYYQ_GetProvidentFund(string username, string password, string token, string vercode)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detail = null;
            Res.ProvidentFundCity = "苏州工业园区";
            string Url = string.Empty;
            string postUrl = string.Empty;
            string postdata = string.Empty;
            int PaymentMonths = 0;
            decimal payRate = (decimal)0.12;
            List<string> results = new List<string>();
            try
            {
                //校验参数
                if (username.IsEmpty() || password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    CacheHelper.RemoveCache(token);
                }

                #region 第一步，登录系统
                Url = "http://www.sipspf.org.cn/person_online/service/EMPLogin/login?wqcall=1417161422909";
                postdata = String.Format("logontype=2&loginname={0}&type=person&password={1}&_login_checkcode={2}&x=36&y=9", username, password, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
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

                string errorInfo = CommonFun.GetMidStr(httpResult.Html, "出现错误！", "，请核实！");
                if (!errorInfo.IsEmpty())
                {
                    Res.StatusDescription = errorInfo;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                #endregion

                #region 第二步，查询个人基本信息

                Url = "http://www.wxgjj.com.cn/phoneNum.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[1]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Name = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/form/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.Phone = results[0];
                }

                Url = "http://www.wxgjj.com.cn/zg_info.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[2]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.IdentityCard = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[3]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayAmount = results[0].ToTrim("(元)").ToTrim().ToDecimal(0) / 2;
                    Res.CompanyMonthPayAmount = Res.PersonalMonthPayAmount;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[4]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.TotalAmount = results[0].ToTrim("(元)").ToTrim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[5]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.SalaryBase = results[0].ToTrim().ToDecimal(0);
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[6]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.PersonalMonthPayRate = results[0].ToTrim("%").ToTrim().ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[7]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyMonthPayRate = results[0].ToTrim("%").ToTrim().ToDecimal(0) / 100;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[8]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.LastProvidentFundTime = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[9]/td[2]", "text");
                if (results.Count > 0)
                {
                    Res.OpenTime = results[0].ToDateTime(Consts.DateFormatString4).ToString();
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div/table/tr[2]/td[4]", "text");
                if (results.Count > 0)
                {
                    Res.CompanyName = results[0];
                }
                #endregion

                #region 第三步，缴费明细
                Url = "http://www.wxgjj.com.cn/mx_info.do";
                postdata = "zjlx=1&hjstatus=%D5%FD%B3%A3%BB%E3%BD%C9&submit=%B2%E9++%D1%AF";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/table/tr/td[2]/div/table/tr[position()>1]", "");

                string payTime = string.Empty;
                string fundTime = string.Empty;
                foreach (string item in results)
                {
                    var tdRow = HtmlParser.GetResultFromParser(item, "//td", "text", true);
                    if (tdRow.Count != 7)
                    {
                        continue;
                    }
                    payTime = CommonFun.GetMidStr(tdRow[1], "cc = \"", "\";");
                    fundTime = CommonFun.GetMidStr(tdRow[2], "bb = \"", "\";");
                    detail = new ProvidentFundDetail();
                    detail.PayTime = payTime.ToDateTime(Consts.DateFormatString5);
                    if (tdRow[3] == "正常汇缴")
                    {
                        detail.ProvidentFundTime = fundTime;
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;
                        detail.PersonalPayAmount = tdRow[4].ToDecimal(0) / 2;//金额
                        detail.CompanyPayAmount = detail.PersonalPayAmount;//金额
                        detail.ProvidentFundBase = detail.PersonalPayAmount / payRate;//缴费基数
                        PaymentMonths++;
                    }
                    else
                    {
                        detail.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Adjust;
                        detail.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Adjust;
                        detail.Description = tdRow[2];
                        detail.PersonalPayAmount = tdRow[3].ToDecimal(0) / 2;//金额
                    }
                    Res.ProvidentFundDetailList.Add(detail);
                }
                #endregion

                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(Res.ProvidentFundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
        #endregion

    }
}
