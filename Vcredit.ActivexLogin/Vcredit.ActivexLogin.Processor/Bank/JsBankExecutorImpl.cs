using Newtonsoft.Json;
using System;
using System.Text;
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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.JsBank)]
	public class JsBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly JsBankExecutorImpl Instance = new JsBankExecutorImpl();

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new JsBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;

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
				URL = "https://ebank.jsbchina.cn/newperbank/",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);

			var token = Guid.NewGuid().ToString("N");
			currentCookie = httpResult.Cookie;
			return new Tuple<string, string>(token, "");
		}

		private string GetCaptcha(string token)
		{
			var httpItem = new HttpItem()
			{
				URL = "https://ebank.jsbchina.cn/newperbank/VerifyImage",
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://ebank.jsbchina.cn/newperbank/",
			};
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
			var entity = JsonConvert.DeserializeObject<JsBankEncryptEntity>(encryptStr);

			var _postData = string.Format("logonId={0}&password={1}&channel={2}&currentBusinessCode={3}&userName={4}&checkCode={5}&EMP_SID={6}&responseFormat={7}",
				entity.logonId, entity.password.ToUrlEncode(), entity.channel, entity.currentBusinessCode, entity.userName,
				captcha, entity.EMP_SID, entity.responseFormat);

			var httpItem = new HttpItem
			{
				URL = "https://ebank.jsbchina.cn/newperbank/signIn.do",
				Method = "POST",
				Accept = "application/json, text/javascript, */*; q=0.01",
				ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
				Referer = "https://ebank.jsbchina.cn/newperbank/",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "ebank.jsbchina.cn",
				PostEncoding = Encoding.UTF8,
				Postdata = _postData,
				Cookie = currentCookie,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);

			if (httpResult.Html.Contains("成功"))
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
			currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			var captchaStr = GetCaptcha(token);

			var data = new VerCodeRes
			{
				Token = token,
				VerCodeBase64 = captchaStr
			};
			return data;
		}
	}

	public class JsBankEncryptEntity
	{
		public string logonId { get; set; }
		public string password { get; set; }
		public string channel { get; set; }
		public string currentBusinessCode { get; set; }
		public string userName { get; set; }
		public string EMP_SID { get; set; }
		public string responseFormat { get; set; }

	}

}
