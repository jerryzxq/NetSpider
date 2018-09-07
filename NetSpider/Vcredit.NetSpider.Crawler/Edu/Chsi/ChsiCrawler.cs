using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.Common.Constants;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Vcredit.NetSpider.DataAccess.Ftp;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Crawler.Edu.Chsi
{
    public class ChsiCrawler : IChsiCrawler
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
        /// <summary>
        /// 学信数据查询初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Query_Init()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {

                Url = "https://account.chsi.com.cn/passport/login";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                //results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@id='captcha']", "value");
                var results1 = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='lt']", "value");
                //if (results.Count > 0)
                {
                    Url = "https://account.chsi.com.cn/passport/captcha.image";
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
                    Res.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);
                    //Res.VerCode = secParser.GetVerCodeByCharSort(httpResult.ResultByte, CharSort.NumberAndLower);
                    //保存验证码图片在本地
                    //FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                    //Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                }


                Res.StatusDescription = "学信网初始化完成";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("lt", results1[0]);
                CacheHelper.SetCache(token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网初始化异常";
                Log4netAdapter.WriteError("学信网初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 学信数据查询
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public BaseRes Query_GetInfo(LoginReq login)
        {

            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            string lt = string.Empty;
            string filename = string.Empty;
            List<string> results = new List<string>();
            List<string> results1 = new List<string>();
            List<Chsi_InfoRes> Infos = new List<Chsi_InfoRes>();

            try
            {
                //获取缓存
                if (CacheHelper.GetCache(login.Token) != null)
                {
                    Dictionary<string, object> dics = (Dictionary<string, object>)CacheHelper.GetCache(login.Token);
                    lt = dics["lt"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(login.Token);
                }

                if (login.Username.IsEmpty() || login.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                #region 第一步，登录
                Url = "https://account.chsi.com.cn/passport/login?service=http%3A%2F%2Fmy.chsi.com.cn%2Farchive%2Fj_spring_cas_security_check";
                postdata = String.Format("username={0}&password={1}&captcha={2}&lt={3}&_eventId=submit&submit=%E7%99%BB%C2%A0%C2%A0%E5%BD%95", login.Username, login.Password.ToUrlEncode(), login.Vercode, lt);
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
                    results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='errors']", "text");
                    if (results.Count == 0)
                    {
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='ct_input errors']", "text");
                    }
                    if (results.Count > 0)
                    {
                        Res.StatusDescription = results[0];
                    }
                    else
                    {
                        Res.StatusDescription = "登录失败";
                    }
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                #endregion

                #region 第二步，查询信息

                //学籍信息
                Url = "https://my.chsi.com.cn/archive/gdjy/xj/show.action";//20161227网站改版更新
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
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

                results1 = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='main']/div[@class='clearfix']");
                string dtid = string.Empty;
                string strHtml = httpResult.Html;
                if (results1.Count == 0)
                {
                    Res.StatusDescription = "无数据";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                else
                {
                    foreach (string item in results1)
                    {
                        var tbStr = HtmlParser.GetResultFromParser(item, "//div[@class='xj-m-r']/table[@class='mb-table']");

                        dtid = CommonFun.GetMidStr(item, "&dtid=", "\"");
                        Chsi_InfoRes infoRes = new Chsi_InfoRes();
                        results = HtmlParser.GetResultFromParser(item, "//tr[1]/td[1]", "text");

                        infoRes.Name = results[0];//姓名

                        results = HtmlParser.GetResultFromParser(item, "//tr[1]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.Sex = results[0];//性别
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[2]/td[1]", "text");
                        if (results.Count > 0)
                        {
                            var birthdate = results[0].ToDateTime(Consts.DateFormatString6);
                            if (birthdate != null)
                            {
                                infoRes.BirthDate = birthdate.ToString();//出生日期
                            }
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[2]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.Race = results[0];//民族
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[5]/td[1]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.Schoolinglength = results[0];//学制
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[6]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.College = results[0];//分院
                        }
                        results = HtmlParser.GetResultFromParser(item, "//tr[7]/td[1]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.Department = results[0];//系
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[7]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.Class = results[0];//班级
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[8]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            var EnrollmentDate = results[0].ToDateTime(Consts.DateFormatString6);
                            if (EnrollmentDate != null)
                            {
                                infoRes.EnrollmentDate = EnrollmentDate.ToString();//入学日期
                            }
                        }
                        results = HtmlParser.GetResultFromParser(item, "//tr[9]/td[1]", "text");
                        if (results.Count > 0)
                        {
                            var LeavingDate = CommonFun.ClearFlag(results[0]).ToDateTime(Consts.DateFormatString6);//离校日期
                            if (LeavingDate != null)
                            {
                                infoRes.LeavingDate = LeavingDate.ToString();//离校日期
                            }
                        }

                        results = HtmlParser.GetResultFromParser(item, "//tr[9]/td[2]", "text");
                        if (results.Count > 0)
                        {
                            infoRes.SchoolState = results[0];//学籍状态
                        }

                        infoRes.University = CommonFun.GetMidStr(item, dtid + "\"+\"-m1\", \"", "\");");//身份证号
                        infoRes.MajorName = CommonFun.GetMidStr(item, dtid + "\"+\"-m2\", \"", "\");");//专业
                        //infoRes.ExamineeNo = CommonFun.GetMidStr(strHtml, dtid + "\"+\"-m3\", \"", "\");");//考生号
                        infoRes.StudentNo = CommonFun.GetMidStr(item, dtid + "\"+\"-m4\", \"", "\");");//学号
                        infoRes.Degree = CommonFun.GetMidStr(item, dtid + "\"+\"-m5\", \"", "\");");//层次
                        infoRes.EducationType = CommonFun.GetMidStr(item, dtid + "\"+\"-m6\", \"", "\");");//学历类别
                        infoRes.LearningMode = CommonFun.GetMidStr(item, dtid + "\"+\"-m7\", \"", "\");");//学习形式
                        infoRes.IdentityCard = CommonFun.GetMidStr(item, dtid + "\"+\"-m8\", \"", "\");");//身份证号
                        //毕业照片
                        results = HtmlParser.GetResultFromParser(item, "//img[@alt='学历照片']", "src");
                        if (results.Count > 0 && !results[0].Contains("no-photo"))
                        {
                            httpItem = new HttpItem()
                            {
                                URL = "http://my.chsi.com.cn/archive" + results[0],
                                Method = "GET",
                                ResultType = ResultType.Byte,
                                CookieCollection = cookies,
                                ResultCookieType = ResultCookieType.CookieCollection
                            };
                            httpResult = httpHelper.GetHtml(httpItem);
                            if (httpResult.StatusCode == HttpStatusCode.OK && httpResult.ResultByte != null)
                            {
                                try
                                {
                                    filename = infoRes.IdentityCard + "_" + infoRes.University + "_by.jpg";
                                    EduChsiFTP ftp = new EduChsiFTP();
                                    ftp.UploadChsiPhoto(httpResult.ResultByte, filename);

                                    infoRes.GraduatePhoto = filename;
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        //录取照片
                        results = HtmlParser.GetResultFromParser(item, "//img[@alt='录取照片']", "src");
                        if (results.Count > 0 && !results[0].Contains("no-photo"))
                        {
                            httpItem = new HttpItem()
                            {
                                URL = "http://my.chsi.com.cn/archive" + results[0],
                                Method = "GET",
                                ResultType = ResultType.Byte,
                                CookieCollection = cookies,
                                ResultCookieType = ResultCookieType.CookieCollection
                            };
                            httpResult = httpHelper.GetHtml(httpItem);
                            if (httpResult.StatusCode == HttpStatusCode.OK && httpResult.ResultByte != null)
                            {
                                try
                                {
                                    filename = infoRes.IdentityCard + "_" + infoRes.University + "_lq.jpg";
                                    EduChsiFTP ftp = new EduChsiFTP();
                                    ftp.UploadChsiPhoto(httpResult.ResultByte, filename);

                                    infoRes.EnrollPhoto = filename;
                                }
                                catch (Exception)
                                { }
                            }

                        }
                        Infos.Add(infoRes);
                    }
                }

                //学历信息
                Url = "https://my.chsi.com.cn/archive/gdjy/xl/show.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                strHtml = httpResult.Html;
                int count = results1.Count;
                results1 = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='main']/div[@class='clearfix']");
                for (int i = 0; i < count; i++)
                {
                    var tbStr = HtmlParser.GetResultFromParser(results1[i], "//div[@class='xj-m-r']/table[@class='mb-table']");

                    dtid = CommonFun.GetMidStr(results1[i], "&dtid=", "\"");
                    if(tbStr.Count>0)
                    {
                        results = HtmlParser.GetResultFromParser(tbStr[0], "//tr[5]/td[1]", "text");
                        if (results.Count > 0)
                        {
                            Infos[i].UniversityLocation = results[0].ToTrim();//院校所在地
                        }

                        results = HtmlParser.GetResultFromParser(results1[i], "//tr[7]/td[1]", "text");
                        if (results.Count > 0)
                        {
                            Infos[i].GraduateState = results[0];//毕结业结论
                        }
                        Infos[i].CertificateNo = CommonFun.GetMidStr(strHtml, dtid + "\"+\"-s5\", \"", "\");").ToTrim();//证书编号
                    }
                }

                //获取手机号
                Url = "https://account.chsi.com.cn/account/account!show";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "GET",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "/html/body/div[2]/div[1]/div[2]/div[1]/div/div/div[11]/div[2]", "text", true);
                if (results.Count > 0)
                {
                    string phone = results[0].ToTrim("&nbsp;&nbsp;&nbsp;(仅绑定大陆地区手机的用户可进行图像校对、学籍/学历核验)");
                    foreach (var item in Infos)
                    {
                        item.Phone = phone;
                    }
                }

                #endregion
                Res.Result = jsonParser.SerializeObject(Infos);
                Res.StatusDescription = "学信网查询结束";
                Res.StatusCode = ServiceConsts.StatusCode_success;

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网查询出错";
                Log4netAdapter.WriteError("学信网查询出错", e);
            }
            return Res;
        }
        /// <summary>
        /// 学信注册，第一步，初始化(参数：mobile)
        /// </summary>
        /// <param name="registerReq"></param>
        /// <returns></returns>
        public VerCodeRes Register_Init(ChsiRegisterReq registerReq)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            if (registerReq.Token.IsEmpty())
            {
                Res.Token = token;
            }
            if (string.IsNullOrEmpty(registerReq.Mobile))
            {
                Res.StatusDescription = "手机号码不能为空";
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            try
            {

                Url = "https://account.chsi.com.cn/account/preregister.action?from=chsi-home";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://account.chsi.com.cn/account/checkmobilephoneother.action";
                postdata = string.Format("mphone={0}&dataInfo={0}&optType=REGISTER", registerReq.Mobile);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "https://account.chsi.com.cn/account/preregister.action?from=chsi-home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.Result = CommonFun.ClearFlag(httpResult.Html);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                if (Res.Result == "true")
                {
                    Url = "https://account.chsi.com.cn/account/captchimagecreateaction.action";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "get",
                        ResultType = ResultType.Byte,
                        Referer = "https://account.chsi.com.cn/account/preregister.action?from=chsi-home",
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
                    Res.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);
                    //保存验证码图片在本地
                    FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                    Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                    Res.StatusDescription = "学信网注册初始化完成";
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Res.StatusDescription = "手机号已存在";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网注册初始化异常";
                Log4netAdapter.WriteError("学信网注册初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 学信注册，第二步，发短信验证码(参数：mobile,Vercode,ignoremphone)
        /// </summary>
        /// <param name="registerReq"></param>
        /// <returns></returns>
        public BaseRes Register_Step1(ChsiRegisterReq registerReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = registerReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(registerReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(registerReq.Token);
                    //CacheHelper.RemoveCache(registerReq.Token);
                }

                if (registerReq.Mobile.IsEmpty())
                {
                    Res.StatusDescription = "手机号不能为空";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string ignoremphone = string.Empty;
                if (registerReq.ignoremphone.IsEmpty())
                {
                    ignoremphone = "false";
                }
                else
                {
                    ignoremphone = registerReq.ignoremphone;
                }

                Url = "https://account.chsi.com.cn/account/getmphonpincode.action";
                postdata = string.Format("captch={0}&mobilePhone={1}&optType=REGISTER&ignoremphone={2}", registerReq.Vercode, registerReq.Mobile, ignoremphone);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "https://account.chsi.com.cn/account/preregister.action?from=chsi-home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string status = jsonParser.GetResultFromParser(httpResult.Html, "status");
                string tips = jsonParser.GetResultFromParser(httpResult.Html, "tips");

                Res.StatusDescription = tips;
                if (status == "2")
                {
                    Res.Result = status;
                    Res.StatusCode = ServiceConsts.StatusCode_success;
                }
                else
                {
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                }

                CacheHelper.SetCache(registerReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网注册初始化异常";
                Log4netAdapter.WriteError("学信网注册初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 学信注册，第三步，提交(参数：mobile,ignoremphone,Smscode,password,password1,name,credentialtype,Identitycard,email,pwdreq1,pwdanswer1,pwdreq2,pwdanswer2,pwdreq3,pwdanswer3)
        /// </summary>
        /// <param name="registerReq"></param>
        /// <returns></returns>
        public BaseRes Register_Step2(ChsiRegisterReq registerReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = registerReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(registerReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(registerReq.Token);
                    CacheHelper.RemoveCache(registerReq.Token);
                }

                if (registerReq.Email.IsEmpty() || registerReq.Name.IsEmpty() || registerReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = "缺少必填项";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://account.chsi.com.cn/account/checkusername.action";
                postdata = string.Format("dataInfo={0}&optType=REGISTER", registerReq.Email);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "https://account.chsi.com.cn/account/preregister.action?from=chsi-home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                string status = jsonParser.GetResultFromParser(httpResult.Html, "status");
                string info = jsonParser.GetResultFromParser(httpResult.Html, "info");

                if (status != "1")
                {
                    Res.StatusDescription = info;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                string ignoremphone = string.Empty;//手机号
                if (registerReq.ignoremphone.IsEmpty())
                {
                    ignoremphone = "false";
                }
                else
                {
                    ignoremphone = registerReq.ignoremphone;
                }

                Url = "https://account.chsi.com.cn/account/registerprocess.action";
                postdata = string.Format("from=chsi-home&mphone={0}&ignoremphone={1}&vcode={2}&password={3}&password1={4}&xm={5}&credentialtype={6}&sfzh={7}&email={8}&pwdreq1={9}&pwdanswer1={10}&pwdreq2={11}&pwdanswer2={12}&pwdreq3={13}&pwdanswer3={14}&continueurl=&serviceId=&serviceNote=1&serviceNote_res=0"
                    , registerReq.Mobile, ignoremphone, registerReq.Smscode, registerReq.Password.ToUrlEncode(), registerReq.Password1.ToUrlEncode(), registerReq.Name.ToUrlEncode(), registerReq.Credentialtype, registerReq.Identitycard, registerReq.Email.ToUrlEncode(), registerReq.Pwdreq1, registerReq.Pwdanswer1.ToUrlEncode(), registerReq.Pwdreq2, registerReq.Pwdanswer2.ToUrlEncode(), registerReq.Pwdreq3, registerReq.Pwdanswer3.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    Referer = "https://account.chsi.com.cn/account/preregister.action?from=chsi-home",
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@class='errorMessage']/li/span");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Res.StatusDescription = "学信网注册成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //CacheHelper.SetCache(registerReq.Token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网注册初始化异常";
                Log4netAdapter.WriteError("学信网注册初始化异常", e);
            }
            return Res;
        }
        /// <summary>
        /// 找回用户名
        /// </summary>
        /// <param name="forgetReq">Identitycard-身份证号，name-姓名</param>
        /// <returns></returns>
        public BaseRes ForgetUsername(ChsiForgetReq forgetReq)
        {
            BaseRes Res = new BaseRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {

                if (forgetReq.Identitycard.IsEmpty() || forgetReq.Name.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://account.chsi.com.cn/account/password!rtvlgname";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://account.chsi.com.cn/account/password!rtvlgname.action";
                postdata = string.Format("sfzh={0}&xm={1}", forgetReq.Identitycard, forgetReq.Name.ToUrlEncode());
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@id='user_retrivelgname_fm_error_info']/li/span", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@class='retriveName']/table/tr/td[1]", "text");
                Res.Result = results[0];
                Res.StatusDescription = "学信网找回用户名成功";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网找回用户名出错";
                Log4netAdapter.WriteError("学信网找回用户名出错", e);
            }
            return Res;
        }
        /// <summary>
        /// 找回密码,第一步，页面初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes ForgetPwd_Step1()
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                Url = "https://account.chsi.com.cn/account/password!retrive";
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
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = "https://account.chsi.com.cn/account/captchimagecreateaction.action";
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
                Res.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);

                Res.StatusDescription = "学信网找回密码第一步结束";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                CacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网找回密码第一步出错";
                Log4netAdapter.WriteError("学信网找回密码第一步出错", e);
            }
            return Res;
        }

        /// <summary>
        /// 找回密码,第二步，参数：用户名，验证码
        /// </summary>
        /// <param name="forgetReq"></param>
        /// <returns></returns>
        public VerCodeRes ForgetPwd_Step2(ChsiForgetReq forgetReq)
        {
            VerCodeRes Res = new VerCodeRes();
            Res.Token = forgetReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(forgetReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(forgetReq.Token);
                    CacheHelper.RemoveCache(forgetReq.Token);
                }
                if (forgetReq.Username.IsEmpty() || forgetReq.Vercode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://account.chsi.com.cn/account/password!retrive.action";
                postdata = string.Format("loginName={0}&captch={1}", forgetReq.Username, forgetReq.Vercode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@id='user_retrivePsd_form_error_info']/li/span", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='ctoken']", "value");

                string ctoken = results[0];

                Url = "https://account.chsi.com.cn/account/forgot/rtvbymphoneindex.action";
                postdata = string.Format("ctoken={0}", ctoken);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);

                Url = "https://account.chsi.com.cn/account/captchimagecreateaction.action";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                Res.VerCodeBase64 = Convert.ToBase64String(httpResult.ResultByte);
                FileOperateHelper.WriteVerCodeImage(forgetReq.Token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(forgetReq.Token);

                Res.StatusDescription = "学信网找回密码第二步结束";
                Res.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
                Dictionary<string, object> dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("ctoken", ctoken);
                CacheHelper.SetCache(forgetReq.Token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网找回密码第二步出错";
                Log4netAdapter.WriteError("学信网找回密码第二步出错", e);
            }
            return Res;
        }
        /// <summary>
        /// 找回密码,第三步，参数：姓名，验证码,手机号，身份证号
        /// </summary>
        /// <param name="forgetReq"></param>
        /// <returns></returns>
        public BaseRes ForgetPwd_Step3(ChsiForgetReq forgetReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = forgetReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            string ctoken = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, object> dics = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(forgetReq.Token) != null)
                {
                    dics = (Dictionary<string, object>)CacheHelper.GetCache(forgetReq.Token);
                    ctoken = dics["ctoken"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(forgetReq.Token);
                }

                if (forgetReq.Name.IsEmpty() || forgetReq.Mobile.IsEmpty() || forgetReq.Vercode.IsEmpty() || forgetReq.Identitycard.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://account.chsi.com.cn/account/forgot/rtvbymphone.action";
                postdata = string.Format("captch={0}&mphone={1}&ctoken={2}&xm={3}&sfzh={4}", forgetReq.Vercode, forgetReq.Mobile, ctoken, forgetReq.Name.ToUrlEncode(), forgetReq.Identitycard);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@id='user_retrivePsd_form_error_info']/li/span", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@id='error_info']/li/span", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@id='user_reg_fm_error_info']/li/span", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                results = HtmlParser.GetResultFromParser(httpResult.Html, "//input[@name='clst']", "value");

                Res.StatusDescription = "学信网找回密码第三步结束";
                Res.StatusCode = ServiceConsts.StatusCode_success;

                //添加缓存
                dics = new Dictionary<string, object>();
                dics.Add("cookie", cookies);
                dics.Add("clst", results[0]);
                CacheHelper.SetCache(forgetReq.Token, dics);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网找回密码第三步出错";
                Log4netAdapter.WriteError("学信网找回密码第三步出错", e);
            }
            return Res;
        }
        /// <summary>
        /// 找回密码,第四步，参数：密码，确认密码,短信验证码
        /// </summary>
        /// <param name="forgetReq"></param>
        /// <returns></returns>
        public BaseRes ForgetPwd_Step4(ChsiForgetReq forgetReq)
        {
            BaseRes Res = new BaseRes();
            Res.Token = forgetReq.Token;
            string Url = string.Empty;
            string postdata = string.Empty;
            string clst = string.Empty;
            List<string> results = new List<string>();
            Dictionary<string, object> dics = null;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(forgetReq.Token) != null)
                {
                    dics = (Dictionary<string, object>)CacheHelper.GetCache(forgetReq.Token);
                    clst = dics["clst"].ToString();
                    cookies = (CookieCollection)dics["cookie"];
                    CacheHelper.RemoveCache(forgetReq.Token);
                }

                if (forgetReq.Password.IsEmpty() || forgetReq.Password1.IsEmpty() || forgetReq.Smscode.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.RequiredEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                Url = "https://account.chsi.com.cn/account/forgot/rstpwdbymphone.action";
                postdata = string.Format("clst={0}&password={1}&key=&password1={2}&vcode={3}", clst, forgetReq.Password, forgetReq.Password1, forgetReq.Smscode);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "post",
                    Postdata = postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//ul[@id='error_info']/li/span", "text");
                if (results.Count > 0)
                {
                    Res.StatusDescription = results[0];
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }

                Res.StatusDescription = "您的密码已被重新设置";
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "学信网找回密码第四步出错";
                Log4netAdapter.WriteError("学信网找回密码第四步出错", e);
            }
            return Res;
        }


        /// <summary>
        /// 聚信立学历查询
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public Chsi_InfoEntity Query_GetJxlInfo(string infos)
        {
            string filename = string.Empty;
            string status = string.Empty;
            string result = string.Empty;
            Chsi_InfoEntity infoRes = new Chsi_InfoEntity();
            try
            {
                #region 聚信立接口返回学历信息
                result = jsonParser.GetResultFromParser(infos, "eduInfos").TrimStart('[').TrimEnd(']');
                if (result.IsEmpty())
                {
                    //Res.StatusDescription = "身份证号码长度或校验位不正确。";
                    //Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return infoRes;
                }
                status = jsonParser.GetResultFromParser(result, "message");
                if (!status.IsEmpty())
                {
                    status = jsonParser.GetResultFromParser(status, "status");
                }
                if (status != "0")
                {
                    //Res.StatusDescription = "未查到数据。";
                    //Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return infoRes;
                }
                var name = jsonParser.GetResultFromParser(result, "userName");
                if (!name.IsEmpty())
                {
                    infoRes.Name = jsonParser.GetResultFromParser(name, "#text");  //姓名
                }

                var identityCard = jsonParser.GetResultFromParser(result, "identityCard");
                if (!identityCard.IsEmpty())
                {
                    infoRes.IdentityCard = jsonParser.GetResultFromParser(identityCard, "#text");  //身份证号
                }

                var graduate = jsonParser.GetResultFromParser(result, "graduate");
                if (!graduate.IsEmpty())
                {
                    infoRes.University = jsonParser.GetResultFromParser(graduate, "#text");  //毕业院校
                }

                var educationDegree = jsonParser.GetResultFromParser(result, "educationDegree");
                if (!educationDegree.IsEmpty())
                {
                    infoRes.Degree = jsonParser.GetResultFromParser(educationDegree, "#text");  //学历
                }

                var enrolDate = jsonParser.GetResultFromParser(result, "enrolDate");
                if (!enrolDate.IsEmpty())
                {
                    var t = jsonParser.GetResultFromParser(enrolDate, "#text") + "-09-01";
                    infoRes.EnrollmentDate = DateTime.Parse(t);   //入学年份
                }

                var specialityName = jsonParser.GetResultFromParser(result, "specialityName");
                if (!specialityName.IsEmpty())
                {
                    infoRes.MajorName = jsonParser.GetResultFromParser(specialityName, "#text");  //专业
                }

                var graduateTime = jsonParser.GetResultFromParser(result, "graduateTime");
                if (!graduateTime.IsEmpty())
                {
                    var t = jsonParser.GetResultFromParser(graduateTime, "#text") + "-09-01";
                    infoRes.LeavingDate = DateTime.Parse(t);  //毕业时间
                }

                var studyResult = jsonParser.GetResultFromParser(result, "studyResult");
                if (!studyResult.IsEmpty())
                {
                    infoRes.GraduateState = jsonParser.GetResultFromParser(studyResult, "#text");  //毕业结论
                }

                var studyStyle = jsonParser.GetResultFromParser(result, "studyStyle");
                if (!studyStyle.IsEmpty())
                {
                    infoRes.EducationType = jsonParser.GetResultFromParser(studyStyle, "#text");  //学历类型
                }
                var no = jsonParser.GetResultFromParser(result, "no");
                if (!no.IsEmpty())
                {
                    infoRes.Token = jsonParser.GetResultFromParser(no, "#text");  //唯一标识
                }

                var photo = jsonParser.GetResultFromParser(result, "photo");
                if (!photo.IsEmpty())
                {
                    var photoStr = jsonParser.GetResultFromParser(photo, "#text");  //照片
                    //base64转码成图片
                    var photobyte = Convert.FromBase64String(photoStr);
                    filename = infoRes.IdentityCard + "_" + infoRes.University + "_by.jpg";
                    EduChsiFTP ftp = new EduChsiFTP();
                    ftp.UploadChsiPhoto(photobyte, filename);
                    infoRes.GraduatePhoto = filename;
                }
                #endregion


            }
            catch (Exception e)
            {
                //Res.StatusCode = ServiceConsts.StatusCode_error;
                //Res.StatusDescription = "学信网查询出错";
                //Log4netAdapter.WriteError("学信网查询出错", e);
            }
            return infoRes;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
