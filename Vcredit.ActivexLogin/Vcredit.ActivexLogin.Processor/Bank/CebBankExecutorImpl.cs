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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CebBank)]
	public class CebBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly CebBankExecutorImpl Instance = new CebBankExecutorImpl();
		public string _tokenName;

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new CebBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;
			entity.UrlParam = req.AdditionalParam;

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
			var captchaStr = GetCaptcha(tup.Item1, tup.Item2);

			var data = new VerCodeRes
			{
				Token = tup.Item1,
				Result = tup.Item2,
				VerCodeBase64 = captchaStr
			};
			return data;
		}
		private Tuple<string, string> InitLoginPage()
		{
			var httpItem = new HttpItem()
			{
				URL = "https://www.cebbank.com/per/prePerlogin.do?_locale=zh_CN",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			var ran = doc.Select("form [name=ran]").Attr("value");
			_tokenName = doc.Select("form [name=_tokenName]").Attr("value");
			string token = Guid.NewGuid().ToString("N");

			currentCookie = httpResult.Cookie;
			return new Tuple<string, string>(token, ran);
		}

		private string GetCaptcha(string token,string ran)
		{
			var httpItem = new HttpItem()
			{
				URL = "https://www.cebbank.com/per/tokenImage.xx?_timesShowToken=2&ran="+ran,
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://www.cebbank.com/per/prePerlogin.do?_locale=zh_CN",
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			//RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

			return CommonFun.GetVercodeBase64(httpResult.ResultByte);
		}

		public BaseRes DoRealLogin(string token, string captcha)
		{
			var res = new BaseRes() { Token = token };
			//currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);

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
			var entity = JsonConvert.DeserializeObject<CebBankEncryptEntity>(encryptStr);

			var _postData = string.Format("_viewReferer={0}&_locale={1}&ran={2}&TransName={3}&Plain={4}&Signature={5}&MerName={6}&TransType={7}&OperationNo={8}&MerDCFlag={9}&checkloginflag={10}&version={11}&_tokenName={12}&LoginName={13}&Password={14}&Password_RC={15}",
			entity._viewReferer.ToUrlEncode(), entity._locale, entity.ran, entity.TransName, entity.Plain, entity.Signature,
			entity.MerName, entity.TransType, entity.OperationNo, entity.MerDCFlag, entity.checkloginflag, entity.version,
			_tokenName, entity.LoginName, entity.Password.ToUrlEncode(), entity.Password_RC.ToUrlEncode());

			var httpItem = new HttpItem
			{
				URL = "https://www.cebbank.com/per/perlogin1.do",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				Referer = "https://www.cebbank.com/per/prePerlogin.do?_locale=zh_CN",
				ContentType= "application/x-www-form-urlencoded",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "www.cebbank.com",
				KeepAlive = true,
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue=false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			httpItem.Header.Add("Cache-Control", "no-cache");
			httpItem.ProxyIp = "127.0.0.1:8888";
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			if (doc.Select("div.sund a") != null && (doc.Select("div.sund a")).Text.Contains("网银设置"))
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
			var captchaStr = GetCaptcha(token,"");

			var data = new VerCodeRes
			{
				Token = token,
				VerCodeBase64 = captchaStr,
			};
			return data;
		}
	}

	public class CebBankEncryptEntity
	{
		public string _viewReferer { get; set; }
		public string _locale { get; set; }
		public string ran { get; set; }
		public string TransName { get; set; }
		public string Plain { get; set; }
		public string Signature { get; set; }
		public string MerName { get; set; }
		public string TransType { get; set; }
		public string OperationNo { get; set; }
		public string MerDCFlag { get; set; }
		public string checkloginflag { get; set; }
		public string version { get; set; }
		public string _tokenName { get; set; }
		public string LoginName { get; set; }
		public string Password { get; set; }
		public string Password_RC { get; set; }

	}

}
