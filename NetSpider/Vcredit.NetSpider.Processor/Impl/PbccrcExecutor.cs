using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using System.Threading.Tasks;
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class PbccrcExecutor : IPbccrcExecutor
    {
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();

        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string forwardedIp = GenerateIP();
        /// <summary>
        /// 央行互联网征信页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Init()
        {
            VerCodeRes baseRes = new VerCodeRes();
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            baseRes.Token = token;
            string proxyip = string.Empty;
          
            try
            {
                //proxyip = GetProxyIP();
               
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                httpItem = new HttpItem()
                {
                    URL = "https://ipcrs.pbccrc.org.cn/login.do?method=initLogin",
                    CookieCollection = cookies,
                    //ProxyIp = proxyip.IsEmpty() ? "" : proxyip,
                    Referer = "https://ipcrs.pbccrc.org.cn/index1.do",
                    SecurityProtocolType = SecurityProtocolType.Tls,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Log4netAdapter.WriteInfo(httpResult.Html);
                    baseRes.StatusDescription = httpResult.Html;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }

                httpItem = new HttpItem()
                {
                    URL = "https://ipcrs.pbccrc.org.cn/imgrc.do",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    //ProxyIp = proxyip.IsEmpty() ? "" : proxyip,
                    SecurityProtocolType = SecurityProtocolType.Tls,
                    Referer = "https://ipcrs.pbccrc.org.cn/page/login/loginreg.jsp",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                baseRes.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                baseRes.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                baseRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("央行征信查询初始化失败", e);
            }
            return baseRes;
        }
        /// <summary>
        /// 央行互联网征信登录
        /// </summary>
        /// <param name="token">会话令牌</param>
        /// <param name="loginname">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="verCode">图片验证码</param>
        /// <returns></returns>
        public BaseRes Login(string token, string loginname, string password, string verCode)
        {
            Log4netAdapter.WriteInfo(string.Format("央行互联网征信登录，登录名：{0}，密码：{1},token:{2}", loginname, password, token));

            BaseRes baseRes = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            List<string> results = new List<string>();

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //添加缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    //CacheHelper.RemoveCache(token);
                }
                //校验参数
                if (loginname.IsEmpty() || password.IsEmpty())
                {
                    baseRes.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }

                //string answer = RSAHelper.EncryptStringByRsaJS("000000", "ddc1b17fe4c89d81461d885b81b261f16ce20e5170810f87319fa34233b437aa33c71fc111aa9256af607b997c51ba5cf6f537fa75ad7425c32049e9443082756e002c966bdf82a9febae17369faf215c7d82baa8afd973ac92ba8d33eb779ec024dba1a451805b47510237c5e5901da59e7a818896160a76cd32171a35e8034307a9828118c318745499dc491186c2748f225e6817a9d959ac143b4e0e5896d17e53f9c4a03e7d7ecf3947c2ed6cbe6058c61dd9a44637844c11f0a4308dae5de5bd24519e5e09ea60f4ec81f32f8ae8fe55c4237c607c15b17158cf5ae91268c6a76a8e6ced80fafc8969a09db41dc07a9a6c18bc060885d0fede70ca33aa1");

                //第一步,登录请求
                Url = "https://ipcrs.pbccrc.org.cn/login.do";
                Postdata = String.Format("method=login&loginname={0}&password={1}&_@IMGRC@_={2}", loginname, password, verCode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    Referer = "https://ipcrs.pbccrc.org.cn/login.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
               
                List<string> _error_field = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_error_field_']", "text", true);
                if (_error_field.Count > 0)
                {
                    if (_error_field[0].IsEmpty())
                    {
                        _error_field = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_@MSG@_']", "text", true);
                    }
                    baseRes.StatusDescription = _error_field[0];
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    CacheHelper.RemoveCache(token);
                }
                else if (httpResult.Html.Contains("regularModifyPWForm"))
                {
                    baseRes.StatusDescription = "密码已过期，请去官网修改密码";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    CacheHelper.RemoveCache(token);
                }
                else if (httpResult.Html.Contains("centerDiv"))
                {
                    baseRes.StatusDescription = "登录成功";
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;

                    //添加缓存
                    Dictionary<string, object> dics = new Dictionary<string, object>();
                    dics.Add("cookie", cookies);
                    dics.Add("loginname", loginname);
                    CacheHelper.SetCache(token, dics);
                }
                Task.Factory.StartNew(() =>
                {
                    if (baseRes.StatusCode == ServiceConsts.StatusCode_success)
                    {
                        ICRD_ACCOUNT service = NetSpiderFactoryManager.GetCRDACCOUNTService();
                        int count = service.CountHql("from CRD_ACCOUNTEntity where UserName=?", new object[] { loginname });
                        if (count == 0)
                        {
                            CRD_ACCOUNTEntity accountEntity = new CRD_ACCOUNTEntity() { UserName = loginname, Password = password };
                            service.Save(accountEntity);
                        }
                    }
                });

                Log4netAdapter.WriteInfo("央行互联网征信登录,接口调用结束,token:" + token);

            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("央行互联网征信登录异常", e);
            }
            return baseRes;
        }
        public BaseRes QueryApplication_Result(string token)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("央行互联网征信查询码申请结果查询,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    loginname = dics["loginname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //第一步，检查是否已经提交申请
                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=applicationReport";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/menu.do",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@class='span-red span-12']", "text");

                results = results.Where(o=>!o.IsEmpty()).ToList();
                if (results.Count > 0)
                {
                    if (results[0] == "(已生成)")
                    {
                        Res.Result = "已生成";
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                    }
                    else if (results[0] == "(加工成功)")
                    {
                        Res.Result = "已生成";
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                    }
                    else if (results[0] == "(处理中)")
                    {
                        Res.Result = "处理中";
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                    }
                    else if (results[0] == "(验证未通过)")
                    {
                        Res.Result = "验证未通过";
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                    }    
                }
                else
                {
                    Res.Result = "无信息";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }

                Log4netAdapter.WriteInfo("央行互联网征信查询码申请结果查询,结束,token:" + token);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "央行互联网征信查询码申请结果查询异常";
                Log4netAdapter.WriteError("央行互联网征信查询码申请结果查询异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 申请信用信息查询，第一步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="isContinue">如果已有报告，是否继续重新申请</param>
        /// <returns></returns>
        public BaseRes QueryApplication_Step1(string token, bool isContinue, string identitycard)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("央行互联网征信查询码申请第一步,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    loginname = dics["loginname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    //CacheHelper.RemoveCache(token);
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //第一步，检查是否已经提交申请
                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=applicationReport";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/menu.do",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@class='span-red span-12']", "text");
                if (results.Count > 0)
                {
                    //判断是否点继续按钮
                    if (!isContinue)
                    {
                        foreach (string item in results)
                        {
                            if (item == "(加工成功)")
                            {
                                Res.StatusDescription = "您的个人信用信息报告已存在，可以直接调用报告查询接口查询。";
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                            if (item == "(已生成)")
                            {
                                Res.StatusDescription = "您的个人信用信息报告已存在，可以直接调用报告查询接口查询。";
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                        }
                    }
                    //判断是否有处理中的报告
                    foreach (string item in results)
                    {
                        if (item == "(处理中)")
                        {
                            Res.StatusDescription = "您的信用信息查询请求已提交，请在24小时后访问平台获取结果。为保障您的信息安全，您申请的信用信息将于7日后自动清理，请及时获取查询结果。";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                    }
                }
                //获取TOKEN
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                if (results.Count > 0)
                {
                    TOKEN = results[0];
                }


                //第二步，提交问题验证选项后获取问题验证列表
                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport";
                Postdata = String.Format("org.apache.struts.taglib.html.TOKEN={0}&method=checkishasreport&authtype=2&ApplicationOption=25&ApplicationOption=24&ApplicationOption=21", TOKEN);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    CookieCollection = cookies,
                    Encoding = Encoding.GetEncoding("gb2312"),
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=applicationReport",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='erro_div1']", "text", true);
                if (results.Count > 0 && !results[0].IsEmpty())
                {
                    if (results[0].Contains("目前系统尚未收录足够的信息对您的身份进行“问题验证”。请您选择其他验证方式，继续完成验证"))
                    {
                        ////添加征信空白记录
                        //if (!identitycard.IsEmpty())
                        //{
                        //    Task.Factory.StartNew(() =>
                        //    {
                        //        ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                        //        CRD_HD_REPORTEntity reportEntity = new CRD_HD_REPORTEntity();
                        //        reportEntity.BusType = "creditblank";
                        //        reportEntity.BusIdentityCard = identitycard;
                        //        reportEntity.CertNo = identitycard;
                        //        reportEntity.CertType = "0";
                        //        reportEntity.Name = "";
                        //        reportService.Save(reportEntity);
                        //    });
                        //}
                        Res.StatusDescription = "目前系统尚未收录足够的信息对您的身份进行“问题验证”";
                        Res.StatusCode = ServiceConsts.StatusCode_NetCredit_NoQuestion;
                        return Res;
                    }
                    else
                    {
                        Res.StatusDescription = results[0];
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                }

                //判断央行征信网是否已存在报告
                if (httpResult.Html.Contains("您的个人信用信息产品已存在，请点击\"获取信用信息\"查看。若继续申请查询，现有的个人信用信息产品将不再保留，是否继续"))
                {
                    if (!isContinue)
                    {
                        Res.StatusDescription = "您的个人信用信息报告已存在，可以直接调用报告查询接口查询。";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    else
                    {
                        Url = "https://ipcrs.pbccrc.org.cn/reportAction.do";
                        Postdata = String.Format("org.apache.struts.taglib.html.TOKEN={0}&method=verify&authtype=2&ApplicationOption=25&ApplicationOption=24&ApplicationOption=21", TOKEN);

                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = Postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection,
                            Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport"
                        };
                        if (!forwardedIp.IsEmpty())
                        {
                            httpItem.Header.Add("x-forwarded-for", forwardedIp);
                        }
                        httpResult = httpHelper.GetHtml(httpItem);
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                if (results.Count > 0)
                {
                    TOKEN = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[0].derivativecode']", "");

                CRD_KbaQuestion kbaEntity = null;
                List<CRD_KbaQuestion> kbaList = new List<CRD_KbaQuestion>();
                if (results.Count > 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        kbaEntity = new CRD_KbaQuestion();
                        kbaEntity.derivativecode = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].derivativecode']", "value")[0];
                        kbaEntity.businesstype = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].businesstype']", "value")[0];
                        kbaEntity.questionno = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].questionno']", "value")[0];
                        kbaEntity.kbanum = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].kbanum']", "value")[0];
                        kbaEntity.question = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].question']", "value")[0];
                        kbaEntity.options1 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].options1']", "value")[0];
                        kbaEntity.options2 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].options2']", "value")[0];
                        kbaEntity.options3 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].options3']", "value")[0];
                        kbaEntity.options4 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].options4']", "value")[0];
                        kbaEntity.options5 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='kbaList[" + i + "].options5']", "value")[0];
                        kbaEntity.answerresult = "0";
                        kbaList.Add(kbaEntity);
                    }
                    Res.StatusDescription = "已获取验证问题";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    Res.Result = jsonParser.SerializeObject(kbaList);

                    //添加缓存
                    Dictionary<string, object> dics = new Dictionary<string, object>();
                    dics.Add("cookie", cookies);//cookie
                    dics.Add("loginname", loginname);//登录名
                    dics.Add("token", TOKEN);//页面生成的token
                    CacheHelper.SetCache(token, dics);
                }
                else
                {
                    Res.StatusDescription = "征信空白";
                    Res.StatusCode = ServiceConsts.StatusCode_NetCredit_NoQuestion;
                }
                Log4netAdapter.WriteInfo("央行互联网征信查询码申请第一步,结束,token:" + token);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("央行征信查询码，获取验证问题异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 填写征信空白
        /// </summary>
        /// <param name="identitycard">身份证号</param>
        /// <param name="name">姓名</param>
        /// <returns></returns>
        public BaseRes QueryApplication_AddBlankRecord(string certNo, string name)
        {
            BaseRes Res = new BaseRes();

            Log4netAdapter.WriteInfo("央行互联网征信填写征信空白,开始,certNo:" + certNo);
            try
            {
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                CRD_HD_REPORTEntity reportEntity = reportService.GetByIdentityCard(certNo, false);
                if (reportEntity == null)
                {
                    reportEntity = new CRD_HD_REPORTEntity();
                    reportEntity.BusType = "CREDITBLANK";
                    reportEntity.BusIdentityCard = certNo;
                    reportEntity.CertNo = certNo;
                    reportEntity.Name = name;
                    reportEntity.ReportSn = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    reportService.Save(reportEntity);
                }
                var result = new { ReportId = reportEntity.Id, ReportSn = reportEntity.ReportSn };
                Res.Result = jsonParser.SerializeObject(result);
                Res.StatusDescription = "填写征信空白成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Log4netAdapter.WriteInfo("央行互联网征信填写征信空白,结束,certNo:" + certNo);
            }
            catch (Exception e)
            {
                Res.StatusDescription = "填写征信空白失败";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                Log4netAdapter.WriteInfo(string.Format("央行互联网征信填写征信空白,异常,错误信息[certNo:{0}]：{1}", certNo, e.ToString()));
            }

            return Res;
        }

        /// <summary>
        /// 申请信用信息查询，第二步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="kbaList">验证问题集合</param>
        /// <returns></returns>
        public BaseRes QueryApplication_Step2(string token, List<CRD_KbaQuestion> kbaList)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("央行互联网征信查询码申请第二步,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    loginname = dics["loginname"].ToString();
                    TOKEN = dics["token"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    //CacheHelper.RemoveCache(token);
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //整理post参数
                //for (int i = 0; i < kbaList.Count; i++)
                //{
                //    Postdata += "kbaList[" + i + "].derivativecode=" + kbaList[i].derivativecode + "&";
                //    Postdata += "kbaList[" + i + "].businesstype=" + kbaList[i].businesstype + "&";
                //    Postdata += "kbaList[" + i + "].questionno=" + kbaList[i].questionno + "&";
                //    Postdata += "kbaList[" + i + "].kbanum=" + kbaList[i].kbanum + "&";
                //    Postdata += "kbaList[" + i + "].question=" + kbaList[i].question + "&";
                //    Postdata += "kbaList[" + i + "].options1=" + kbaList[i].options1 + "&";
                //    Postdata += "kbaList[" + i + "].options2=" + kbaList[i].options2 + "&";
                //    Postdata += "kbaList[" + i + "].options3=" + kbaList[i].options3 + "&";
                //    Postdata += "kbaList[" + i + "].options4=" + kbaList[i].options4 + "&";
                //    Postdata += "kbaList[" + i + "].options5=" + kbaList[i].options5 + "&";
                //    Postdata += "kbaList[" + i + "].answerresult=" + kbaList[i].answerresult + "&";
                //}
                Postdata = "org.apache.struts.taglib.html.TOKEN=" + TOKEN;
                Postdata += "&method=&authtype=2&ApplicationOption=25&ApplicationOption=24&ApplicationOption=21";

                for (int i = 0; i < kbaList.Count; i++)
                {
                    Postdata += "&kbaList%5B" + i + "%5D.derivativecode=" + kbaList[i].derivativecode.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.businesstype=" + kbaList[i].businesstype.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.questionno=" + kbaList[i].questionno.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.kbanum=" + kbaList[i].kbanum.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.question=" + kbaList[i].question.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.options1=" + kbaList[i].options1.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.options2=" + kbaList[i].options2.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.options3=" + kbaList[i].options3.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.options4=" + kbaList[i].options4.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.options5=" + kbaList[i].options5.ToUrlEncode();
                    Postdata += "&kbaList%5B" + i + "%5D.answerresult=" + kbaList[i].answerresult;
                    Postdata += "&kbaList%5B" + i + "%5D.options=" + kbaList[i].answerresult;
                }

                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=submitKBA";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                //数据保存
                Task.Factory.StartNew(() =>
                {
                    ICRD_ACCOUNT accService = NetSpiderFactoryManager.GetCRDACCOUNTService();
                    CRD_ACCOUNTEntity acc = accService.Find("from CRD_ACCOUNTEntity where UserName=?", new object[] { loginname }).FirstOrDefault();
                    if (acc != null)
                    {
                        List<CRD_KbaQuestionEntity> kbaEntityList = new List<CRD_KbaQuestionEntity>();
                        CRD_KbaQuestionEntity kbaEntity = null;
                        foreach (var kbaItem in kbaList)
                        {
                            kbaEntity = new CRD_KbaQuestionEntity();
                            kbaEntity.AccountId = acc.Id;
                            kbaEntity.Answer = kbaItem.answer;
                            kbaEntity.Answerresult = kbaItem.answerresult;
                            kbaEntity.Businesstype = kbaItem.businesstype;
                            kbaEntity.Derivativecode = kbaItem.derivativecode;
                            kbaEntity.Question = kbaItem.question;
                            kbaEntity.Questionno = kbaItem.questionno;
                            kbaEntity.Kbanum = kbaItem.kbanum;
                            kbaEntity.Options1 = kbaItem.options1;
                            kbaEntity.Options2 = kbaItem.options2;
                            kbaEntity.Options3 = kbaItem.options3;
                            kbaEntity.Options4 = kbaItem.options4;
                            kbaEntity.Options5 = kbaItem.options5;
                            kbaEntityList.Add(kbaEntity);
                        }
                        ICRD_KbaQuestion kbaService = NetSpiderFactoryManager.GetCRDKbaQuestionService();
                        kbaService.SaveAll(kbaEntityList);
                    }

                });
                if (httpResult.Html.Contains("您的查询申请已提交"))
                {
                    Res.StatusDescription = "信用信息查询码获取请求已提交";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                    //Res.Result = jsonParser.SerializeObject(kbaList);
                }
                else
                {
                    Res.StatusDescription = "信用信息查询码获取请求失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='erro_div1']", "");
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = CommonFun.ClearFlag(results[0]);
                    }
                }
                Log4netAdapter.WriteInfo("央行互联网征信查询码申请第二步,结束,token:" + token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "信用信息查询码获取请求异常";
                Log4netAdapter.WriteError("信用信息查询码获取请求异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 银行卡申请信用信息查询，第一步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="isContinue">如果已有报告，是否继续重新申请</param>
        /// <returns></returns>
        public VerCodeRes QueryApplication_CreditCard_Step1(string token, bool isContinue)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请第一步,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    loginname = dics["loginname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    //CacheHelper.RemoveCache(token);
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //第一步，检查是否已经提交申请
                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=applicationReport";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/menu.do",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@class='span-red span-12']", "text");
                if (results.Count > 0)
                {
                    //判断是否点继续按钮
                    if (!isContinue)
                    {
                        foreach (string item in results)
                        {
                            if (item == "(加工成功)")
                            {
                                Res.StatusDescription = "您的个人信用信息报告已存在，可以直接调用报告查询接口查询。";
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                            if (item == "(已生成)")
                            {
                                Res.StatusDescription = "您的个人信用信息报告已存在，可以直接调用报告查询接口查询。";
                                Res.StatusCode = ServiceConsts.StatusCode_fail;
                                return Res;
                            }
                        }
                    }
                    //判断是否有处理中的报告
                    foreach (string item in results)
                    {
                        if (item == "(处理中)")
                        {
                            Res.StatusDescription = "您的信用信息查询请求已提交，请在24小时后访问平台获取结果。为保障您的信息安全，您申请的信用信息将于7日后自动清理，请及时获取查询结果。";
                            Res.StatusCode = ServiceConsts.StatusCode_fail;
                            return Res;
                        }
                    }
                }
                //获取TOKEN
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                if (results.Count > 0)
                {
                    TOKEN = results[0];
                }

                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport";
                Postdata = String.Format("org.apache.struts.taglib.html.TOKEN={0}&method=checkishasreport&authtype=3&ApplicationOption=25&ApplicationOption=24&ApplicationOption=21", TOKEN);

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    CookieCollection = cookies,
                    Encoding = Encoding.GetEncoding("gb2312"),
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=applicationReport",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='erro_div1']", "text", true);
                if (results.Count > 0 && !results[0].IsEmpty())
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //判断央行征信网是否已存在报告
                if (httpResult.Html.Contains("您的个人信用信息产品已存在，请点击\"获取信用信息\"查看。若继续申请查询，现有的个人信用信息产品将不再保留，是否继续"))
                {
                    if (!isContinue)
                    {
                        Res.StatusDescription = "您的个人信用信息报告已存在，可以直接调用报告查询接口查询。";
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    else
                    {
                        Url = "https://ipcrs.pbccrc.org.cn/reportAction.do";
                        Postdata = String.Format("org.apache.struts.taglib.html.TOKEN={0}&method=verify&authtype=3&ApplicationOption=25&ApplicationOption=24&ApplicationOption=21", TOKEN);

                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "POST",
                            Postdata = Postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection,
                            Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport"
                        };
                        if (!forwardedIp.IsEmpty())
                        {
                            httpItem.Header.Add("x-forwarded-for", forwardedIp);
                        }
                        httpResult = httpHelper.GetHtml(httpItem);
                    }
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                if (results.Count > 0)
                {
                    TOKEN = results[0];
                }

                //第二步，获取验证码及获取认证码所需的页面
                Url = "https://ipcrs.pbccrc.org.cn/imgrc.do";//?0.4572030928407854";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    SecurityProtocolType = SecurityProtocolType.Tls,
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                Res.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);


                Url = "https://ipcrs.pbccrc.org.cn/unionpayAction.do";
                Postdata = "method=getunionpaycode";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do"
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Res.StatusDescription = "央行互联网征信银行卡查询码申请页面初始化成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = httpResult.Html;

                //添加缓存
                Dictionary<string, object> dic = new Dictionary<string, object>();
                //dic.Add("html", httpResult.Html);//获取认证码页面
                dic.Add("cookie", cookies);//cookie
                dic.Add("loginname", loginname);//登录名
                dic.Add("token", TOKEN);//页面生成的token
                CacheHelper.SetCache(token, dic);
                Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请第一步,结束,token:" + token);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError("央行征信银行卡查询码，获取跳转页面异常", e);
            }

            return Res;
        }
        /// <summary>
        /// 银行卡申请信用信息查询，第二步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="kbaList">验证问题集合</param>
        /// <returns></returns>
        public BaseRes QueryApplication_CreditCard_Step2(string token, string unionpaycode, string vercode)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请第二步,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    loginname = dics["loginname"].ToString();
                    TOKEN = dics["token"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    //CacheHelper.RemoveCache(token);
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=submitUPA";
                Postdata = string.Format("org.apache.struts.taglib.html.TOKEN={0}&counttime=0&authtype=3&ApplicationOption=25&ApplicationOption=24&ApplicationOption=21&code={1}&_%40IMGRC%40_={2}", TOKEN, unionpaycode, vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=checkishasreport",
                    Encoding = Encoding.GetEncoding("gb2312"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                if (results.Count > 0)
                {
                    TOKEN = results[0];
                }

                if (httpResult.Html.Contains("您的信用信息查询请求已提交"))
                {
                    Res.StatusDescription = "信用信息查询码获取请求已提交";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Res.StatusDescription = "信用信息查询码获取请求失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;

                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_error_field_']");
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = CommonFun.ClearFlag(results[0]);
                    }

                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("cookie", cookies);//cookie
                    dic.Add("loginname", loginname);//登录名
                    dic.Add("token", TOKEN);//页面生成的token
                    CacheHelper.SetCache(token, dic);
                }
                Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请第二步,结束,token:" + token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "信用信息查询码获取请求异常";
                Log4netAdapter.WriteError("信用信息查询码获取请求异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 银联认证页面初始化
        /// </summary>
        /// <param name="html">银联认证页面信息</param>
        /// <returns></returns>
        public BaseRes QueryApplication_GetUnionPayCode_Init(string html)
        {
            BaseRes Res = new BaseRes();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            string Url = string.Empty;
            string Postdata = string.Empty;
            Dictionary<string, object> dics = new Dictionary<string, object>();
            Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码初始化,开始,token:" + token);

            try
            {
                byte[] bhtml = Convert.FromBase64String(html);
                html = System.Text.Encoding.Default.GetString(bhtml);
                html = html.ToUrlDecode();
                string location = HtmlParser.GetResultFromParser(html, "//form", "action")[0];
                string version = HtmlParser.GetResultFromParser(html, "//input[@name='version']", "value")[0];
                string charset = HtmlParser.GetResultFromParser(html, "//input[@name='charset']", "value")[0];
                string certorgcode = HtmlParser.GetResultFromParser(html, "//input[@name='certorgcode']", "value")[0];
                string receiveorgcode = HtmlParser.GetResultFromParser(html, "//input[@name='receiveorgcode']", "value")[0];
                string encrypmethod = HtmlParser.GetResultFromParser(html, "//input[@name='encrypmethod']", "value")[0];
                string encrypmessage = HtmlParser.GetResultFromParser(html, "//input[@name='encrypmessage']", "value")[0];
                string signmethod = HtmlParser.GetResultFromParser(html, "//input[@name='signmethod']", "value")[0];
                string signmessage = HtmlParser.GetResultFromParser(html, "//input[@name='signmessage']", "value")[0];
                string ReferUrl = location;
                Url = location;
                Postdata = string.Format("version={0}&charset={1}&certorgcode={2}&receiveorgcode={3}&encrypmethod={4}&encrypmessage={5}&signmethod={6}&signmessage={7}",
                    version, charset, certorgcode, receiveorgcode, encrypmethod, encrypmessage, signmethod, signmessage);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Postdata = Postdata,
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                location = HtmlParser.GetResultFromParser(httpResult.Html, "//form", "action")[0];
                version = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='version']", "value")[0];
                string accessType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='accessType']", "value")[0];
                string backUrl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='backUrl']", "value")[0];
                string bizType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='bizType']", "value")[0];
                string certId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='certId']", "value")[0];
                string channelType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='channelType']", "value")[0];
                string customerIp = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='customerIp']", "value")[0];
                string encoding = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='encoding']", "value")[0];
                string frontUrl = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='frontUrl']", "value")[0];
                string logId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='internal.logId']", "value")[0];
                string merReferer = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='internal.merReferer']", "value")[0];
                string origReqInfo = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='internal.origReqInfo']", "value")[0];
                string merId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='merId']", "value")[0];
                string orderId = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='orderId']", "value")[0];
                string payTimeout = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='payTimeout']", "value")[0];
                string reserved = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='reserved']", "value")[0];
                string signMethod = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='signMethod']", "value")[0];
                string signature = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='signature']", "value")[0];
                string txnSubType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='txnSubType']", "value")[0];
                string txnTime = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='txnTime']", "value")[0];
                string txnType = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='txnType']", "value")[0];

                Url = location;
                Postdata = @"accessType={19}&backUrl={1}&bizType={2}&certId={3}&channelType={4}&customerIp={5}&encoding={6}&frontUrl={7}&internal.logId={8}&internal.merReferer={9}&internal.origReqInfo={10}&merId={11}&orderId={20}&payTimeout={12}&reserved={13}&signMethod={14}&signature={15}&txnSubType={16}&txnTime={17}&txnType={18}&version={0}";
                Postdata = string.Format(Postdata, version, backUrl.ToUrlEncode(), bizType, certId, channelType, customerIp, encoding, frontUrl.ToUrlEncode(), logId, merReferer, origReqInfo.ToUrlEncode(),
                    merId, payTimeout, reserved.ToUrlEncode(), signMethod, signature.ToUrlEncode(), txnSubType, txnTime, txnType, accessType, orderId);

                //DateTime dt = DateTime.Now.ToUniversalTime();
                //string t = dt.ToString("r");
                //string temp = "{\"p_lst_vt\":\"" + t + "\"}";
                //string cookiestr = "ub_smpl_ln=" + temp.ToUrlEncode();

                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "cashier.95516.com",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                location = httpResult.ResponseUri;

                string post_sign = CommonFun.GetMidStr(location, "sign=", "&");
                string UnionToken = CommonFun.GetMidStr(location, "transNumber=", "&");
                Url = "https://mcashier.95516.com/mobile/verify/init.action";
                Postdata = "{\"p\":{\"sign\":\"" + post_sign + "\"},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "mcashier.95516.com",
                    ContentType = "application/json; charset=UTF-8",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Referer = location
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                string sk = jsonParser.GetResultFromMultiNode(httpResult.Html, "p:sk");
                string modulus = jsonParser.GetResultFromMultiNode(httpResult.Html, "p:modulus");

                Res.StatusDescription = "获取银联卡认证码初始化成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                dics.Add("UnionToken", UnionToken);
                dics.Add("sk", sk);
                dics.Add("modulus", modulus);
                CacheHelper.SetCache(token, dics);

                Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码初始化,结束,token:" + token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "获取银联卡认证码初始化请求异常";
                Log4netAdapter.WriteError("获取银联卡认证码初始化请求异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 银联认证，第一步，获取银行卡信息
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="creditcardNo">银联卡卡号</param>
        /// <returns></returns>
        public BaseRes QueryApplication_GetUnionPayCode_Step1(string token, string creditcardNo)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string html = string.Empty;
            string UnionToken = string.Empty;
            UnionPayResult unionResult = null;
            Dictionary<string, object> dics = new Dictionary<string, object>();
            Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码第一步,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    UnionToken = dics["UnionToken"].ToString();
                }

                Url = "https://mcashier.95516.com/mobile/verify/cardValidate.action";
                Postdata = "{\"p\":{\"cardNumber\":\"" + creditcardNo.ToTrim() + "\"},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "mcashier.95516.com",
                    ContentType = "application/json; charset=UTF-8",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection,
                };
                httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                unionResult = jsonParser.DeserializeObject<UnionPayResult>(httpResult.Html);
                if (unionResult.r != "00")
                {
                    Res.StatusDescription = unionResult.m;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "https://mcashier.95516.com/mobile/verify/getCardInfo.action";
                Postdata = "{\"p\":{},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "mcashier.95516.com",
                    ContentType = "application/json; charset=UTF-8",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                Res.StatusDescription = "获取银联卡认证码校验卡号成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = httpResult.Html;

                Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码第一步,结束,token:" + token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "获取银联卡认证码校验卡号请求异常";
                Log4netAdapter.WriteError("获取银联卡认证码校验卡号请求异常", e);
            }
            return Res;

        }

        /// <summary>
        /// 银联认证，第二步，获取短信验证码
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        public BaseRes QueryApplication_GetUnionPayCode_Step2(string token, string mobile)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string UnionToken = string.Empty;

            Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码第二步,开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    UnionToken = dics["UnionToken"].ToString();
                }

                Url = "https://mcashier.95516.com/mobile/verify/sendSMS.action";
                Postdata = "{\"p\":{\"smsType\":\"UnionSMS\",\"mobile\":\"" + mobile + "\"},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "mcashier.95516.com",
                    ContentType = "application/json; charset=UTF-8",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }

                Res.StatusDescription = "获取银联卡认证码获取短信验证码成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码第二步,结束,token:" + token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "获取银联卡认证码获取短信验证码请求异常";
                Log4netAdapter.WriteError("获取银联卡认证码获取短信验证码请求异常", e);
            }
            return Res;

        }

        /// <summary>
        /// 银联认证，第三步，提交信息
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="unionpay">银联卡详细信息</param>
        /// <returns></returns>
        public BaseRes QueryApplication_GetUnionPayCode_Step3(UnionPayReq unionpay)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string UnionToken = string.Empty;
            string sk = string.Empty;
            string modulus = string.Empty;
            string result = string.Empty;

            Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码第三步,开始,token:" + unionpay.Token + ",参数：" + jsonParser.SerializeObject(unionpay));

            try
            {
                if (CacheHelper.GetCache(unionpay.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(unionpay.Token);
                    UnionToken = dics["UnionToken"].ToString();
                    sk = dics["sk"].ToString();
                    modulus = dics["modulus"].ToString();
                }

                Url = "https://mcashier.95516.com/mobile/verify/cardPay.action";
                if (unionpay.password.IsEmpty())
                {
                    Postdata = "{\"p\":{\"cvn2\":\"" + GetCVN2(unionpay.cvn2, sk).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\",\"expire\":\"" + unionpay.expire + "\",\"mobile\":\"" + unionpay.mobile + "\",\"smsCode\":\"" + unionpay.smsCode + "\",\"credentialType\":\"" + unionpay.credentialType + "\",\"credential\":\"" + unionpay.credential + "\",\"name\":\"" + unionpay.name.ToUrlEncode() + "\",\"discountActivity\":{}},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                }
                else
                {
                    //modulus = "ddc1b17fe4c89d81461d885b81b261f16ce20e5170810f87319fa34233b437aa33c71fc111aa9256af607b997c51ba5cf6f537fa75ad7425c32049e9443082756e002c966bdf82a9febae17369faf215c7d82baa8afd973ac92ba8d33eb779ec024dba1a451805b47510237c5e5901da59e7a818896160a76cd32171a35e8034307a9828118c318745499dc491186c2748f225e6817a9d959ac143b4e0e5896d17e53f9c4a03e7d7ecf3947c2ed6cbe6058c61dd9a44637844c11f0a4308dae5de5bd24519e5e09ea60f4ec81f32f8ae8fe55c4237c607c15b17158cf5ae91268c6a76a8e6ced80fafc8969a09db41dc07a9a6c18bc060885d0fede70ca33aa1";
                    string enpassword = UnionEncypt(unionpay.creditcardNo.ToTrim(), unionpay.password, modulus);
                    Postdata = "{\"p\":{\"password\":\"" + enpassword + "\",\"expire\":\"" + unionpay.expire + "\",\"mobile\":\"" + unionpay.mobile + "\",\"smsCode\":\"" + unionpay.smsCode + "\",\"credentialType\":\"" + unionpay.credentialType + "\",\"credential\":\"" + unionpay.credential + "\",\"name\":\"" + unionpay.name.ToUrlEncode() + "\",\"discountActivity\":{}},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                }
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "mcashier.95516.com",
                    ContentType = "application/json; charset=UTF-8",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.Html.Contains("Error report"))
                {
                    Res.StatusDescription = CommonFun.GetMidStr(httpResult.Html, "description</b> <u>", "</u>");
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                UnionPayResult unionResult = jsonParser.DeserializeObject<UnionPayResult>(httpResult.Html);
                if (unionResult.r != "00")
                {
                    Res.StatusDescription = unionResult.m;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                int Count = 0;
                do
                {
                    Count++;
                    if (!result.IsEmpty())
                    {
                        System.Threading.Thread.SpinWait(1000);
                    }
                    Url = "https://mcashier.95516.com/mobile/verify/cardPayProcessing.action";
                    Postdata = "{\"p\":{},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Host = "mcashier.95516.com",
                        ContentType = "application/json; charset=UTF-8",
                        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                        UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                        Postdata = Postdata,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                        Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                        return Res;
                    }
                    //Log4netAdapter.WriteInfo("(Kerry)Time="+ Count + ",Html:" + httpResult.Html);
                    unionResult = jsonParser.DeserializeObject<UnionPayResult>(httpResult.Html);
                    if (unionResult.r != "00")
                    {
                        Res.StatusDescription = unionResult.m;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }
                    result = jsonParser.GetResultFromMultiNode(httpResult.Html, "p:status");
                    if (result.IsEmpty())
                        result = "Fail";
                }
                while (!(result == "Succeed" || Count == 10));

                if (result != "Succeed")
                {
                    Res.StatusDescription = "获取银联卡认证码超时";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Url = "https://mcashier.95516.com/mobile/verify/cardPayResult.action";
                Postdata = "{\"p\":{},\"t\":\"" + UnionToken + "\",\"s\":\"2\"}";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "POST",
                    Host = "mcashier.95516.com",
                    ContentType = "application/json; charset=UTF-8",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Android; Mobile; rv:29.0) Gecko/29.0 Firefox/29.0",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8");
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                result = jsonParser.GetResultFromMultiNode(httpResult.Html, "p:payResult:authCode");
                if (result.IsEmpty())
                {
                    Res.StatusDescription = "获取银联卡认证码失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusDescription = "获取银联卡认证码提交成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.Result = result;

                Log4netAdapter.WriteInfo("央行互联网征信银行卡查询码申请获取银联卡认证码第三步,结束,token:" + unionpay.Token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "获取银联卡认证码提交请求异常";
                Log4netAdapter.WriteError("获取银联卡认证码提交请求异常", e);
            }
            return Res;

        }
        /// <summary>
        /// 重新发送查询码
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public BaseRes QueryReport_SendQueryCode(string token)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("重新发送查询码，开始,token:" + token);

            try
            {
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    loginname = dics["loginname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //第一步，检查是否已经提交申请
                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=applicationReport";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/menu.do",
                    Encoding = Encoding.GetEncoding("GB2312"),
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//font[@class='span-red span-12']", "text");

                results = results.Where(o => !o.IsEmpty()).ToList();
                if (results.Count == 0)
                {
                    Res.StatusDescription = "无查询码申请信息";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (results[0] != "(已生成)")
                {
                    Res.StatusDescription = "查询码还未生成成功";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (results[0] != "(加工成功)")
                {
                    Res.StatusDescription = "查询码还未生成成功";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                //第一步，检查是否已经提交申请
                Url = "https://ipcrs.pbccrc.org.cn/reportAction.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    Method="post",
                    Referer = "https://ipcrs.pbccrc.org.cn/reportAction.do?method=queryReport",
                    Postdata = "method=sendAgain&reportformat=21",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return Res;
                }
                if (httpResult.Html == "success")
                {
                    Res.StatusDescription = "发送成功";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Res.StatusDescription = "发送失败";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                }

                Log4netAdapter.WriteInfo("重新发送查询码,结束,token:" + token);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "重新发送查询码失败";
                Log4netAdapter.WriteError("重新发送查询码异常", e);
            }
            return Res;
        }

        /// <summary>
        /// 查询央行征信报告
        /// </summary>
        /// <param name="token">会话token</param>
        /// <param name="querycode">查询码</param>
        /// <param name="bid"></param>
        /// <returns></returns>
        public CRD_HD_REPORTRes GetReport(PbccrcReportQueryReq queryReq)
        {
            CRD_HD_REPORTRes Res = new CRD_HD_REPORTRes();
            CRD_HD_REPORTEntity reportEntity = new CRD_HD_REPORTEntity();
            string Url = string.Empty;
            string year = string.Empty;
            string loginname = string.Empty;
            List<string> results = new List<string>();

            Log4netAdapter.WriteInfo("央行互联网征信报告查询,开始,token:" + queryReq.Token);

            try
            {
                if (CacheHelper.GetCache(queryReq.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(queryReq.Token);
                    loginname = dics["loginname"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    //CacheHelper.RemoveCache(token);
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                Url = "https://ipcrs.pbccrc.org.cn/simpleReport.do?method=viewReport";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = "reportformat=21&tradeCode=" + queryReq.querycode,
                    Encoding = Encoding.GetEncoding("gb2312"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@class='erro_div1']", "");
                if (results.Count > 0)
                { 
                    Res.StatusDescription = CommonFun.ClearFlag(results[0]);
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    Log4netAdapter.WriteInfo(string.Format("央行互联网征信报告查询失败，失败原因{0},token:{1}", Res.StatusDescription, queryReq.Token));
                    return Res;
                }

                //string readStr = FileOperateHelper.ReadFile(@"E:\work\卡卡贷\1.txt", Encoding.GetEncoding("gb2312"));
                //httpResult = new HttpResult();
                //httpResult.Html = readStr;

                Res = ParserCreditReport(httpResult.Html, loginname, queryReq);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "央行征信报告查询异常";
                Log4netAdapter.WriteError("央行征信报告查询异常,token:" + queryReq.Token, e);
            }
            return Res;
        }
        /// <summary>
        /// 央行互联网认证注册,第一步，输入基本信息
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="name">姓名</param>
        /// <param name="certNo">证件号</param>
        /// <param name="certType">证件类别</param>
        /// <param name="vercode">验证码</param>
        /// <returns></returns>
        public BaseRes Register_Step1(string token, string name, string certNo, string certType, string verCode)
        {
            BaseRes baseRes = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            List<string> results = new List<string>();
            baseRes.Token = token;

            Log4netAdapter.WriteInfo("央行互联网征信注册第一步,开始,token:" + token);
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(token);
                    //CacheHelper.RemoveCache(token);
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //参数校验
                if (name.IsEmpty() || certNo.IsEmpty())
                {
                    baseRes.StatusDescription = "姓名或身份证不能为空";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                if (certType == "0")
                {
                    certNo = certNo.ToUpper();
                }

                #region 第一步，注册页面初始化，获取页面TOKEN
                httpItem = new HttpItem()
                {
                    URL = "https://ipcrs.pbccrc.org.cn/userReg.do?method=initReg",
                    Method = "get",
                    Referer = "https://ipcrs.pbccrc.org.cn/top1.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");

                if (results.Count == 0)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "央行征信注册初始化失败";
                    return baseRes;
                }
                #endregion


                #region 第二步，输入注册基本信息，并提交
                Url = "https://ipcrs.pbccrc.org.cn/userReg.do";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/userReg.do?method=initReg",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Postdata = String.Format("org.apache.struts.taglib.html.TOKEN={0}&method=checkIdentity&userInfoVO.name={1}&userInfoVO.certType={2}&userInfoVO.certNo={3}&_@IMGRC@_={4}&1=on", results[0], HttpUtility.UrlDecode(name), certType, certNo, verCode)
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                if (results.Count > 0)
                {
                    baseRes.Result = results[0];
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_error_field_']", "");
                #endregion

                if (results.Count == 0)
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "央行征信注册第一步完成";

                    //添加缓存
                    Dictionary<string, object> dics = new Dictionary<string, object>();
                    dics.Add("cookie", cookies);
                    dics.Add("token", baseRes.Result);
                    dics.Add("name", name);
                    dics.Add("certNo", certNo);
                    dics.Add("certType", certType);
                    CacheHelper.SetCache(token, dics);
                }
                else
                {
                    if (results[0].Contains("目前系统尚未收录您的个人信息，无法进行注册"))
                        baseRes.StatusCode = ServiceConsts.StatusCode_NetCredit_NoQuestion;
                    else
                        baseRes.StatusCode = ServiceConsts.StatusCode_NetCredit_ExistUser;
                    baseRes.StatusDescription = results[0];

                    //if (results[0].Contains("目前系统尚未收录您的个人信息，无法进行注册"))
                    //{
                    //    //添加征信空白记录
                    //    Task.Factory.StartNew(() =>
                    //    {
                    //        try
                    //        {
                    //            ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                    //            CRD_HD_REPORTEntity reportEntity = reportService.GetByIdentityCard(certNo);
                    //            if (reportEntity == null)
                    //            {
                    //                reportEntity = new CRD_HD_REPORTEntity();
                    //                reportEntity.BusType = "CREDITBLANK";
                    //                reportEntity.BusIdentityCard = certNo;
                    //                reportEntity.CertNo = certNo;
                    //                reportEntity.Name = name;
                    //                reportService.Save(reportEntity);
                    //            }
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            Log4netAdapter.WriteError("征信空白保存出错", e);
                    //        }
                    //    });
                    //}
                }
                Log4netAdapter.WriteInfo("央行互联网征信注册第一步,结束,token:" + token);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "央行征信注册异常";
                Log4netAdapter.WriteError("央行征信注册异常", e);
            }
            return baseRes;
        }
        /// <summary>
        /// 央行互联网认证注册,第二步，发送手机验证码
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="mobileTel">手机号</param>
        /// <returns></returns>
        public BaseRes Register_Step2(string token, string mobileTel)
        {
            BaseRes baseRes = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string name = string.Empty;
            string certNo = string.Empty;
            string certType = string.Empty;
            List<string> results = new List<string>();
            baseRes.Token = token;

            Log4netAdapter.WriteInfo("央行互联网征信注册第二步,开始,token:" + token);

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    TOKEN = dics["token"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    name = dics["name"].ToString();
                    certNo = dics["certNo"].ToString();
                    certType = dics["certType"].ToString();
                    //CacheHelper.RemoveCache(token);
                }
                else
                {
                    baseRes.StatusDescription = "未获取Token对应信息！";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                //校验参数
                if (mobileTel.IsEmpty())
                {
                    baseRes.StatusDescription = "手机号不能为空";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                //提交手机号，发送手机验证码
                Postdata = string.Format("method=getAcvitaveCode&mobileTel={0}", mobileTel);
                httpItem = new HttpItem()
                {
                    URL = "https://ipcrs.pbccrc.org.cn/userReg.do",
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/userReg.do",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                if (httpResult.Html.IsEmpty())
                {
                    baseRes.StatusDescription = "发送手机验证码失败，手机号：" + mobileTel;
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "央行征信注册，发送手机验证码完毕，手机号：" + mobileTel;
                baseRes.Result = httpResult.Html;

                //添加缓存
                Dictionary<string, object> dics1 = new Dictionary<string, object>();
                dics1.Add("cookie", cookies);
                dics1.Add("token", TOKEN);
                dics1.Add("tcId", baseRes.Result);
                dics1.Add("name", name);
                dics1.Add("certNo", certNo);
                dics1.Add("certType", certType);
                CacheHelper.SetCache(token, dics1);
                Log4netAdapter.WriteInfo("央行互联网征信注册第二步,结束,token:" + token);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "央行征信注册异常";
                Log4netAdapter.WriteError("央行征信注册异常", e);
            }
            return baseRes;
        }
        /// <summary>
        /// 央行互联网认证注册,第三步，补充用户信息
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="loginname">登录账号</param>
        /// <param name="password">密码</param>
        /// <param name="confirmpassword">确认密码</param>
        /// <param name="email">电子邮件</param>
        /// <param name="mobileTel">手机号</param>
        /// <param name="verifyCode">手机验证码</param>
        /// <returns></returns>
        public BaseRes Register_Step3(string token, string loginname, string password, string confirmpassword, string email, string mobileTel, string verifyCode)
        {
            BaseRes baseRes = new BaseRes();
            string Url = string.Empty;
            string Postdata = string.Empty;
            string TOKEN = string.Empty;
            string tcId = string.Empty;
            string name = string.Empty;
            string certNo = string.Empty;
            string certType = string.Empty;
            List<string> results = new List<string>();
            baseRes.Token = token;
            Log4netAdapter.WriteInfo("央行互联网征信注册第三步,开始,token:" + token);

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(token);
                    TOKEN = dics["token"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    tcId = dics["tcId"].ToString();
                    name = dics["name"].ToString();
                    certNo = dics["certNo"].ToString();
                    certType = dics["certType"].ToString();
                }
                //校验参数
                if (loginname.IsEmpty() || password.IsEmpty() || confirmpassword.IsEmpty() || mobileTel.IsEmpty() || verifyCode.IsEmpty())
                {
                    //添加缓存
                    Dictionary<string, object> dics1 = new Dictionary<string, object>();
                    dics1.Add("cookie", cookies);
                    dics1.Add("token", TOKEN);
                    dics1.Add("tcId", baseRes.Result);
                    dics1.Add("name", name);
                    dics1.Add("certNo", certNo);
                    dics1.Add("certType", certType);
                    CacheHelper.SetCache(token, dics1);
                    baseRes.StatusDescription = "有必填项为空";
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return baseRes;
                }
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);

                #region 第一步，校验登录名是否已存在
                Postdata = string.Format("method=checkRegLoginnameHasUsed&loginname={0}", loginname);
                httpItem = new HttpItem()
                {
                    URL = "https://ipcrs.pbccrc.org.cn/userReg.do",
                    Method = "post",
                    CookieCollection = cookies,
                    Referer = "https://ipcrs.pbccrc.org.cn/userReg.do",
                    Postdata = Postdata,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                if (httpResult.Html == "1")
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = "登录名已经存在";
                    return baseRes;
                }
                #endregion

                #region 第二步，提交注册信息
                Postdata = string.Format("org.apache.struts.taglib.html.TOKEN={0}&method=saveUser&counttime=40&tcId={1}&userInfoVO.loginName={2}&userInfoVO.password={3}&userInfoVO.confirmpassword={4}&userInfoVO.email={5}&userInfoVO.mobileTel={6}&userInfoVO.verifyCode={7}&userInfoVO.smsrcvtimeflag=2"
                    , TOKEN, tcId, loginname, password, confirmpassword, email, mobileTel, verifyCode);
                httpItem = new HttpItem()
                {
                    URL = "https://ipcrs.pbccrc.org.cn/userReg.do",
                    Method = "post",
                    Postdata = Postdata,
                    Referer = "https://ipcrs.pbccrc.org.cn/userReg.do",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                if (!forwardedIp.IsEmpty())
                {
                    httpItem.Header.Add("x-forwarded-for", forwardedIp);
                }
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    baseRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    baseRes.StatusCode = ServiceConsts.StatusCode_httpfail;
                    return baseRes;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//span[@id='_error_field_']", "");
                #endregion

                if (results.Count == 0 && httpResult.Html.Contains("您在个人信用信息平台已注册成功"))
                {
                    baseRes.StatusCode = ServiceConsts.StatusCode_success;
                    baseRes.StatusDescription = "央行征信注册成功";

                    //注册信息保存
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (!name.IsEmpty() && !certNo.IsEmpty() && !loginname.IsEmpty() && !password.IsEmpty())
                            {
                                ICRD_ACCOUNT service = NetSpiderFactoryManager.GetCRDACCOUNTService();
                                int count = service.CountHql("from CRD_ACCOUNTEntity where UserName=?", new object[] { loginname });
                                if (count == 0)
                                {
                                    CRD_ACCOUNTEntity accountEntity = new CRD_ACCOUNTEntity()
                                    {
                                        UserName = loginname,
                                        Password = password,
                                        Name = name,
                                        CertType = certType,
                                        CertNo = certNo,
                                        Mobile = mobileTel,
                                        Email = email
                                    };
                                    service.Save(accountEntity);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log4netAdapter.WriteError("注册信息保存失败", e);
                        }

                    });
                }
                else
                {
                    var tokens = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='org.apache.struts.taglib.html.TOKEN']", "value");
                    if (tokens.Count > 0)
                    {
                        baseRes.Result = tokens[0];
                        //添加缓存
                        Dictionary<string, object> dics1 = new Dictionary<string, object>();
                        dics1.Add("cookie", cookies);
                        dics1.Add("token", tokens[0]);
                        dics1.Add("name", name);
                        dics1.Add("certNo", certNo);
                        dics1.Add("certType", certType);
                        CacheHelper.SetCache(token, dics1);
                    }
                    baseRes.StatusCode = ServiceConsts.StatusCode_fail;
                    baseRes.StatusDescription = results.Count > 0 ? results[0] : "无信息";
                }
                Log4netAdapter.WriteInfo("央行互联网征信注册第三步,结束,token:" + token);
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "央行征信注册异常";
                Log4netAdapter.WriteError("央行征信注册异常", e);
            }
            return baseRes;
        }

        #region 私有方法
        private CRD_HD_REPORTRes ParserCreditReport(string htmlString, string loginname, PbccrcReportQueryReq queryReq)
        {
            CRD_HD_REPORTRes Res = new CRD_HD_REPORTRes();
            CRD_HD_REPORTEntity reportEntity = new CRD_HD_REPORTEntity();
            string Url = string.Empty;
            string year = string.Empty;
            List<string> results = new List<string>();

            try
            {
                #region 数据库服务初始化
                ICRD_HD_REPORT reportService = NetSpiderFactoryManager.GetCRDHDREPORTService();
                ICRD_ACCOUNT accountService = NetSpiderFactoryManager.GetCRDACCOUNTService();
                ICRD_HD_REPORT_HTML htmlService = NetSpiderFactoryManager.GetCRDHDREPORTHTMLService();
                #endregion

                if (!htmlString.Contains("报告编号"))
                {
                    Res.StatusDescription = "报告查询失败";
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(htmlString, "//table/tr/td/table", "");

                //以下是对数据进行分析、统计、整理
                CRD_CD_LNDEntity crdlnd = null;//贷记卡
                CRD_CD_LNEntity crdln = null;//贷款
                CRD_CD_STNCARDEntity crdstncard = null;//准贷记卡
                CRD_CD_GUARANTEEEntity crdguarantee = null;//担保
                CRD_IS_CREDITCUEEntity creditcue = new CRD_IS_CREDITCUEEntity();//信息概要
                CRD_STAT_LNEntity statln = new CRD_STAT_LNEntity();//贷款统计表
                CRD_STAT_LNDEntity statlnd = new CRD_STAT_LNDEntity();//贷记卡统计表
                CRD_STAT_QREntity statqr = new CRD_STAT_QREntity();//查询统计表
                CRD_HD_REPORT_HTMLEntity htmlEntity = new CRD_HD_REPORT_HTMLEntity();
                List<string> tRows = new List<string>();
                List<string> tRows1 = new List<string>();
                DateTime QueryTime = new DateTime();
                int count = 0;

                #region 基本信息、信息概要、报告查询统计
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table/tr[2]/td/table", "");
                if (results.Count > 0)
                {
                    //返回值
                    Res.ReportSn = CommonFun.GetMidStrByRegex(results[0], "报告编号：", "</strong>").Trim();
                    Res.QueryTime = CommonFun.GetMidStrByRegex(results[0], "查询时间：", "</strong>").Trim();
                    Res.ReportCreateTime = CommonFun.GetMidStrByRegex(results[0], "报告时间：", "</strong>");
                    Res.Name = CommonFun.GetMidStrByRegex(results[1], "姓名：", "</strong>").Trim();
                    Res.CertType = CommonFun.GetMidStrByRegex(results[1], "证件类型：", "</strong>").Trim();
                    Res.CertNo = CommonFun.GetMidStrByRegex(results[1], "证件号码：", "</strong>").Trim();
                    Res.MaritalState = CommonFun.GetMidStrByRegex(results[1], "<td width=\"133\" align=\"right\"><strong class=\"p\">", "</strong>").Trim();

                    count = reportService.CountHql("from CRD_HD_REPORTEntity where ReportSn=?", new object[] { Res.ReportSn });

                    //保存html
                    if (count == 0)
                    {
                        htmlEntity.ReportSn = Res.ReportSn;
                        htmlEntity.Html = httpResult.Html;
                        htmlEntity.BusId = queryReq.BusId;
                        htmlEntity.BusType = queryReq.BusType;
                        htmlEntity.Loginname = loginname;
                        htmlService.Save(htmlEntity);
                    }

                    //数据库入库
                    reportEntity.ReportSn = CommonFun.GetMidStrByRegex(results[0], "报告编号：", "</strong>").Trim();
                    reportEntity.QueryTime = CommonFun.GetMidStrByRegex(results[0], "查询时间：", "</strong>").ToDateTime();
                    reportEntity.ReportCreateTime = CommonFun.GetMidStrByRegex(results[0], "报告时间：", "</strong>").ToDateTime();
                    reportEntity.Name = CommonFun.GetMidStrByRegex(results[1], "姓名：", "</strong>").Trim();
                    reportEntity.CertType = CommonFun.GetMidStrByRegex(results[1], "证件类型：", "</strong>").Trim();
                    reportEntity.CertNo = CommonFun.GetMidStrByRegex(results[1], "证件号码：", "</strong>").Trim();
                    reportEntity.MaritalState = CommonFun.GetMidStrByRegex(results[1], "<td width=\"133\" align=\"right\"><strong class=\"p\">", "</strong>").Trim();
                    QueryTime = (DateTime)reportEntity.QueryTime;

                    tRows = HtmlParser.GetResultFromParser(results[3], "/tr[3]/td[1]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[position()>1]", "", true);
                    foreach (string strItem in tRows)
                    {
                        tRows1 = HtmlParser.GetResultFromParser(strItem, "//td", "");
                        if (tRows1.Count < 4)
                        {
                            continue;
                        }
                        if (tRows1[0] == "&nbsp;账户数")
                        {
                            creditcue.HouseLoanCount = tRows1[2].ToInt();
                            creditcue.LoancardCount = tRows1[1].ToInt();
                            creditcue.OtherLoanCount = tRows1[3].ToInt();

                            statln.ALL_LOAN_HOUSE_CNT = tRows1[2].ToInt();//住房贷款账户数
                            statln.ALLLOANOTHERCNT = tRows1[3].ToInt();//其他贷款账户数
                            statlnd.ALL_CREDIT_CNT = tRows1[1].ToInt();//信用卡账户数
                        }
                        if (tRows1[0].Contains("未结清/未销户账户数"))
                        {
                            statln.ALLLOANHOUSEUNCLOSEDCNT = tRows1[2].ToInt();//住房贷款未销户账户数
                            statln.ALLLOANOTHERUNCLOSEDCNT = tRows1[3].ToInt();//其他贷款未销户账户数
                            statlnd.ALLCREDITUNCLOSEDCNT = tRows1[1].ToInt();//信用卡未销户账户数
                        }
                        else if (tRows1[0].Contains("发生过逾期的账户数"))
                        {
                            statln.ALLLOANHOUSEDELAYCNT = tRows1[2].ToInt();//住房贷款发生过逾期的账户数
                            statln.ALLLOANOTHERDELAYCNT = tRows1[3].ToInt();//其他贷款发生过逾期的账户数
                            statlnd.ALLCREDITDELAYCNT = tRows1[1].ToInt();//信用卡发生过逾期的账户数
                        }
                        else if (tRows1[0].Contains("发生过90天以上逾期的账户数"))
                        {
                            statln.ALLLOANHOUSEDELAY90CNT = tRows1[2].ToInt();//住房贷款发生过90天以上逾期的账户数
                            statln.ALLLOANOTHERDELAY90CNT = tRows1[3].ToInt();//其他贷款发生过90天以上逾期的账户数
                            statln.LOANDELAY90CNT = statln.ALLLOANHOUSEDELAY90CNT + statln.ALLLOANOTHERDELAY90CNT;//贷款逾期90天以上账户数
                            statlnd.ALLCREDITDELAY90CNT = tRows1[1].ToInt();//信用卡发生过90天以上逾期的账户数
                        }
                        else if (tRows1[0].Contains("为他人担保笔数"))
                        {
                            statln.ALL_LOAN_HOUSE_FOROTHERS_CNT = tRows1[2].ToInt();//为他人担保笔数住房贷款发生过逾期的账户数
                            statln.ALL_LOAN_OTHER_FOROTHERS_CNT = tRows1[3].ToInt();//为他人担保笔数其他贷款发生过逾期的账户数
                            statlnd.ALL_CREDIT_FOROTHERS_CNT = tRows1[1].ToInt();//为他人担保笔数信用卡发生过逾期的账户数
                        }
                    }
                    reportEntity.CRD_IS_CREDITCUE = creditcue;
                    CRD_QR_RECORDDTLEntity recorddtl = null;

                    //查询记录
                    foreach (string tbItem in results)
                    {
                        tRows.Clear();
                        //tRows.Clear();
                        //if (tbItem.Contains("查询操作员"))
                        //{
                        //    tRows.AddRange(HtmlParser.GetResultFromParser(tbItem, "//tr[position()>3]", ""));
                        //}
                        if (tbItem.Contains("机构查询记录明细") || tbItem.Contains("个人查询记录明细"))
                        {
                            tRows.AddRange(HtmlParser.GetResultFromParser(tbItem, "//tr[position()>3]", ""));
                            foreach (string strItem in tRows)
                            {
                                recorddtl = new CRD_QR_RECORDDTLEntity();
                                tRows1 = HtmlParser.GetResultFromParser(strItem, "//td", "", true);
                                if (tRows1.Count < 4)
                                {
                                    continue;
                                }
                                recorddtl.QueryDate = tRows1[1].ToDateTime(Consts.DateFormatString6);
                                recorddtl.Querier = tRows1[2];
                                recorddtl.QueryReason = tRows1[3];
                                reportEntity.CRD_QR_RECORDDTLList.Add(recorddtl);
                            }

                        }
                        #region 公共记录-强制执行

                        //公共记录-强制执行
                        else if (tbItem.Contains("执行法院") && tbItem.Contains("案号"))
                        {
                            var porc = GetForceexctnFromReport(HtmlParser.GetResultFromParser(tbItem, "//tr", ""));
                            if (porc != null)
                                reportEntity.CRD_PI_FORCEEXCTNEList.Add(porc);
                        }
                        #endregion
                        #region 公共记录-欠税记录段
                        else if (tbItem.Contains("主管税务机关") && tbItem.Contains("欠税统计时间"))
                        {
                            var tax = GetTaxarrearFromReport(HtmlParser.GetResultFromParser(tbItem, "//tr", ""));
                            if (tax != null)
                                reportEntity.CRD_PI_TAXARREARList.Add(tax);
                        }

                        #endregion

                        #region 公共记录-电信欠费信息
                        else if (tbItem.Contains("电信运营商") && tbItem.Contains("业务类型"))
                        {
                            var tax = GetTelpntListFromReport(HtmlParser.GetResultFromParser(tbItem, "//tr", ""));
                            if (tax != null)
                                reportEntity.CRD_PI_TELPNTEntityList.Add(tax);
                        }
                        #endregion

                        #region 公共记录-民事判决记录
                        else if (tbItem.Contains("立案法院") && tbItem.Contains("案号"))
                        {
                            var tax = GetCiviljdgmListFromReport(HtmlParser.GetResultFromParser(tbItem, "//tr", ""));
                            if (tax != null)
                                reportEntity.CRD_PI_CIVILJDGMEntityList.Add(tax);
                        }
                        #endregion
                        #region 公共记录-行政处罚记录段
                        else if (tbItem.Contains("处罚机构") && tbItem.Contains("文书编号"))
                        {
                            var tax = GetAdminpnshmListFromReport(HtmlParser.GetResultFromParser(tbItem, "//tr", ""));
                            if (tax != null)
                                reportEntity.CRD_PI_ADMINPNSHMEntityList.Add(tax);
                        }
                        #endregion

                    }




                    //查询记录统计
                    statqr.M3ALLCNTTOTAL = reportEntity.CRD_QR_RECORDDTLList.Where(o => o.QueryDate >= QueryTime.AddMonths(-3) && (o.QueryReason == "信用卡审批" || o.QueryReason == "贷款审批")).Count();
                    //statqr.M3CREDITCNT = reportEntity.CRD_QR_RECORDDTLList.Where(o => o.QueryDate >= QueryTime.AddMonths(-3) && (o.QueryReason == "信用卡审批" || o.QueryReason == "贷后管理")).Count();
                    statqr.M3CREDITCNT = reportEntity.CRD_QR_RECORDDTLList.Where(o => o.QueryDate >= QueryTime.AddMonths(-3) && (o.QueryReason == "信用卡审批")).Count();
                    statqr.M3LOANCNT = reportEntity.CRD_QR_RECORDDTLList.Where(o => o.QueryDate >= QueryTime.AddMonths(-3) && o.QueryReason == "贷款审批").Count();
                    reportEntity.CRD_STAT_QR = statqr;
                }
                #endregion

                #region 信用卡、贷款、担保贷款明细

                decimal CREDIT_LIMIT_AMOUNT_NORM_MAX = 0;//正常信用卡最大额度
                //decimal NORMAL_CREDIT_BALANCE = 0;//正常信用卡未用额度
                decimal SUM_NORMAL_LIMIT_AMOUNT = 0;//正常信用卡总额度
                decimal SUM_NORMAL_USE_LIMIT_AMOUNT = 0;//正常信用卡使用额度
                decimal credit_dlq_amount = 0;//信用卡逾期金额
                decimal M9_LEND_AMOUNT = 0;//最近九个月发放贷记卡总金额
                int M9_DELIVER_CNT = 0;//最近九个月发放贷记卡数量
                int NORMAL_CARDNUM = 0;//正常卡数据量
                int CREDIT_DELAY_CNT = 0;
                decimal NORMAL_USED_MAX = 0;//正常信用卡最大已使用额度

                decimal loan_dlq_amount = 0;//贷款逾期金额
                decimal loan_pmt_monthly = 0;//贷款每月还款本金金额
                decimal loan_house_pmt_monthly = 0;//贷款每月还款本金金额
                decimal SUM_LOAN_BALANCE = 0;//贷款总余额
                decimal SUM_LOAN_LIMIT_AMOUNT = 0;//贷款本金总额
                decimal loan_Balance = 0;

                DateTime NowDate = QueryTime;//当前日期
                DateTime CreditOpenTime = QueryTime;//信用卡最早开卡时间
                DateTime LoanOpenTime = QueryTime;//最早贷款时间
                DateTime NormalCreditOpenTime = QueryTime;//正常信用卡最早开卡时间
                DateTime NormalLoanOpenTime = QueryTime;//未还完最早贷款时间
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ol[@class='p olstyle']/li", "");

                string TempStr = string.Empty;
                string Currency = string.Empty;
                foreach (string strItem in results)
                {
                    TempStr = CommonFun.ClearFlag(strItem);
                    TempStr = TempStr.Trim();
                    #region 准贷记卡
                    if (TempStr.IndexOf("准贷记卡") != -1)
                    {
                        crdstncard = new CRD_CD_STNCARDEntity();
                        crdstncard.Cue = TempStr;
                        crdstncard.OpenDate = (TempStr.Split('日')[0] + "日").ToDateTime(Consts.DateFormatString6);

                        Currency = CommonFun.GetMidStrByRegex(TempStr, "准贷记卡（", "账户）");
                        crdstncard.CreditLimitAmount = 0;
                        crdstncard.CurrOverdueAmount = 0;
                        if (!Currency.IsEmpty())
                        {
                            crdstncard.Currency = Currency;

                            if (Currency != "人民币")
                            {
                                crdstncard.CreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "信用额度折合人民币", "，透支").ToDecimal(0);
                            }
                            else
                            {
                                crdstncard.CreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "信用额度", "，透支").ToDecimal(0);
                            }
                        }
                        else
                        {
                            continue;
                        }
                        crdstncard.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "透支余额", "。").ToDecimal(0);
                        crdstncard.OverdraftOver60Cyc = CommonFun.GetMidStrByRegex(TempStr, "最近5年内有", "个").ToInt(0);
                        crdstncard.OverdraftOver90Cyc = CommonFun.GetMidStrByRegex(TempStr, "天，其中", "个").ToInt(0);
                        crdstncard.FinanceOrg = CommonFun.GetMidStrByRegex(TempStr, "日", "发放").Trim();

                        if (TempStr.Contains("销户"))
                        {
                            crdstncard.State = "销户";
                        }
                        else if (TempStr.Contains("未激活"))
                        {
                            crdstncard.State = "未激活";
                        }
                        else if (TempStr.Contains("已变成呆账"))
                        {
                            crdstncard.State = "呆账";

                            crdstncard.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdstncard.CurrOverdueAmount == 0)
                            {
                                crdstncard.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }
                        }
                        else if (TempStr.Contains("冻结"))
                        {
                            crdstncard.State = "冻结";

                            crdstncard.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdstncard.CurrOverdueAmount == 0)
                            {
                                crdstncard.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }
                        }
                        else if (TempStr.Contains("止付"))
                        {
                            crdstncard.State = "止付";

                            crdstncard.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdstncard.CurrOverdueAmount == 0)
                            {
                                crdstncard.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }
                        }
                        else
                        {
                            crdstncard.State = "正常";
                            NORMAL_CARDNUM++;

                            ////正常信用卡未用额度
                            //NORMAL_CREDIT_BALANCE += (decimal)crdstncard.CreditLimitAmount - (decimal)crdstncard.UsedCreditLimitAmount;

                            if (strItem.IndexOf("人民币账户") != -1)
                            {
                                //正常信用卡最大额度
                                if (crdstncard.CreditLimitAmount > CREDIT_LIMIT_AMOUNT_NORM_MAX)
                                {
                                    CREDIT_LIMIT_AMOUNT_NORM_MAX = (decimal)crdstncard.CreditLimitAmount;
                                }
                                SUM_NORMAL_LIMIT_AMOUNT += (decimal)crdstncard.CreditLimitAmount;//正常信用卡总额度,不包括美元账户
                            }
                            //正常信用卡使用额度
                            SUM_NORMAL_USE_LIMIT_AMOUNT += (decimal)crdstncard.UsedCreditLimitAmount;
                            //正常信用卡最大已使用额度
                            if (NORMAL_USED_MAX < (decimal)crdstncard.UsedCreditLimitAmount)
                            {
                                NORMAL_USED_MAX = (decimal)crdstncard.UsedCreditLimitAmount;
                            }
                            if (NormalCreditOpenTime > crdstncard.OpenDate)
                            {
                                NormalCreditOpenTime = (DateTime)crdstncard.OpenDate;
                            }
                        }
                        credit_dlq_amount += (decimal)crdstncard.CurrOverdueAmount;//逾期金额
                        if (CreditOpenTime > crdstncard.OpenDate)
                        {
                            CreditOpenTime = (DateTime)crdstncard.OpenDate;
                        }
                        //最近九个月发放贷记卡统计
                        if (crdstncard.OpenDate > NowDate.AddMonths(-9) && strItem.IndexOf("人民币账户") != -1)
                        {
                            M9_LEND_AMOUNT += (decimal)crdstncard.CreditLimitAmount;
                            M9_DELIVER_CNT++;
                        }
                        reportEntity.CRD_CD_STNCARDList.Add(crdstncard);
                    }
                    #endregion

                    #region 贷记卡
                    else if (TempStr.IndexOf("贷记卡") != -1)
                    {
                        crdlnd = new CRD_CD_LNDEntity();
                        crdlnd.Cue = TempStr;
                        crdlnd.OpenDate = (TempStr.Split('日')[0] + "日").ToDateTime(Consts.DateFormatString6);

                        Currency = CommonFun.GetMidStrByRegex(TempStr, "贷记卡（", "账户）");

                        if (!Currency.IsEmpty())
                        {
                            crdlnd.Currency = Currency;
                            if (TempStr.IndexOf("已使用额度") != -1)
                            {
                                crdlnd.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "已使用额度", "。").ToDecimal(0);
                                if (crdlnd.UsedCreditLimitAmount == 0)
                                {
                                    crdlnd.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "已使用额度", "，").ToDecimal(0);
                                }
                            }

                            if (Currency != "人民币")
                            {
                                crdlnd.CreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "信用额度折合人民币", "，已").ToDecimal(0);
                            }
                            else
                            {
                                crdlnd.CreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "信用额度", "，已").ToDecimal(0);
                            }
                        }
                        else
                        {
                            continue;
                        }
                        crdlnd.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "逾期金额", "。").ToDecimal(0);
                        crdlnd.OverdueCyc = CommonFun.GetMidStrByRegex(TempStr, "最近5年内有", "个").ToInt(0);
                        crdlnd.OverdueOver90Cyc = CommonFun.GetMidStrByRegex(TempStr, "逾期状态，其中", "个").ToInt(0);
                        crdlnd.FinanceOrg = CommonFun.GetMidStrByRegex(TempStr, "日", "发放").Trim();

                        //贷记卡状态判断
                        if (TempStr.Contains("销户"))
                        {
                            crdlnd.State = "销户";
                        }
                        else if (TempStr.Contains("未激活"))
                        {
                            crdlnd.State = "未激活";
                        }
                        else if (TempStr.Contains("已变成呆账"))
                        {
                            crdlnd.State = "呆账";

                            crdlnd.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdlnd.CurrOverdueAmount == 0)
                            {
                                crdlnd.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }

                            //crdlnd.CurrOverdueAmount = crdlnd.UsedCreditLimitAmount;
                        }
                        else if (TempStr.Contains("冻结"))
                        {
                            crdlnd.State = "冻结";

                            crdlnd.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdlnd.UsedCreditLimitAmount == 0)
                            {
                                crdlnd.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }

                            crdlnd.CurrOverdueAmount = crdlnd.UsedCreditLimitAmount;
                        }
                        else if (TempStr.Contains("止付"))
                        {
                            crdlnd.State = "止付";

                            crdlnd.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdlnd.UsedCreditLimitAmount == 0)
                            {
                                crdlnd.UsedCreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }

                            crdlnd.CurrOverdueAmount = crdlnd.UsedCreditLimitAmount;
                        }
                        else
                        {
                            crdlnd.State = "正常";
                            NORMAL_CARDNUM++;
                            ////正常信用卡未用额度
                            //NORMAL_CREDIT_BALANCE += (decimal)crdlnd.CreditLimitAmount - (decimal)crdlnd.UsedCreditLimitAmount;

                            if (strItem.IndexOf("人民币账户") != -1)
                            {
                                //正常信用卡最大额度
                                if (crdlnd.CreditLimitAmount > CREDIT_LIMIT_AMOUNT_NORM_MAX)
                                {
                                    CREDIT_LIMIT_AMOUNT_NORM_MAX = (decimal)crdlnd.CreditLimitAmount;
                                }
                                SUM_NORMAL_LIMIT_AMOUNT += (decimal)crdlnd.CreditLimitAmount; //正常信用卡总额度,不包括美元账户
                            }
                            if (crdlnd.UsedCreditLimitAmount != null && crdlnd.UsedCreditLimitAmount > 0)
                            {
                                //正常信用卡使用额度
                                SUM_NORMAL_USE_LIMIT_AMOUNT += (decimal)crdlnd.UsedCreditLimitAmount;
                                //正常信用卡最大已使用额度
                                if (NORMAL_USED_MAX < (decimal)crdlnd.UsedCreditLimitAmount)
                                {
                                    NORMAL_USED_MAX = (decimal)crdlnd.UsedCreditLimitAmount;
                                }
                            }

                            if (NormalCreditOpenTime > crdlnd.OpenDate)
                            {
                                NormalCreditOpenTime = (DateTime)crdlnd.OpenDate;
                            }
                        }
                        credit_dlq_amount += (decimal)crdlnd.CurrOverdueAmount;//逾期金额
                        if (CreditOpenTime > crdlnd.OpenDate)
                        {
                            CreditOpenTime = (DateTime)crdlnd.OpenDate;
                        }
                        //最近九个月发放贷记卡统计
                        if (crdlnd.OpenDate > NowDate.AddMonths(-9) && strItem.IndexOf("人民币账户") != -1)
                        {
                            M9_LEND_AMOUNT += (decimal)crdlnd.CreditLimitAmount;
                            M9_DELIVER_CNT++;
                        }
                        //逾期账户数
                        if (crdlnd.CurrOverdueAmount > 0)
                        {
                            CREDIT_DELAY_CNT++;
                        }

                        reportEntity.CRD_CD_LNDList.Add(crdlnd);
                    }
                    #endregion

                    #region 担保贷款
                    else if (strItem.IndexOf("担保贷款") != -1)
                    {
                        crdguarantee = new CRD_CD_GUARANTEEEntity();
                        crdguarantee.BeginDate = (TempStr.Split('日')[0] + "日").ToDateTime(Consts.DateFormatString6);//开始时间
                        crdguarantee.Name = CommonFun.GetMidStrByRegex(TempStr, "，为", "(");//姓名
                        crdguarantee.CertType = CommonFun.GetMidStrByRegex(TempStr, "证件类型：", "，");//证件类型
                        crdguarantee.CertNo = CommonFun.GetMidStrByRegex(TempStr, "证件号码：", "）");//证件号码
                        crdguarantee.GuaranteeMoney = CommonFun.GetMidStrByRegex(TempStr, "担保金额", "。").ToDecimal(0);//担保金额
                        crdguarantee.GuaranteeBalance = CommonFun.GetMidStrByRegex(TempStr, "担保贷款余额", "。").ToDecimal(0);//担保余额 
                        crdguarantee.OrganName = CommonFun.GetMidStrByRegex(TempStr, "）在", "办理的");//办理机构
                        reportEntity.CRD_CD_GUARANTEEList.Add(crdguarantee);
                    }
                    #endregion

                    #region 贷款
                    else if (TempStr.IndexOf("贷款") != -1)
                    {
                        crdln = new CRD_CD_LNEntity();
                        //贷款种类
                        if (TempStr.IndexOf("住房贷款") != -1)
                        {
                            crdln.TypeDw = "住房贷款";
                        }
                        else if (TempStr.IndexOf("汽车贷款") != -1)
                        {
                            crdln.TypeDw = "汽车贷款";
                        }
                        else if (TempStr.IndexOf("个人经营性贷款") != -1)
                        {
                            crdln.TypeDw = "个人经营性贷款";
                        }
                        else if (TempStr.IndexOf("助学贷款") != -1)
                        {
                            crdln.TypeDw = "助学贷款";
                        }
                        else if (TempStr.IndexOf("公积金贷款") != -1)
                        {
                            crdln.TypeDw = "个人住房公积金贷款";
                        }
                        else if (TempStr.IndexOf("商铺贷款") != -1)
                        {
                            crdln.TypeDw = "个人住房商铺贷款";
                        }
                        else if (TempStr.IndexOf("个人消费贷款") != -1)
                        {
                            crdln.TypeDw = "个人消费贷款";
                        }
                        else if (TempStr.IndexOf("其他贷款") != -1)
                        {
                            crdln.TypeDw = "其他贷款";
                        }
                        else if (TempStr.IndexOf("个人商用房") != -1)
                        {
                            crdln.TypeDw = "个人商用房";
                        }
                        else if (TempStr.IndexOf("农户贷款") != -1)
                        {
                            crdln.TypeDw = "农户贷款";
                        }
                        else
                        {
                            continue;
                        }

                        crdln.Cue = TempStr;//所有信息
                        crdln.OpenDate = (TempStr.Split('日')[0] + "日").ToDateTime(Consts.DateFormatString6);//贷款日期

                        //币种
                        if (TempStr.IndexOf("美元") != -1)
                        {
                            crdln.Currency = "美元";
                        }
                        else if (TempStr.IndexOf("人民币") != -1)
                        {
                            crdln.Currency = "人民币";
                        }
                        crdln.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "逾期金额", "。").ToDecimal(0);
                        crdln.OverdueCyc = CommonFun.GetMidStrByRegex(TempStr, "最近5年内有", "个").ToInt(0);
                        crdln.OverdueOver90Cyc = CommonFun.GetMidStrByRegex(TempStr, "逾期状态，其中", "个").ToInt(0);
                        crdln.FinanceOrg = CommonFun.GetMidStrByRegex(TempStr, "日", "发放的");//发放机构
                        //合同金额
                        crdln.CreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "发放的", "元").ToDecimal(0);
                        if (crdln.CreditLimitAmount == 0 && strItem.IndexOf("合同金额") != -1)
                        {
                            crdln.CreditLimitAmount = CommonFun.GetMidStrByRegex(TempStr, "合同金额", "，").ToDecimal(0);
                        }


                        //余额
                        if (TempStr.IndexOf("余额") != -1)
                        {
                            loan_Balance = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (loan_Balance == 0)
                            {
                                loan_Balance = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }
                            crdln.Balance = loan_Balance;
                            SUM_LOAN_BALANCE += loan_Balance;
                        }

                        //贷款状态
                        if (TempStr.IndexOf("结清") != -1)
                        {
                            int length = TempStr.Split('，').Length;
                            crdln.EndDate = (TempStr.Split('，')[length - 1]).Replace("结清", "").ToDateTime(Consts.DateFormatString7);//结清日期
                            crdln.State = "结清";
                        }
                        else if (TempStr.IndexOf("已转出") != -1)
                        {
                            int length = TempStr.Split('，').Length;
                            crdln.EndDate = (CommonFun.GetMidStrByRegex(TempStr, "贷款，", "已转出")).ToDateTime(Consts.DateFormatString8);//转出
                            crdln.State = "转出";
                        }
                        else if (TempStr.Contains("已变成呆账"))
                        {
                            crdln.State = "呆账";

                            crdln.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "。").ToDecimal(0);
                            if (crdln.CurrOverdueAmount == 0)
                            {
                                crdln.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "余额", "，").ToDecimal(0);
                            }
                        }
                        else
                        {
                            crdln.EndDate = (CommonFun.GetMidStrByRegex(TempStr, "，", "到期")).ToDateTime(Consts.DateFormatString6);
                            crdln.State = "正常";
                            int intervalMonth = CommonFun.GetIntervalOf2DateTime((DateTime)crdln.EndDate, (DateTime)crdln.OpenDate, "M");
                            int surplusMonth = 0;//剩余期数
                            DateTime? DeadlineDate = CommonFun.GetMidStrByRegex(TempStr, "截至", "，").ToDateTime(Consts.DateFormatString8);//截止时间

                            if (DeadlineDate != null)
                            {
                                surplusMonth = CommonFun.GetIntervalOf2DateTime((DateTime)((DateTime)crdln.EndDate).ToString(Consts.DateFormatString8).ToDateTime(), (DateTime)DeadlineDate, "M");
                                surplusMonth++;
                            }
                            //期数大于0且余额大于0
                            if (intervalMonth > 0 && crdln.Balance > 0)
                            {
                                #region 判断每月还息，最后还本的情况 2016-04-14

                                if (surplusMonth < intervalMonth && crdln.Balance == crdln.CreditLimitAmount)//每月还息，最后一次还本
                                {
                                    loan_pmt_monthly += 0;
                                    loan_house_pmt_monthly += 0;
                                }
                                else
                                {
                                    loan_pmt_monthly += (decimal)(crdln.CreditLimitAmount / intervalMonth);//贷款每月还款金额
                                    //住房贷款每月还款金额
                                    if (crdln.TypeDw == "住房贷款")
                                    {
                                        loan_house_pmt_monthly += (decimal)(crdln.CreditLimitAmount / intervalMonth);
                                    }
                                }
                                #endregion
                            }

                            //比较是否未还清最早贷款
                            if (NormalLoanOpenTime > crdln.OpenDate)
                            {
                                NormalLoanOpenTime = (DateTime)crdln.OpenDate;
                            }
                        }

                        //最近还款
                        string recentPayDate = CommonFun.GetMidStr(TempStr, "截至", "，");
                        if (!recentPayDate.IsEmpty())
                        {
                            crdln.RecentPayDate = recentPayDate.ToDateTime(Consts.DateFormatString8);
                        }
                        //剩余还款期数
                        if (crdln.EndDate != null && crdln.RecentPayDate != null)
                        {
                            crdln.RemainPaymentCyc = (decimal)CommonFun.GetIntervalOf2DateTime((DateTime)crdln.EndDate, (DateTime)crdln.RecentPayDate, "M");
                        }

                        crdln.CurrOverdueAmount = CommonFun.GetMidStrByRegex(TempStr, "逾期金额", "。").ToDecimal(0);//当前贷款逾期金额
                        loan_dlq_amount += (decimal)crdln.CurrOverdueAmount;
                        //比较是否最早贷款
                        if (LoanOpenTime > crdln.OpenDate)
                        {
                            LoanOpenTime = (DateTime)crdln.OpenDate;
                        }
                        SUM_LOAN_LIMIT_AMOUNT += (decimal)crdln.CreditLimitAmount;//贷款总额
                        reportEntity.CRD_CD_LNList.Add(crdln);
                    }
                    #endregion

                    #region 保证人代偿
                    else if (strItem.IndexOf("代偿") != -1)
                    {
                        var assrre = GetAsrrepayListFromReport(strItem.Trim());
                        if (assrre != null)
                        {
                            reportEntity.CRD_CD_ASRREPAYList.Add(assrre);
                        }
                    }

                    #endregion
                }
                #endregion

                #region 信用卡相关
                statlnd.CREDITDLQAMOUNT = credit_dlq_amount;//信用卡逾期金额
                statlnd.NORMALCARDNUM = NORMAL_CARDNUM;//正常卡数据量
                statlnd.NORMALCREDITBALANCE = SUM_NORMAL_LIMIT_AMOUNT - SUM_NORMAL_USE_LIMIT_AMOUNT;//正常信用卡未用额度
                statlnd.SUMNORMALLIMITAMOUNT = SUM_NORMAL_LIMIT_AMOUNT;//正常信用卡总信用额度
                statlnd.NORMALUSEDMAX = NORMAL_USED_MAX;//正常信用卡最大已使用额度
                //正常信用卡使用率
                if (SUM_NORMAL_LIMIT_AMOUNT > 0)
                {
                    statlnd.NORMALUSEDRATE = SUM_NORMAL_USE_LIMIT_AMOUNT / SUM_NORMAL_LIMIT_AMOUNT;
                }
                else
                {
                    statlnd.NORMALUSEDRATE = -9999998;
                }
                statlnd.CREDITLIMITAMOUNTNORMMAX = CREDIT_LIMIT_AMOUNT_NORM_MAX;//正常信用卡最大额度
                statlnd.CARDAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, CreditOpenTime, "M");//最早卡龄
                statlnd.NORMALCARDAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, NormalCreditOpenTime, "M");//正常卡最早卡龄
                statlnd.SUMUSEDCREDITLIMITAMOUNT = SUM_NORMAL_USE_LIMIT_AMOUNT;//信用卡已使用总额度

                statlnd.M9LENDAMOUNT = M9_LEND_AMOUNT;//最近九个月发放贷记卡数量
                statlnd.M9DELIVERCNT = M9_DELIVER_CNT;//最近九个月发放贷记卡总金额
                //最近九个月发放贷记卡平均金额
                if (M9_DELIVER_CNT > 0)
                {
                    statlnd.M9AVGLENDAMOUNT = M9_LEND_AMOUNT / M9_DELIVER_CNT;
                }
                else
                {
                    statlnd.M9AVGLENDAMOUNT = 0;
                }
                statlnd.CREDITDELAYCNT = CREDIT_DELAY_CNT;//逾期账户数

                //判断是否有信用卡,没有的话赋默认值
                if (reportEntity.CRD_CD_LNDList.Count == 0 && reportEntity.CRD_CD_STNCARDList.Count == 0)
                {
                    statlnd.M9AVGLENDAMOUNT = -9999999;
                    statlnd.M9DELIVERCNT = -9999999;
                    statlnd.M9LENDAMOUNT = -9999999;
                    statlnd.SUMUSEDCREDITLIMITAMOUNT = -9999999;
                    statlnd.CARDAGEMONTH = -9999999;
                    statlnd.NORMALCREDITBALANCE = -9999999;
                    statlnd.NORMALUSEDRATE = -9999999;
                }
                reportEntity.CRD_STAT_LND = statlnd;
                #endregion

                #region 贷款相关
                statln.LOANAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, LoanOpenTime, "M");//最早贷龄
                statln.NORMALLOANAGEMONTH = CommonFun.GetIntervalOf2DateTime(NowDate, NormalLoanOpenTime, "M");//未还清的最早贷龄
                statln.LOANDLQAMOUNT = loan_dlq_amount;//贷款逾期金额
                statln.LOANPMTMONTHLY = loan_pmt_monthly;//贷款每月还款本金金额
                statln.LOAN_HOUSE_DLQ_AMOUNT = loan_house_pmt_monthly;//房贷每月还款本金金额
                statln.SUMLOANBALANCE = SUM_LOAN_BALANCE;//贷款总余额
                statln.SUMLOANLIMITAMOUNT = SUM_LOAN_LIMIT_AMOUNT;//贷款本金总额

                //判断是否有贷款,没有的话赋默认值
                if (reportEntity.CRD_CD_LNList.Count == 0)
                {
                    statln.LOANAGEMONTH = -9999999;
                    statln.LOANDLQAMOUNT = -9999999;
                    statln.LOANPMTMONTHLY = -9999999;
                }
                reportEntity.CRD_STAT_LN = statln;
                #endregion

                #region 征信数据入库
                Res.CRD_STAT_LN = jsonParser.DeserializeObject<CRD_STAT_LN>(jsonParser.SerializeObject(statln));
                Res.CRD_STAT_LND = jsonParser.DeserializeObject<CRD_STAT_LND>(jsonParser.SerializeObject(statlnd));
                Res.CRD_STAT_QR = jsonParser.DeserializeObject<CRD_STAT_QR>(jsonParser.SerializeObject(statqr));

                //Task.Factory.StartNew(() =>
                //{
                try
                {
                    if (count == 0)
                    {
                        CRD_ACCOUNTEntity accountEntity = accountService.Find("from CRD_ACCOUNTEntity where UserName=?", new object[] { loginname }).FirstOrDefault();

                        if (accountEntity != null)
                        {
                            if (accountEntity.QueryCode != queryReq.querycode)
                            {
                                accountEntity.QueryCode = queryReq.querycode;
                                accountService.Update(accountEntity);
                            }
                        }

                        reportEntity.BusId = queryReq.BusId;
                        reportEntity.BusType = queryReq.BusType;
                        reportEntity.BusIdentityCard = queryReq.IdentityCard;//业务身份证号
                        if (accountEntity != null)
                        {
                            reportEntity.AcountId = accountEntity.Id;
                            reportEntity.Loginname = accountEntity.UserName;
                            reportEntity.Password = accountEntity.Password;
                        }
                        reportService.Save(reportEntity);
                    }
                }
                catch (Exception e)
                {
                    Log4netAdapter.WriteError("保存原始征信报告异常,token:" + queryReq.Token, e);
                }
                //});
                #endregion

                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "央行征信报告查询成功";
                Log4netAdapter.WriteInfo("央行互联网征信报告查询,结束,token:" + queryReq.Token);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "央行征信报告解析异常";
                Log4netAdapter.WriteError("央行征信报告解析异常,token:" + queryReq.Token, e);
            }

            return Res;
        }
        /// <summary>
        /// 强制执行记录段
        /// </summary>
        /// <param name="tRows"></param>
        /// <returns></returns>

        public CRD_PI_FORCEEXCTNEntity GetForceexctnFromReport(List<string> tRows)
        {
            try
            {
                if (tRows.Count == 0) return null;
                var forceextn = new CRD_PI_FORCEEXCTNEntity();
                tRows.ForEach(e =>
                {
                    var tRows1 = HtmlParser.GetResultFromParser(e, "//td", "", true);
                    if (tRows1.Count == 1)
                    {
                        return;
                    }
                    tRows1.ForEach(r =>
                    {
                        r = r.Replace("</strong>", "").Replace("<strong>", "");
                        if (r.Contains("执行法院："))
                        {
                            forceextn.Court = CommonFun.GetMidStrByRegex(r, "执行法院：").Trim();
                        }
                        else if (r.Contains("案号："))
                        {
                            forceextn.Case_No = CommonFun.GetMidStrByRegex(r, "案号：").Trim();
                        }
                        else if (r.Contains("执行案由："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "执行案由：").Trim();
                            forceextn.Case_Reason = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("结案方式："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "结案方式：").Trim();
                            forceextn.Closed_Type = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("立案时间："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "立案时间：").Trim();
                            if (rsl == "--")
                                forceextn.Register_Date = null;
                            else
                            {
                                forceextn.Register_Date = (rsl + "1日").ToDateTime(Consts.DateFormatString6);
                            }
                        }
                        else if (r.Contains("案件状态："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "案件状态：").Trim();
                            forceextn.Case_State = rsl == "--" ? null : rsl; ;
                        }
                        else if (r.Contains("申请执行标的："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "申请执行标的：").Trim();
                            forceextn.Enforce_Object = rsl == "--" ? null : rsl; ;
                        }
                        else if (r.Contains("已执行标的："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "已执行标的：").Trim();
                            forceextn.Already_Enforce_Object = rsl == "--" ? null : rsl; ;
                        }
                        else if (r.Contains("申请执行标的金额："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "申请执行标的金额：").Trim();
                            if (rsl == "--")
                                forceextn.Enforce_Object_Money = null;
                            else
                            {
                                forceextn.Enforce_Object_Money = rsl.ToDecimal();
                            }
                        }
                        else if (r.Contains("已执行标的金额："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "已执行标的金额：").Trim();
                            if (rsl == "--")
                                forceextn.Already_Enforce_Object_Money = null;
                            else
                            {
                                forceextn.Already_Enforce_Object_Money = rsl.ToDecimal();
                            }
                        }

                        else if (r.Contains("结案时间："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "结案时间：").Trim();
                            if (rsl == "--")
                                forceextn.Closed_Date = null;
                            else
                            {
                                forceextn.Closed_Date = (rsl + "1日").ToDateTime(Consts.DateFormatString6);
                            }

                        }

                    });
                });
                return forceextn;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("公共信息-强制执行", ex);
                return null;
            }

        }
        /// <summary>
        /// 欠税记录
        /// </summary>
        /// <param name="tRows"></param>
        /// <returns></returns>

        public CRD_PI_TAXARREAREntity GetTaxarrearFromReport(List<string> tRows)
        {
            try
            {
                if (tRows.Count == 0) return null;
                var tax = new CRD_PI_TAXARREAREntity();
                tRows.ForEach(e =>
                {
                    var tRows1 = HtmlParser.GetResultFromParser(e, "//td", "", true);
                    if (tRows1.Count == 1)
                    {
                        return;
                    }
                    tRows1.ForEach(r =>
                    {
                        r = r.Replace("</strong>", "").Replace("<strong>", "");
                        if (r.Contains("主管税务机关："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "主管税务机关：").Trim();
                            tax.Organ_Name = rsl;
                        }
                        else if (r.Contains("欠税统计时间："))
                        {
                            var rsl = (CommonFun.GetMidStrByRegex(r, "欠税统计时间：") + "1日").ToDateTime(Consts.DateFormatString6);
                            tax.Tax_Arrear_Date = rsl;
                        }
                        else if (r.Contains("欠税总额："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "欠税总额：").ToDecimal();
                            tax.Tax_Arrea_Amount = rsl;
                        }
                    });

                });
                return tax;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("公共信息-欠税记录", ex);
                return null;
            }

        }
        /// <summary>
        /// 保证人代偿信息 
        /// </summary>
        /// <param name="strItem"></param>
        /// <returns></returns>
        public CRD_CD_ASRREPAYEntity GetAsrrepayListFromReport(string strItem)
        {
            if (strItem.IsEmpty()) return null;

            try
            {
                var asrrepaye = new CRD_CD_ASRREPAYEntity();
                asrrepaye.Latest_Assurer_Repay_Date = (strItem.Split('日')[0] + "日").ToDateTime(Consts.DateFormatString6);//开始时间
                asrrepaye.Organ_Name = CommonFun.GetMidStrByRegex(strItem, "日", "进行").Trim();
                asrrepaye.Money = CommonFun.GetMidStrByRegex(strItem, "代偿金额", "。").Trim().ToDecimal();
                asrrepaye.Balance = CommonFun.GetMidStrByRegex(strItem, "余额", "。").Trim().ToDecimal();
                var latestrepaydate = CommonFun.GetMidStrByRegex(strItem, "还款日期为", "，").Trim();
                if (!latestrepaydate.IsEmpty())
                {
                    asrrepaye.Latest_Repay_Date = latestrepaydate.ToDateTime(Consts.DateFormatString6);//最近一次还款日期
                }
                return asrrepaye;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("公共信息-保证人代偿信息", ex);
                return null;
            }

        }


        /// <summary>
        /// 电信欠费信息
        /// </summary>
        /// <param name="strItem"></param>
        /// <returns></returns>
        public CRD_PI_TELPNTEntity GetTelpntListFromReport(List<string> tRows)
        {

            try
            {
                if (tRows.Count == 0) return null;
                var tax = new CRD_PI_TELPNTEntity();
                tRows.ForEach(e =>
                {
                    var tRows1 = HtmlParser.GetResultFromParser(e, "//td", "", true);
                    if (tRows1.Count == 1)
                    {
                        return;
                    }
                    tRows1.ForEach(r =>
                    {
                        r = r.Replace("</strong>", "").Replace("<strong>", "");
                        if (r.Contains("电信运营商："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "电信运营商：").Trim();
                            tax.Organ_Name = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("业务类型："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "业务类型：").Trim();
                            tax.Type_Dw = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("记账年月："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "记账年月：").Trim();
                            tax.Get_Time = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("业务开通时间："))
                        {
                            var rsl = (CommonFun.GetMidStrByRegex(r, "业务开通时间：").Trim() + "1日").ToDateTime();
                            tax.Register_Date = rsl;
                        }
                        else if (r.Contains("欠费金额："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "欠费金额：").Trim().ToDecimal();
                            tax.Arrear_Money = rsl;
                        }

                    });

                });
                return tax;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("公共信息-电信欠费信息", ex);
                return null;
            }
        }

        /// <summary>
        /// 民事判决记录段
        /// </summary>
        /// <param name="strItem"></param>
        /// <returns></returns>
        public CRD_PI_CIVILJDGMEntity GetCiviljdgmListFromReport(List<string> tRows)
        {

            try
            {
                if (tRows.Count == 0) return null;
                var tax = new CRD_PI_CIVILJDGMEntity();
                tRows.ForEach(e =>
                {
                    var tRows1 = HtmlParser.GetResultFromParser(e, "//td", "", true);
                    if (tRows1.Count == 1)
                    {
                        return;
                    }
                    tRows1.ForEach(r =>
                    {
                        r = r.Replace("</strong>", "").Replace("<strong>", "");
                        if (r.Contains("立案法院："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "立案法院：").Trim();
                            tax.Court = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("案号："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "案号：").Trim();
                            tax.Case_No = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("案由："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "案由：").Trim();
                            tax.Case_Reason = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("立案时间："))
                        {
                            var rsl = (CommonFun.GetMidStrByRegex(r, "立案时间：").Trim() + "1日").ToDateTime();
                            tax.Register_Date = rsl;
                        }
                        else if (r.Contains("结案方式："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "结案方式：").Trim();
                            tax.Closed_Type = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("判决/调解结果："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "判决/调解结果：").Trim();
                            tax.Case_Result = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("诉讼标的："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "诉讼标的：").Trim();
                            tax.Suit_Object = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("判决/调解生效时间："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "判决/调解生效时间：").Trim();
                            tax.Case_Validate_Date = rsl.ToDateTime();
                        }
                        else if (r.Contains("诉讼标的金额："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "诉讼标的金额：").Trim();
                            tax.Suit_Object_Money = rsl.ToDecimal();
                        }
                    });

                });
                return tax;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("公共信息-民事判决记录段", ex);
                return null;
            }
        }

        /// <summary>
        /// 行政处罚记录段
        /// </summary>
        /// <param name="strItem"></param>
        /// <returns></returns>
        public CRD_PI_ADMINPNSHMEntity GetAdminpnshmListFromReport(List<string> tRows)
        {

            try
            {
                if (tRows.Count == 0) return null;
                var tax = new CRD_PI_ADMINPNSHMEntity();
                tRows.ForEach(e =>
                {
                    var tRows1 = HtmlParser.GetResultFromParser(e, "//td", "", true);
                    if (tRows1.Count == 1)
                    {
                        return;
                    }
                    tRows1.ForEach(r =>
                    {
                        r = r.Replace("</strong>", "").Replace("<strong>", "");
                        if (r.Contains("处罚机构："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "处罚机构：").Trim();
                            tax.Organ_Name = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("文书编号："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "文书编号：").Trim();
                            tax.Case_No = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("处罚内容："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "处罚内容：").Trim();
                            tax.Content = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("处罚金额："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "处罚金额：").Trim().ToDecimal();
                            tax.Money = rsl;
                        }
                        else if (r.Contains("行政复议结果："))
                        {
                            var rsl = CommonFun.GetMidStrByRegex(r, "行政复议结果：").Trim();
                            tax.Result_Dw = rsl == "--" ? null : rsl;
                        }
                        else if (r.Contains("处罚生效时间："))
                        {
                            var rsl = (CommonFun.GetMidStrByRegex(r, "处罚生效时间：").Trim() + "1日").ToDateTime();
                            tax.Begin_Date = rsl;
                        }
                        else if (r.Contains("处罚截止时间："))
                        {
                            var rsl = (CommonFun.GetMidStrByRegex(r, "处罚截止时间：").Trim() + "1日").ToDateTime();
                            tax.End_Date = rsl;
                        }
                    });

                });
                return tax;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("公共信息-行政处罚记录段", ex);
                return null;
            }
        }
        /// <summary>
        /// 获取征信查看记录
        /// </summary>
        /// <param name="repid"></param>
        /// <returns></returns>
        public BaseRes GetListCrdQrrecorddtlByrepid(int repid)
        {
            ICRD_QR_RECORDDTL service = NetSpiderFactoryManager.GetCRDQRRECORDDTLService();
            BaseRes Res = new BaseRes();
            try
            {
                Res.StatusCode = ServiceConsts.StatusCode_success;
                Res.StatusDescription = "获取查看征信记录成功";
                Res.Result = jsonParser.SerializeObject(service.GetListByReportId(repid));
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "获取查看记录异常";
                Log4netAdapter.WriteError("获取查看记录异常,repid:" + repid, e);
            }

            return Res;
        }
        public static bool RemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 银联认证CVN2加密
        /// </summary>
        /// <param name="cvn2"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetCVN2(string cvn2, string key)
        {
            string ret = string.Empty;

            string[] key_list = new string[] { };
            try
            {
                key_list = CommonFun.GetMidStr(key, "[", "]").Split(',');
                if (key_list.Count() > 0)
                {
                    List<int> ee = new List<int>();
                    foreach (string i in key_list.Skip(203).Take(95))
                    {
                        ee.Add(int.Parse(i));
                    }

                    System.Collections.Hashtable e = new System.Collections.Hashtable();
                    for (int i = 0; i < ee.Count; i++)
                    {
                        e.Add(ee[i], i);
                    }

                    string a = string.Empty;
                    for (int j = 0; j < cvn2.Length; j++)
                    {
                        a += Convert.ToChar((int)e[cvn2.CharAt(j)[0] - 32] + 32);
                    }
                    for (int f = 0; f < a.Length; f++)
                    {
                        ret += Convert.ToChar(((a.CharAt(f)[0] - 32 + f * (f + 1029600 + f) * f) % 95 + 95) % 95 + 32);

                    }
                }

            }
            catch { }
            return ret;
        }
        private static string UnionEncypt(string cardNumber, string password, string modulus)
        {
            string ret = string.Empty;
            string strjs = @" function formatToHex(b, a) {
                var c = Array(8),d = Array(8);
                c[0] = '0x06';
                d[0] = '0x00';
                d[1] = '0x00';
                for (var e = 1, h = 2, l = '', g = 0; g < b.length; g += 2) c[e++] = '0x' + b.charAt(g) + b.charAt(g + 1);
                c[4] = '0xFF';
                c[5] = '0xFF';
                c[6] = '0xFF';
                c[7] = '0xFF';
                for (g = a.length - 13; g < a.length - 1; g += 2) d[h++] = '0x' + a.charAt(g) + a.charAt(g + 1);
                for (g = 0; 8 > g; g++) l += convertToHex(d[g] ^ c[g]);
                return do_encrypt(l)
            }

            function convertToHex(b) {
                b = b.toString(16);
                1 == b.length && (b = '0' + b);
                return b = '0x' + b.toUpperCase()
            }

            function do_encrypt(b) {
                var a = '" + modulus + @"';
                var c = new RSAKey;
                c.setPublic(a, '10001');
                if (b = c.encrypt(b)) return b;
            }
            var dbits, canary = 0xdeadbeefcafe,j_lm = 15715070 == (canary & 16777215);

            function BigInteger(b, a, c) {
                null != b && ('number' == typeof b ? this.fromNumber(b, a, c) : null == a && 'string' != typeof b ? this.fromString(b, 256) : this.fromString(b, a))
            }
            function nbi() {
                return new BigInteger(null)
            }
            function am1(b, a, c, d, e, h) {
                for (; 0 <= --h; ) {
                    var l = a * this[b++] + c[d] + e,
			e = Math.floor(l / 67108864);
                    c[d++] = l & 67108863
                }
                return e
            }

            function am2(b, a, c, d, e, h) {
                for (var l = a & 32767, a = a >> 15; 0 <= --h; ) {
                    var g = this[b] & 32767,
			p = this[b++] >> 15,
			q = a * g + p * l,
			g = l * g + ((q & 32767) << 15) + c[d] + (e & 1073741823),
			e = (g >>> 30) + (q >>> 15) + a * p + (e >>> 30);
                    c[d++] = g & 1073741823
                }
                return e
            }
            function am3(b, a, c, d, e, h) {
                for (var l = a & 16383, a = a >> 14; 0 <= --h; ) {
                    var g = this[b] & 16383,
			p = this[b++] >> 14,
			q = a * g + p * l,
			g = l * g + ((q & 16383) << 14) + c[d] + e,
			e = (g >> 28) + (q >> 14) + a * p;
                    c[d++] = g & 268435455
                }
                return e
            }
            BigInteger.prototype.am = am3, dbits = 28;
            BigInteger.prototype.DB = dbits;
            BigInteger.prototype.DM = (1 << dbits) - 1;
            BigInteger.prototype.DV = 1 << dbits;
            var BI_FP = 52;
            BigInteger.prototype.FV = Math.pow(2, BI_FP);
            BigInteger.prototype.F1 = BI_FP - dbits;
            BigInteger.prototype.F2 = 2 * dbits - BI_FP;
            var BI_RM = '0123456789abcdefghijklmnopqrstuvwxyz',BI_RC = [],rr, vv;
            rr = 48;
            for (vv = 0; 9 >= vv; ++vv) BI_RC[rr++] = vv;
            rr = 97;
            for (vv = 10; 36 > vv; ++vv) BI_RC[rr++] = vv;
            rr = 65;
            for (vv = 10; 36 > vv; ++vv) BI_RC[rr++] = vv;

            function int2char(b) {
                return BI_RM.charAt(b)
            }
            function intAt(b, a) {
                var c = BI_RC[b.charCodeAt(a)];
                return c == null ? -1 : c
            }
            function bnpCopyTo(b) {
                for (var a = this.t - 1; a >= 0; --a) b[a] = this[a];
                b.t = this.t;
                b.s = this.s
            }
            function bnpFromInt(b) {
                this.t = 1;
                this.s = b < 0 ? -1 : 0;
                b > 0 ? this[0] = b : b < -1 ? this[0] = b + this.DV : this.t = 0
            }
            function nbv(b) {
                var a = nbi();
                a.fromInt(b);
                return a
            }

            function bnpFromString(b, a) {
                var c;
                if (a == 16) c = 4;
                else if (a == 8) c = 3;
                else if (a == 256) c = 8;
                else if (a == 2) c = 1;
                else if (a == 32) c = 5;
                else if (a == 4) c = 2;
                else {
                    this.fromRadix(b, a);
                    return
                }
                this.s = this.t = 0;
                for (var d = b.length, e = false, h = 0; --d >= 0; ) {
                    var l = c == 8 ? b[d] & 255 : intAt(b, d);
                    if (l < 0) b.charAt(d) == '-' && (e = true);
                    else {
                        e = false;
                        if (h == 0) this[this.t++] = l;
                        else if (h + c > this.DB) {
                            this[this.t - 1] = this[this.t - 1] | (l & (1 << this.DB - h) - 1) << h;
                            this[this.t++] = l >> this.DB - h
                        } else this[this.t - 1] = this[this.t - 1] | l << h;
                        h = h + c;
                        h >= this.DB && (h = h - this.DB)
                    }
                }
                if (c == 8 && (b[0] & 128) != 0) {
                    this.s = -1;
                    h > 0 && (this[this.t - 1] = this[this.t - 1] | (1 << this.DB - h) - 1 << h)
                }
                this.clamp();
                e && BigInteger.ZERO.subTo(this, this)
            }
            function bnpClamp() {
                for (var b = this.s & this.DM; this.t > 0 && this[this.t - 1] == b; ) --this.t
            }

            function bnToString(b) {
                if (this.s < 0) return '-' + this.negate().toString(b);
                if (b == 16) b = 4;
                else if (b == 8) b = 3;
                else if (b == 2) b = 1;
                else if (b == 32) b = 5;
                else if (b == 4) b = 2;
                else return this.toRadix(b);
                var a = (1 << b) - 1,c, d = false,e = '',h = this.t,l = this.DB - h * this.DB % b;
                if (h-- > 0) {
                    if (l < this.DB && (c = this[h] >> l) > 0) {
                        d = true;
                        e = int2char(c)
                    }
                    for (; h >= 0; ) {
                        if (l < b) {
                            c = (this[h] & (1 << l) - 1) << b - l;
                            c = c | this[--h] >> (l = l + (this.DB - b))
                        } else {
                            c = this[h] >> (l = l - b) & a;
                            if (l <= 0) {
                                l = l + this.DB;
                                --h
                            }
                        }
                        c > 0 && (d = true);
                        d && (e = e + int2char(c))
                    }
                }
                return d ? e : '0'
            }

            function bnNegate() {
                var b = nbi();
                BigInteger.ZERO.subTo(this, b);
                return b
            }
            function bnAbs() {
                return this.s < 0 ? this.negate() : this
            }
            function bnCompareTo(b) {
                var a = this.s - b.s;
                if (a != 0) return a;
                var c = this.t,a = c - b.t;
                if (a != 0) return this.s < 0 ? -a : a;
                for (; --c >= 0; ) if ((a = this[c] - b[c]) != 0) return a;
                return 0
            }
            function nbits(b) {
                var a = 1,c;
                if ((c = b >>> 16) != 0) {
                    b = c;
                    a = a + 16
                }
                if ((c = b >> 8) != 0) {
                    b = c;
                    a = a + 8
                }
                if ((c = b >> 4) != 0) {
                    b = c;
                    a = a + 4
                }
                if ((c = b >> 2) != 0) {
                    b = c;
                    a = a + 2
                }
                b >> 1 != 0 && (a = a + 1);
                return a
            }

            function bnBitLength() {
                return this.t <= 0 ? 0 : this.DB * (this.t - 1) + nbits(this[this.t - 1] ^ this.s & this.DM)
            }
            function bnpDLShiftTo(b, a) {
                var c;
                for (c = this.t - 1; c >= 0; --c) a[c + b] = this[c];
                for (c = b - 1; c >= 0; --c) a[c] = 0;
                a.t = this.t + b;
                a.s = this.s
            }
            function bnpDRShiftTo(b, a) {
                for (var c = b; c < this.t; ++c) a[c - b] = this[c];
                a.t = Math.max(this.t - b, 0);
                a.s = this.s
            }

            function bnpLShiftTo(b, a) {
                var c = b % this.DB,d = this.DB - c,e = (1 << d) - 1,h = Math.floor(b / this.DB),l = this.s << c & this.DM,g;
                for (g = this.t - 1; g >= 0; --g) {
                    a[g + h + 1] = this[g] >> d | l;
                    l = (this[g] & e) << c
                }
                for (g = h - 1; g >= 0; --g) a[g] = 0;
                a[h] = l;
                a.t = this.t + h + 1;
                a.s = this.s;
                a.clamp()
            }

            function bnpRShiftTo(b, a) {
                a.s = this.s;
                var c = Math.floor(b / this.DB);
                if (c >= this.t) a.t = 0;
                else {
                    var d = b % this.DB,
			e = this.DB - d,
			h = (1 << d) - 1;
                    a[0] = this[c] >> d;
                    for (var l = c + 1; l < this.t; ++l) {
                        a[l - c - 1] = a[l - c - 1] | (this[l] & h) << e;
                        a[l - c] = this[l] >> d
                    }
                    d > 0 && (a[this.t - c - 1] = a[this.t - c - 1] | (this.s & h) << e);
                    a.t = this.t - c;
                    a.clamp()
                }
            }

            function bnpSubTo(b, a) {
                for (var c = 0, d = 0, e = Math.min(b.t, this.t); c < e; ) {
                    d = d + (this[c] - b[c]);
                    a[c++] = d & this.DM;
                    d = d >> this.DB
                }
                if (b.t < this.t) {
                    for (d = d - b.s; c < this.t; ) {
                        d = d + this[c];
                        a[c++] = d & this.DM;
                        d = d >> this.DB
                    }
                    d = d + this.s
                } else {
                    for (d = d + this.s; c < b.t; ) {
                        d = d - b[c];
                        a[c++] = d & this.DM;
                        d = d >> this.DB
                    }
                    d = d - b.s
                }
                a.s = d < 0 ? -1 : 0;
                d < -1 ? a[c++] = this.DV + d : d > 0 && (a[c++] = d);
                a.t = c;
                a.clamp()
            }

            function bnpMultiplyTo(b, a) {
                var c = this.abs(),d = b.abs(),e = c.t;
                for (a.t = e + d.t; --e >= 0; ) a[e] = 0;
                for (e = 0; e < d.t; ++e) a[e + c.t] = c.am(0, d[e], a, e, 0, c.t);
                a.s = 0;
                a.clamp();
                this.s != b.s && BigInteger.ZERO.subTo(a, a)
            }
            function bnpSquareTo(b) {
                for (var a = this.abs(), c = b.t = 2 * a.t; --c >= 0; ) b[c] = 0;
                for (c = 0; c < a.t - 1; ++c) {
                    var d = a.am(c, a[c], b, 2 * c, 0, 1);
                    if ((b[c + a.t] = b[c + a.t] + a.am(c + 1, 2 * a[c], b, 2 * c + 1, d, a.t - c - 1)) >= a.DV) {
                        b[c + a.t] = b[c + a.t] - a.DV;
                        b[c + a.t + 1] = 1
                    }
                }
                b.t > 0 && (b[b.t - 1] = b[b.t - 1] + a.am(c, a[c], b, 2 * c, 0, 1));
                b.s = 0;
                b.clamp()
            }

            function bnpDivRemTo(b, a, c) {
                var d = b.abs();
                if (!(d.t <= 0)) {
                    var e = this.abs();
                    if (e.t < d.t) {
                        a != null && a.fromInt(0);
                        c != null && this.copyTo(c)
                    } else {
                        c == null && (c = nbi());
                        var h = nbi(),
				l = this.s,
				b = b.s,
				g = this.DB - nbits(d[d.t - 1]);
                        if (g > 0) {
                            d.lShiftTo(g, h);
                            e.lShiftTo(g, c)
                        } else {
                            d.copyTo(h);
                            e.copyTo(c)
                        }
                        d = h.t;
                        e = h[d - 1];
                        if (e != 0) {
                            var p = e * (1 << this.F1) + (d > 1 ? h[d - 2] >> this.F2 : 0),
					q = this.FV / p,
					p = (1 << this.F1) / p,
					u = 1 << this.F2,
					r = c.t,
					o = r - d,
					f = a == null ? nbi() : a;
                            h.dlShiftTo(o, f);
                            if (c.compareTo(f) >= 0) {
                                c[c.t++] = 1;
                                c.subTo(f, c)
                            }
                            BigInteger.ONE.dlShiftTo(d, f);
                            for (f.subTo(h, h); h.t < d; ) h[h.t++] = 0;
                            for (; --o >= 0; ) {
                                var w = c[--r] == e ? this.DM : Math.floor(c[r] * q + (c[r - 1] + u) * p);
                                if ((c[r] = c[r] + h.am(0, w, c, o, 0, d)) < w) {
                                    h.dlShiftTo(o, f);
                                    for (c.subTo(f, c); c[r] < --w; ) c.subTo(f, c)
                                }
                            }
                            if (a != null) {
                                c.drShiftTo(d, a);
                                l != b && BigInteger.ZERO.subTo(a, a)
                            }
                            c.t = d;
                            c.clamp();
                            g > 0 && c.rShiftTo(g, c);
                            l < 0 && BigInteger.ZERO.subTo(c, c)
                        }
                    }
                }
            }
            function bnMod(b) {
                var a = nbi();
                this.abs().divRemTo(b, null, a);
                this.s < 0 && a.compareTo(BigInteger.ZERO) > 0 && b.subTo(a, a);
                return a
            }
            function Classic(b) {
                this.m = b
            }

            function cConvert(b) {
                return b.s < 0 || b.compareTo(this.m) >= 0 ? b.mod(this.m) : b
            }
            function cRevert(b) {
                return b
            }
            function cReduce(b) {
                b.divRemTo(this.m, null, b)
            }
            function cMulTo(b, a, c) {
                b.multiplyTo(a, c);
                this.reduce(c)
            }
            function cSqrTo(b, a) {
                b.squareTo(a);
                this.reduce(a)
            }
            Classic.prototype.convert = cConvert;
            Classic.prototype.revert = cRevert;
            Classic.prototype.reduce = cReduce;
            Classic.prototype.mulTo = cMulTo;
            Classic.prototype.sqrTo = cSqrTo;

            function bnpInvDigit() {
                if (this.t < 1) return 0;
                var b = this[0];
                if ((b & 1) == 0) return 0;
                var a = b & 3,
		a = a * (2 - (b & 15) * a) & 15,
		a = a * (2 - (b & 255) * a) & 255,
		a = a * (2 - ((b & 65535) * a & 65535)) & 65535,
		a = a * (2 - b * a % this.DV) % this.DV;
                return a > 0 ? this.DV - a : -a
            }
            function Montgomery(b) {
                this.m = b;
                this.mp = b.invDigit();
                this.mpl = this.mp & 32767;
                this.mph = this.mp >> 15;
                this.um = (1 << b.DB - 15) - 1;
                this.mt2 = 2 * b.t
            }

            function montConvert(b) {
                var a = nbi();
                b.abs().dlShiftTo(this.m.t, a);
                a.divRemTo(this.m, null, a);
                b.s < 0 && a.compareTo(BigInteger.ZERO) > 0 && this.m.subTo(a, a);
                return a
            }
            function montRevert(b) {
                var a = nbi();
                b.copyTo(a);
                this.reduce(a);
                return a
            }

            function montReduce(b) {
                for (; b.t <= this.mt2; ) b[b.t++] = 0;
                for (var a = 0; a < this.m.t; ++a) {
                    var c = b[a] & 32767,
			d = c * this.mpl + ((c * this.mph + (b[a] >> 15) * this.mpl & this.um) << 15) & b.DM,
			c = a + this.m.t;
                    for (b[c] = b[c] + this.m.am(0, d, b, a, 0, this.m.t); b[c] >= b.DV; ) {
                        b[c] = b[c] - b.DV;
                        b[++c]++
                    }
                }
                b.clamp();
                b.drShiftTo(this.m.t, b);
                b.compareTo(this.m) >= 0 && b.subTo(this.m, b)
            }
            function montSqrTo(b, a) {
                b.squareTo(a);
                this.reduce(a)
            }
            function montMulTo(b, a, c) {
                b.multiplyTo(a, c);
                this.reduce(c)
            }
            Montgomery.prototype.convert = montConvert;
            Montgomery.prototype.revert = montRevert;
            Montgomery.prototype.reduce = montReduce;
            Montgomery.prototype.mulTo = montMulTo;
            Montgomery.prototype.sqrTo = montSqrTo;

            function bnpIsEven() {
                return (this.t > 0 ? this[0] & 1 : this.s) == 0
            }
            function bnpExp(b, a) {
                if (b > 4294967295 || b < 1) return BigInteger.ONE;
                var c = nbi(),d = nbi(),e = a.convert(this),h = nbits(b) - 1;
                for (e.copyTo(c); --h >= 0; ) {
                    a.sqrTo(c, d);
                    if ((b & 1 << h) > 0) a.mulTo(d, e, c);
                    else var l = c,
			c = d,
			d = l
                }
                return a.revert(c)
            }

            function bnModPowInt(b, a) {
                var c;
                c = b < 256 || a.isEven() ? new Classic(a) : new Montgomery(a);
                return this.exp(b, c)
            }
            BigInteger.prototype.copyTo = bnpCopyTo;
            BigInteger.prototype.fromInt = bnpFromInt;
            BigInteger.prototype.fromString = bnpFromString;
            BigInteger.prototype.clamp = bnpClamp;
            BigInteger.prototype.dlShiftTo = bnpDLShiftTo;
            BigInteger.prototype.drShiftTo = bnpDRShiftTo;
            BigInteger.prototype.lShiftTo = bnpLShiftTo;
            BigInteger.prototype.rShiftTo = bnpRShiftTo;
            BigInteger.prototype.subTo = bnpSubTo;
            BigInteger.prototype.multiplyTo = bnpMultiplyTo;
            BigInteger.prototype.squareTo = bnpSquareTo;
            BigInteger.prototype.divRemTo = bnpDivRemTo;
            BigInteger.prototype.invDigit = bnpInvDigit;
            BigInteger.prototype.isEven = bnpIsEven;
            BigInteger.prototype.exp = bnpExp;
            BigInteger.prototype.toString = bnToString;
            BigInteger.prototype.negate = bnNegate;
            BigInteger.prototype.abs = bnAbs;
            BigInteger.prototype.compareTo = bnCompareTo;
            BigInteger.prototype.bitLength = bnBitLength;
            BigInteger.prototype.mod = bnMod;
            BigInteger.prototype.modPowInt = bnModPowInt;
            BigInteger.ZERO = nbv(0);
            BigInteger.ONE = nbv(1);

            function Arcfour() {
                this.j = this.i = 0;
                this.S = []
            }
            function ARC4init(b) {
                var a, c, d;
                for (a = 0; a < 256; ++a) this.S[a] = a;
                for (a = c = 0; a < 256; ++a) {
                    c = c + this.S[a] + b[a % b.length] & 255;
                    d = this.S[a];
                    this.S[a] = this.S[c];
                    this.S[c] = d
                }
                this.j = this.i = 0
            }
            function ARC4next() {
                var b;
                this.i = this.i + 1 & 255;
                this.j = this.j + this.S[this.i] & 255;
                b = this.S[this.i];
                this.S[this.i] = this.S[this.j];
                this.S[this.j] = b;
                return this.S[b + this.S[this.i] & 255]
            }
            Arcfour.prototype.init = ARC4init;
            Arcfour.prototype.next = ARC4next;

            function prng_newstate() {
                return new Arcfour
            }
            var rng_psize = 256,rng_state, rng_pool, rng_pptr;

            function rng_seed_int(b) {
                rng_pool[rng_pptr++] ^= b & 255;
                rng_pool[rng_pptr++] ^= b >> 8 & 255;
                rng_pool[rng_pptr++] ^= b >> 16 & 255;
                rng_pool[rng_pptr++] ^= b >> 24 & 255;
                rng_pptr >= rng_psize && (rng_pptr = rng_pptr - rng_psize)
            }
            function rng_seed_time() {
                rng_seed_int((new Date).getTime())
            }
            if (null == rng_pool) {
                rng_pool = [];
                rng_pptr = 0;
                var t;
                for (; rng_pptr < rng_psize; ) t = Math.floor(65536 * Math.random()), rng_pool[rng_pptr++] = t >>> 8, rng_pool[rng_pptr++] = t & 255;
                rng_pptr = 0;
                rng_seed_time()
            }

            function rng_get_byte() {
                if (rng_state == null) {
                    rng_seed_time();
                    rng_state = prng_newstate();
                    rng_state.init(rng_pool);
                    for (rng_pptr = 0; rng_pptr < rng_pool.length; ++rng_pptr) rng_pool[rng_pptr] = 0;
                    rng_pptr = 0
                }
                return rng_state.next()
            }
            function rng_get_bytes(b) {
                var a;
                for (a = 0; a < b.length; ++a) b[a] = rng_get_byte()
            }
            function SecureRandom() { }
            SecureRandom.prototype.nextBytes = rng_get_bytes;

            function parseBigInt(b, a) {
                return new BigInteger(b, a)
            }

            function pkcs1pad2(b, a) {
                for (var c = [], d = 248, e = 0; e < b.length; e = e + 4) c[d++] = parseInt(b.substring(e, e + 4));
                a = 248;
                c[--a] = 0;
                d = new SecureRandom;
                for (e = []; a > 2; ) {
                    for (e[0] = 0; e[0] == 0; ) d.nextBytes(e);
                    c[--a] = e[0]
                }
                c[--a] = 2;
                c[--a] = 0;
                return new BigInteger(c)
            }

            function RSAKey() {
                this.n = null;
                this.e = 0;
                this.coeff = this.dmq1 = this.dmp1 = this.q = this.p = this.d = null
            }
            function RSASetPublic(b, a) {
                if (b != null && a != null && b.length > 0 && a.length > 0) {
                    this.n = parseBigInt(b, 16);
                    this.e = parseInt(a, 16)
                }
            }
            function RSADoPublic(b) {
                return b.modPowInt(this.e, this.n)
            }
            function RSAEncrypt(b) {
                b = pkcs1pad2(b, this.n.bitLength() + 7 >> 3);
                if (b == null) return null;
                b = this.doPublic(b);
                if (b == null) return null;
                b = b.toString(16);
                return (b.length & 1) == 0 ? b : '0' + b
            }
            RSAKey.prototype.doPublic = RSADoPublic;
            RSAKey.prototype.setPublic = RSASetPublic;
            RSAKey.prototype.encrypt = RSAEncrypt;";
            object str1 = JavaScriptHelper.JavaScriptEval(strjs, "formatToHex('" + password + "','" + cardNumber.ToTrim() + "')");
            ret = str1.ToString();
            return ret;
        }

        private string GetProxyIP()
        {
            string proxyip = string.Empty;
            string proxyipService = string.Empty;
            try
            {
                proxyipService = Vcredit.Common.Helper.ConfigurationHelper.GetAppSetting("proxyipServiceUrl");
                if (!proxyipService.IsEmpty())
                {
                    httpItem = new HttpItem()
                    {
                        URL = proxyipService + "/query/GetProxyIp",
                        Timeout = 3000,
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        proxyipResult proxyidRes = jsonParser.DeserializeObject<proxyipResult>(httpResult.Html);
                        if (proxyidRes != null)
                        {
                            proxyip = proxyidRes.Result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("获取代理IP出错", e);
            }
            return proxyip;
        }
        private static string GenerateIP()
        {
            string forwardedIp = "43.226.162.107";
            try
            {
                Random ran = new Random();
                forwardedIp = string.Format("{0}.{1}.{2}.{3}", "60", ran.Next(160, 191), ran.Next(1, 250), ran.Next(1, 250));

                //if (CacheHelper.GetCache("forwardedIp") == null)
                //{
                //    TimeSpan tspan = TimeSpan.FromMinutes(1);
                //    Random ran = new Random();
                //    forwardedIp = string.Format("{0}.{1}.{2}.{3}", "60", ran.Next(160, 191), ran.Next(1, 250), ran.Next(1, 250));
                //    CacheHelper.SetCache("forwardedIp", forwardedIp, tspan);
                //}
                //else
                //{
                //    forwardedIp = CacheHelper.GetCache("forwardedIp").ToString() ;
                //}
            }
            catch (Exception)
            {
                forwardedIp = "43.226.162.107";
            }

            return forwardedIp;

        }
        #endregion

        private class UnionPayResult
        {
            public string r { get; set; }
            public string m { get; set; }
            public object p { get; set; }
        }

        private class proxyipResult
        {
            public int StatusCode { get; set; }
            public string Result { get; set; }
        }

    }
}
