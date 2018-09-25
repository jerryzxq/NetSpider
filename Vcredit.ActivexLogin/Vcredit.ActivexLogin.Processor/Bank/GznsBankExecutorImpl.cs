using Newtonsoft.Json;
using System;
using System.Text;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.Common.Ext;
using Newtonsoft.Json.Linq;

namespace Vcredit.ActivexLogin.Processor.Bank
{
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.GznsBank)]
	public class GznsBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly GznsBankExecutorImpl Instance = new GznsBankExecutorImpl();

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new GznsBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;
			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var CheckCode = jObj.SelectToken("$..CheckCode");
			if (CheckCode == null || CheckCode.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 CheckCode 不能为空");
			entity.CheckCode = CheckCode.ToString();

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
				URL = "https://ebank.grcbank.com/perbank/",
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
				URL = "https://ebank.grcbank.com/perbank/VerifyImage?update=" + r.Next(100),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://ebank.grcbank.com/perbank/",
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
			var entity = JsonConvert.DeserializeObject<GznsBankEncryptEntity>(encryptStr);
			var _postData = string.Format("logonType={0}&customerAlias={1}&password={2}&logonLanguage={3}&customerType={4}&BankNo={5}&bankName={6}&checkCode={7}&mobileCheckCode={8}&login_flag={9}&pwdType={10}&macJson={11}",
				entity.logonType, entity.customerAlias, entity.password.ToUrlEncode(), entity.logonLanguage, entity.customerType,
				entity.BankNo, entity.bankName.ToUrlEncode(), captcha, entity.mobileCheckCode, entity.login_flag, entity.pwdType, entity.macJson.ToUrlEncode());

			currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			var httpItem = new HttpItem
			{
				URL = "https://ebank.grcbank.com/perbank/signLogin.do",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://ebank.grcbank.com/perbank/",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "ebank.grcbank.com",
				PostEncoding = Encoding.UTF8,
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue = false
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			if (doc.Select("form").First != null && doc.Select("form").Attr("action") == "getMenuInfo.do")
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

	public class GznsBankEncryptEntity
	{
		public string logonType { get; set; }
		public string customerAlias { get; set; }
		public string password { get; set; }
		public string logonLanguage { get; set; }
		public string customerType { get; set; }
		public string BankNo { get; set; }
		public string bankName { get; set; }
		public string mobileCheckCode { get; set; }
		public string login_flag { get; set; }
		public string pwdType { get; set; }
		public string macJson { get; set; }

	}

}
