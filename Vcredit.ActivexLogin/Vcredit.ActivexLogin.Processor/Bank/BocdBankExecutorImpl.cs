using Newtonsoft.Json;
using System;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.Common.Ext;

namespace Vcredit.ActivexLogin.Processor.Bank
{
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.BocdBank)]
	public class BocdBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly BocdBankExecutorImpl Instance = new BocdBankExecutorImpl();
		private string _redisEntityPackage;

		public BocdBankExecutorImpl() : base()
		{
			_redisEntityPackage = redisPackage + ":Entity";
		}

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new BocdBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;

			RedisHelper.SetCache(entity.Token, JsonConvert.SerializeObject(entity), _redisEntityPackage);
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

			var data = new VerCodeRes
			{
				Token = tup.Item1,
				VerCodeBase64 = captchaStr
			};
			return data;
		}
		private Tuple<string, string> InitLoginPage()
		{
			var httpItem = new HttpItem()
			{
				URL = "https://ebank.bocd.com.cn/pweb/prelogin.do?_locale=zh_CN&BankId=9999",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			string token = Guid.NewGuid().ToString("N");

			currentCookie = httpResult.Cookie;
			return new Tuple<string, string>(token, "");
		}

		private string GetCaptcha(string token)
		{
			Random r = new Random();
			var httpItem = new HttpItem()
			{
				URL = "https://ebank.bocd.com.cn/pweb/GenTokenImg.do?random=" + r.Next(100),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://ebank.bocd.com.cn/pweb/prelogin.do?_locale=zh_CN&BankId=9999",
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

			return CommonFun.GetVercodeBase64(httpResult.ResultByte);
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
			var entity = JsonConvert.DeserializeObject<BocdBankEncryptEntity>(encryptStr);
			var entityReq = JsonConvert.DeserializeObject<BocdBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));

			var _postData = string.Format("_viewReferer={0}&_locale={1}&BankId={2}&LoginType={3}&MFM={4}&BrowserName={5}&UserAgent={6}&UserId={7}&LoginPassword={8}&LoginMacAddress={9}&_vTokenName={10}&Submit={11}",
			entity._viewReferer.ToUrlEncode(), entity._locale, entity.BankId, entity.LoginType, entity.MFM, entity.BrowserName,
			entity.UserAgent.ToUrlEncode(), entityReq.Account, entity.LoginPassword.ToUrlEncode(), entity.LoginMacAddress.ToUrlEncode(), captcha, entity.Submit);

			currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			var httpItem = new HttpItem
			{
				URL = "https://ebank.bocd.com.cn/pweb/login.do",
				Method = "POST",
				Accept = "*/*",
				Referer = "https://ebank.bocd.com.cn/pweb/prelogin.do?_locale=zh_CN&BankId=9999",
				ContentType= "application/x-www-form-urlencoded",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "ebank.bocd.com.cn",
				KeepAlive = true,
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue=false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			httpItem.Header.Add("Cache-Control", "no-cache");
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			if (doc.Select("div.exitBtn a") != null && doc.Select("div.exitBtn a").Text.Contains("安全退出"))
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
				VerCodeBase64 = captchaStr,
			};
			return data;
		}
	}

	public class BocdBankEncryptEntity
	{
		public string _viewReferer { get; set; }
		public string _locale { get; set; }
		public string BankId { get; set; }
		public string LoginType { get; set; }
		public string MFM { get; set; }
		public string BrowserName { get; set; }
		public string UserAgent { get; set; }
		public string UserId { get; set; }
		public string LoginPassword { get; set; }
		public string LoginMacAddress { get; set; }
		public string Submit { get; set; }

	}

}
