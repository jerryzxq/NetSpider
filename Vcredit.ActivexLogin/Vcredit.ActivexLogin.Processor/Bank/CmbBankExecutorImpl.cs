using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.Processor.Bank
{
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CmbBank)]
    public class CmbBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
    {
        public static readonly CmbBankExecutorImpl Instance = new CmbBankExecutorImpl();

        public BaseRes SendOriginalData(ActivexLoginReq req)
        {
            var entity = new CmbBankRequestEntity();
            entity.Token = req.Token;
            entity.Account = req.Account;
            entity.Password = req.Password;

            if (req.AdditionalParam.IsEmpty())
                throw new ArgumentNullException("AdditionalParam 不能为空");

            var jObj = JObject.Parse(req.AdditionalParam);
            var jsObj = jObj.SelectToken("$..GenShell_ClientNo");
            if (jsObj == null || jsObj.ToString().IsEmpty())
                throw new ArgumentException("AdditionalParam 参数中 GenShell_ClientNo 不能为空");
            entity.GenShell_ClientNo = jsObj.ToString();

            RedisHelper.Enqueue(JsonConvert.SerializeObject(entity), redisQueuePackage);
            return new BaseRes { StatusDescription = "处理成功，后台正在处理加密..." };
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public VerCodeRes Init()
        {
            var tup = InitLoginPage();

            var data = new VerCodeRes { Token = tup.Item1, Result = tup.Item2 };
            return data;
        }
        private Tuple<string, string> InitLoginPage()
        {
            var httpItem = new HttpItem()
            {
                URL = "https://pbsz.ebank.cmbchina.com/CmbBank_GenShell/UI/GenShellPC/Login/Login.aspx",
                Method = "get",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            var doc = NSoup.NSoupClient.Parse(httpResult.Html);

            var GenShell_ClientNo = Regex.Match(httpResult.Html, @"(?<=var GenShell_ClientNo=\"").*(?=\"";</script></form>)").Value;
            string token = Guid.NewGuid().ToString("N");

            currentCookie = httpResult.Cookie;
            RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
            return new Tuple<string, string>(token, GenShell_ClientNo);
        }

        public BaseRes DoRealLogin(string token, string captcha)
        {
            var res = new BaseRes() { Token = token };

            var encryptStr = RedisHelper.GetCache<string>(token, redisEncryptPackage);
            if (string.IsNullOrEmpty(encryptStr))
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.NotFoundEncrypt,
                        ReasonDescription = "加密没有完成"
                    });
                return res;
            }
            var entity = JsonConvert.DeserializeObject<CmbBankEncryptEntity>(encryptStr);

            var _postData = string.Format("ClientNo={0}&CreditCardVersion={1}&AccountNo={2}&Password={3}&HardStamp={4}&Licex={5}"
                , entity.ClientNo, entity.CreditCardVersion, entity.AccountNo, entity.Password, entity.HardStamp, entity.Licex);

            currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
            var httpItem = new HttpItem
            {
                URL = "https://pbsz.ebank.cmbchina.com/CmbBank_GenShell/UI/GenShellPC/Login/GenUniLogin.aspx",
                Method = "POST",
                Accept = "*/*",
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://pbsz.ebank.cmbchina.com/CmbBank_GenShell/UI/GenShellPC/Login/Login.aspx",

                PostEncoding = Encoding.UTF8,
                Postdata = _postData,
                Cookie = currentCookie,
            };
            httpItem.Header.Add("Accept-Language", "zh-Hans-CN,zh-Hans;q=0.5");
            httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
            //httpItem.ProxyIp = "127.0.0.1:8888";

            var httpResult = httpHelper.GetHtml(httpItem);
            currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
            Log4netAdapter.WriteInfo(httpResult.Html);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(httpResult.Html);
            XmlNode xmlObj = xmlDoc.SelectSingleNode("LoginResponse").SelectSingleNode("LoginFlag");//查找<bookstore>
            if (xmlObj.InnerText == "Y")
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.Success,
                        ReasonDescription = "登陆成功",
                        Data = httpResult.Html + "  Cookie is ==> " + currentCookie
                    });
            }
            else
            {
                res.Result = JsonConvert.SerializeObject(
                   new EncryptDataResultDto
                   {
                       Reason = EncryptStatus.Faild,
                       ReasonDescription = "登陆失败了",
                       Data = httpResult.Html
                   });
            }

            return res;
        }

        public VerCodeRes RefreshCaptcha(string token)
        {
            throw new NotImplementedException();
        }

        private class CmbBankEncryptEntity
        {
            public string ClientNo { get; set; }
            public string CreditCardVersion { get; set; }
            public string AccountNo { get; set; }
            public string Password { get; set; }
            public string HardStamp { get; set; }
            public string Licex { get; set; }
        }
    }
}
