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
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Vcredit.ActivexLogin.Processor.Bank
{
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CommBank)]
	public class CommBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly CommBankExecutorImpl Instance = new CommBankExecutorImpl();
		private string _redisEntityPackage;

		public CommBankExecutorImpl() : base()
		{
			_redisEntityPackage = redisPackage + ":Entity";
		}

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new CommBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;
			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var RandomText = jObj.SelectToken("$..RandomText");
			if (RandomText == null || RandomText.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 RandomText 不能为空");
			entity.RandomText = RandomText.ToString();

			var ReqSafeFields = jObj.SelectToken("$..ReqSafeFields");
			if (ReqSafeFields == null || ReqSafeFields.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 ReqSafeFields 不能为空");
			entity.ReqSafeFields = ReqSafeFields.ToString();

			var PSessionId = jObj.SelectToken("$..PSessionId");
			if (PSessionId == null || PSessionId.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 PSessionId 不能为空");
			entity.PSessionId = PSessionId.ToString();

			entity.UrlParam = string.Format("?RandomText={0}&Token={1}&ReqSafeFields={2}&PSessionId={3}", entity.RandomText, entity.Token, entity.ReqSafeFields, entity.PSessionId);

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
			var captchaStr = GetCaptcha(tup.Item1, tup.Item4);
			var r = new
			{
				RandomText = tup.Item2,
				ReqSafeFields = tup.Item3,
				PSessionId = tup.Item4
			};

			var data = new VerCodeRes
			{
				Token = tup.Item1,
				VerCodeBase64 = captchaStr,
				Result = JsonConvert.SerializeObject(r),
			};
			return data;
		}
		private Tuple<string, string, string, string> InitLoginPage()
		{
			var httpItem = new HttpItem()
			{
				URL = "https://pbank.95559.com.cn/personbank/logon.jsp",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			var randomText = Regex.Match(httpResult.Html, @"(?<=createSafeControl\('password','password',').*(?=','20')").Value;
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			var sReqSafeFields = doc.Select("form[name=loginForm] input[name=ReqSafeFields]").Attr("value");
			var sPSessionId = doc.Select("form[name=loginForm] input[name=PSessionId]").Attr("value");

			string token = Guid.NewGuid().ToString("N");

			currentCookie = httpResult.Cookie;
			return new Tuple<string, string, string, string>(token, randomText, sReqSafeFields, sPSessionId);
		}

		private string GetCaptcha(string token, string sPSessionId)
		{
			Random r = new Random();
			var httpItem = new HttpItem()
			{
				URL = string.Format("https://pbank.95559.com.cn/personbank/system/syCaptchaShow.image?PSessionId={0}&x-channel=0&temp={1}", sPSessionId, r.Next(100)),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://pbank.95559.com.cn/personbank/logon.jsp",
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
			var _postData = encryptStr.Replace("captchaCode=", "captchaCode=" + captcha);

			var httpItem = new HttpItem
			{
				URL = "https://pbank.95559.com.cn/personbank/system/syLogin.do",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://pbank.95559.com.cn/personbank/logon.jsp",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "pbank.95559.com.cn",
				PostEncoding = Encoding.UTF8,
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			if (doc.Select("form[name=syVerifyCustomerNewControlForm]").First != null)
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
			var entity = JsonConvert.DeserializeObject<CommBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
			var captchaStr = GetCaptcha(token, entity.PSessionId);

			var data = new VerCodeRes
			{
				Token = token,
				VerCodeBase64 = captchaStr,
			};
			return data;
		}
	}

}
