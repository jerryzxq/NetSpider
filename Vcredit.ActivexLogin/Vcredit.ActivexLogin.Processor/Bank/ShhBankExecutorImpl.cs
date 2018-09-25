using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.ShhBank)]
	public class ShhBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly ShhBankExecutorImpl Instance = new ShhBankExecutorImpl();

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new ShhBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;

			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var TS = jObj.SelectToken("$..TS");
			if (TS == null || TS.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 TS 不能为空");
			entity.TS = TS.ToString();

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
			var captchaStr = GetCaptcha(tup.Item1);
			var r = new
			{
				TS = tup.Item2,
			};
			var data = new VerCodeRes
			{
				Token = tup.Item1,
				VerCodeBase64 = captchaStr,
				Result = JsonConvert.SerializeObject(r),
			};
			return data;
		}
		private Tuple<string, string> InitLoginPage()
		{
			var httpItem = new HttpItem()
			{
				URL = "https://ebanks.bankofshanghai.com/pweb/prelogin.do?LoginType=R&_locale=zh_CN",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				UserAgent= "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host= "ebanks.bankofshanghai.com",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);
			var ts = Regex.Match(httpResult.Html, @"(?<=var ts = "").*(?="")").Value;
			string token = Guid.NewGuid().ToString("N");
			currentCookie = httpResult.Cookie;

			return new Tuple<string, string>(token, ts);
		}

		private string GetCaptcha(string token)
		{
			Random rd = new Random();
			var httpItem = new HttpItem()
			{
				URL = "https://ebanks.bankofshanghai.com/pweb/GenTokenImg.do?random="+rd.NextDouble(),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,
				UserAgent= "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://ebanks.bankofshanghai.com/pweb/prelogin.do?LoginType=R&_locale=zh_CN",
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);

			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

			return CommonFun.GetVercodeBase64(httpResult.ResultByte);
		}

		public BaseRes DoRealLogin(string token, string captcha)
		{
			var res = new BaseRes() { Token = token };
			currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
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
			var entity = JsonConvert.DeserializeObject<ShhBankEncryptEntity>(encryptStr);

			var _postData = string.Format("_locale={0}&LoginType={1}&PreLoginMode={2}&Password={3}&_lang={4}&LoginId={5}&PasswordLogin3={6}&MachineCode={7}&_vTokenName={8}",
				entity._locale, entity.LoginType, entity.PreLoginMode, entity.Password.ToUrlEncode(), entity._lang, entity.LoginId,
				entity.PasswordLogin3, entity.MachineCode.ToUrlEncode(), captcha);

			var httpItem = new HttpItem
			{
				URL = "https://ebanks.bankofshanghai.com/pweb/login.do",
				Method = "POST",
				Accept = "*/*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://ebanks.bankofshanghai.com/pweb/prelogin.do?LoginType=R&_locale=zh_CN",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "ebanks.bankofshanghai.com",
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue=false
			};
			httpItem.Header.Add("pe-ajax", "true");
			httpItem.Header.Add("pe-encoding", "UTF-8");
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			if (doc.Select(".headtitle").Text== "个人网银登录验证")
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
			var captchaStr = GetCaptcha(token);

			var data = new VerCodeRes
			{
				Token = token,
				VerCodeBase64 = captchaStr
			};
			return data;
		}
	}

	public class ShhBankEncryptEntity
	{
		public string _locale { get; set; }
		public string LoginType { get; set; }
		public string PreLoginMode { get; set; }
		public string Password { get; set; }
		public string _lang { get; set; }
		public string LoginId { get; set; }
		public string PasswordLogin3 { get; set; }
		public string MachineCode { get; set; }
	}

}
