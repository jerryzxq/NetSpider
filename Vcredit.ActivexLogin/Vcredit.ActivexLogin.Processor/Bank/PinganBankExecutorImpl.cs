using Newtonsoft.Json;
using System;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.Processor.Bank
{
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.PinganBank)]
	public class PinganBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly PinganBankExecutorImpl Instance = new PinganBankExecutorImpl();
		private string _redisEntityPackage;

		public PinganBankExecutorImpl() : base()
		{
			_redisEntityPackage = redisPackage + ":Entity";
		}

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new PinganBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;

			entity.UrlParam = string.Format("?token={0}", entity.Token);
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
				URL = "https://bank.pingan.com.cn/ibp/bank/index.html#home/home/index",
				Method = "get",
				Accept = "application/json, text/javascript, */*; q=0.01",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "bank.pingan.com.cn",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);
			string token = Guid.NewGuid().ToString("N");
			currentCookie = httpResult.Cookie;

			return new Tuple<string, string>(token, "");
		}

		private string GetCaptcha(string token)
		{
			var httpItem = new HttpItem()
			{
				URL = "https://bank.pingan.com.cn/ibp/portal/pc/getVcode2.do?1506493867000",
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://bank.pingan.com.cn/ibp/bank/index.html",
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
			var entityReq = JsonConvert.DeserializeObject<PinganBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
			if (encryptStr.Contains("vCode="))
			{
				encryptStr = encryptStr.Replace("vCode=", "vCode=" + captcha);
			}
			if (encryptStr.Contains("userId="))
			{
				encryptStr = encryptStr.Replace("userId=", "userId=" + entityReq.Account);
			}
			var httpItem = new HttpItem
			{
				URL = "https://bank.pingan.com.cn/ibp/ibp4pc/login.do",
				Method = "POST",
				Postdata = encryptStr,
				Cookie = currentCookie,

				Accept = "application/json, text/javascript, */*; q=0.01",
				ContentType = "application/x-www-form-urlencoded;charset=utf-8",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Referer = "https://bank.pingan.com.cn/ibp/bank/index.html#home/home/index",
				Host = "bank.pingan.com.cn",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			httpItem.Header.Add("x-requested-with", "XMLHttpRequest");
			var httpResult = httpHelper.GetHtml(httpItem);
			
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);

			if (httpResult.Html.Contains("\"errCode\":\"000\""))
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
	
}
