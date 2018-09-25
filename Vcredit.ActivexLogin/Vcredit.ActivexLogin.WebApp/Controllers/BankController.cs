using System;
using System.Text;
using System.Web.Mvc;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.WebApp.Controllers
{
	public class BankController : Controller
	{
		private HttpHelper httpHelper = new HttpHelper();

		#region 1. 农业银行

		/// <summary>
		/// 农业银行
		/// </summary>
		/// <returns></returns>
		public ActionResult Abc(string randomText, string timeStamp)
		{
			ViewBag.RandomText = randomText;
			ViewBag.TimeStamp = timeStamp;
			return View();
		}
		#endregion

		#region 2. 中国银行

		/// <summary>
		/// 中国银行
		/// </summary>
		/// <returns></returns>
		public ActionResult Boc(string randomKey_S)
		{
			if (!string.IsNullOrEmpty(randomKey_S))
			{
				ViewBag.RandomKey_S = Encoding.Default.GetString(Convert.FromBase64String(randomKey_S));
			}
			return View();
		}

		#endregion

		#region 3. 中信银行

		/// <summary>
		/// 中信银行
		/// </summary>
		/// <returns></returns>
		public ActionResult Citic(string EMP_SID, string token)
		{
			ViewBag.EMP_SID = EMP_SID;
			ViewBag.token = token;
			return View();
		}

		public string createMcryptKey(string EMP_SID, string token, string clientRandom)
		{
			Log4netAdapter.WriteInfo("createMcryptKey");

			var redisCookiesPackage = ProjectEnums.WebSiteType.CiticBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			Log4netAdapter.WriteInfo("currentCookie ==> " + currentCookie);
			var strUrl = "https://i.bank.ecitic.com/perbank6/createMcryptKey.do?EMP_SID=" + EMP_SID;
			var httpItem = new HttpItem
			{
				URL = strUrl,
				Method = "POST",
				Postdata = "clientRandom=" + clientRandom.ToUrlEncode(),
				Cookie = currentCookie,

				Accept = "*/*",
				ContentType = "application/x-www-form-urlencoded;charset=UTF-8",
				Referer = "https://i.bank.ecitic.com/perbank6/signIn.do",
				Host = "i.bank.ecitic.com",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			var httpResult = httpHelper.GetHtml(httpItem);

			Log4netAdapter.WriteInfo("createMcryptKey:html->>" + httpResult.Html.Trim());
			return httpResult.Html;
		}

		public string createSessionKey(string EMP_SID, string token, string clientRandom)
		{
			Log4netAdapter.WriteInfo("createSessionKey");

			var redisCookiesPackage = ProjectEnums.WebSiteType.CiticBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			var strUrl = "https://i.bank.ecitic.com/perbank6/createSessionKey.do?EMP_SID=" + EMP_SID;
			var httpItem = new HttpItem
			{
				URL = strUrl,
				Method = "POST",
				Postdata = "clientRandom=" + clientRandom.ToUrlEncode(),
				Cookie = currentCookie,

				Accept = "*/*",
				ContentType = "application/x-www-form-urlencoded;charset=UTF-8",
				Referer = "https://i.bank.ecitic.com/perbank6/signIn.do",
				Host = "i.bank.ecitic.com",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			var httpResult = httpHelper.GetHtml(httpItem);

			Log4netAdapter.WriteInfo("createSessionKey:html->>" + httpResult.Html.Trim());
			return httpResult.Html;
		}
		#endregion

		#region 4. 交通银行

		/// <summary>
		/// 交通银行
		/// </summary>
		/// <returns></returns>
		public ActionResult Comm(string randomText, string token, string reqSafeFields, string pSessionId)
		{
			ViewBag.RandomText = randomText;
			ViewBag.Token = token;
			ViewBag.ReqSafeFields = reqSafeFields;
			ViewBag.PSessionId = pSessionId;

			return View();
		}

		public void syLogin(FormCollection fc, string token)
		{
			var postData = "";
			var redisEncryptPackage = ProjectEnums.WebSiteType.CommBank.ToString() + Constants.REQEUST_ENCRYPT_PREFIX;
			foreach (var key in fc.AllKeys)
			{
				if (key == "password" || key == "safeInputDeviceInfo" || key == "ReqSafeFields")
				{
					postData += (key + "=" + fc[key].ToUrlEncode() + "&");
				}
				else
				{
					postData += (key + "=" + fc[key] + "&");
				}
			}
			//保存登录参数
			RedisHelper.SetCache(token, postData.TrimEnd('&'), redisEncryptPackage);
		}

		#endregion

		#region 5. 工商银行

		/// <summary>
		/// 工商银行
		/// </summary>
		/// <returns></returns>
		public ActionResult Icbc(string token, string randomId, string randomCode, string rules, string changeRules, string publicKey)
		{
			ViewBag.Token = token;
			ViewBag.RandomId = randomId;
			ViewBag.RandomCode = randomCode;
			ViewBag.Rules = rules;
			ViewBag.ChangeRules = changeRules;
			ViewBag.PublicKey = publicKey;

			return View();
		}

		public void ICBCINBSEstablishSessionServlet(FormCollection fc, string token)
		{
			var postData = "";
			var redisEncryptPackage = ProjectEnums.WebSiteType.IcbcBank.ToString() + Constants.REQEUST_ENCRYPT_PREFIX;
			foreach (var key in fc.AllKeys)
			{
				if (key == "ua" || key == "HWInfo" || key == "currentip" || key == "currentmac" || key == "firstip" ||
					key == "firstmac" || key == "secondip" || key == "secondmac" || key == "dataHash")
				{
					postData += (key + "=" + fc[key].ToUrlEncode() + "&");
				}
				else
				{
					postData += (key + "=" + fc[key] + "&");
				}
			}
			//保存登录参数
			RedisHelper.SetCache(token, postData.TrimEnd('&'), redisEncryptPackage);
		}

		#endregion

		#region 6. 宁波银行

		/// <summary>
		/// 宁波银行
		/// </summary>
		/// <returns></returns>
		public ActionResult Nbc(string token)
		{
			ViewBag.Token = token;

			return View();
		}

		public string getHttpSessionId(FormCollection fc, string token)
		{
			Log4netAdapter.WriteInfo("getHttpSessionId");

			var postData = "";
			foreach (var key in fc.AllKeys)
			{
				postData += (key + "=" + fc[key] + "&");
			}
			var redisCookiesPackage = ProjectEnums.WebSiteType.NbcBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			var httpItem = new HttpItem
			{
				URL = "https://e.nbcb.com.cn/perbank/getHttpSessionId.do",
				Method = "POST",
				Postdata = postData.TrimEnd('&'),
				Cookie = currentCookie,
				ResultCookieType = ResultCookieType.String,

				Accept = "application/json, text/plain, */*",
				ContentType = "application/x-www-form-urlencoded;charset=utf-8",
				Referer = "https://e.nbcb.com.cn/",
				Host = "e.nbcb.com.cn",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			var httpResult = httpHelper.GetHtml(httpItem);

			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
			Log4netAdapter.WriteInfo("getHttpSessionId:html->>" + httpResult.Html.Trim());
			return httpResult.Html;
		}

		public string HBgetSrandNum(FormCollection fc, string token)
		{
			Log4netAdapter.WriteInfo("HBgetSrandNum");

			var postData = "";
			foreach (var key in fc.AllKeys)
			{
				postData += (key + "=" + fc[key] + "&");
			}
			var redisCookiesPackage = ProjectEnums.WebSiteType.NbcBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			Log4netAdapter.WriteInfo("currentCookie ==> " + currentCookie);
			var httpItem = new HttpItem
			{
				URL = "https://e.nbcb.com.cn/perbank/HBgetSrandNum.do",
				Method = "POST",
				Postdata = postData.TrimEnd('&'),
				Cookie = currentCookie,

				Accept = "application/json, text/plain, */*",
				ContentType = "application/x-www-form-urlencoded;charset=utf-8",
				Referer = "https://e.nbcb.com.cn/",
				Host = "e.nbcb.com.cn",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			var httpResult = httpHelper.GetHtml(httpItem);

			Log4netAdapter.WriteInfo("HBgetSrandNum:html->>" + httpResult.Html.Trim());
			return httpResult.Html;
		}

		public string HBcheckCodeFlag(FormCollection fc, string token)
		{
			Log4netAdapter.WriteInfo("HBcheckCodeFlag");

			var postData = "";
			foreach (var key in fc.AllKeys)
			{
				postData += (key + "=" + fc[key] + "&");
			}
			var redisCookiesPackage = ProjectEnums.WebSiteType.NbcBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			Log4netAdapter.WriteInfo("currentCookie ==> " + currentCookie);
			var httpItem = new HttpItem
			{
				URL = "https://e.nbcb.com.cn/perbank/HBcheckCodeFlag.do",
				Method = "POST",
				Postdata = postData.TrimEnd('&'),
				Cookie = currentCookie,

				Accept = "application/json, text/plain, */*",
				ContentType = "application/x-www-form-urlencoded;charset=utf-8",
				Referer = "https://e.nbcb.com.cn/",
				Host = "e.nbcb.com.cn",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			var httpResult = httpHelper.GetHtml(httpItem);

			Log4netAdapter.WriteInfo("HBcheckCodeFlag:html->>" + httpResult.Html.Trim());
			return httpResult.Html;
		}

		public void HBsignIn(FormCollection fc, string token)
		{
			Log4netAdapter.WriteInfo("HBsignIn");

			var postData = "";
			var redisEncryptPackage = ProjectEnums.WebSiteType.NbcBank.ToString() + Constants.REQEUST_ENCRYPT_PREFIX;
			foreach (var key in fc.AllKeys)
			{
				if (key == "password" || key == "MFM")
				{
					postData += (key + "=" + fc[key].ToUrlEncode() + "&");
				}
				else
				{
					postData += (key + "=" + fc[key] + "&");
				}
			}
			//var redisCookiesPackage = ProjectEnums.WebSiteType.NbcBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			//var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			//Log4netAdapter.WriteInfo("currentCookie ==> " + currentCookie);
			//var httpItem = new HttpItem
			//{
			//	URL = "https://e.nbcb.com.cn/perbank/HBsignIn.do",
			//	Method = "POST",
			//	Postdata = postData.TrimEnd('&'),
			//	Cookie = currentCookie,

			//	Accept = "application/json, text/plain, */*",
			//	ContentType = "application/x-www-form-urlencoded;charset=utf-8",
			//	UserAgent= "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
			//	Referer = "https://e.nbcb.com.cn/",
			//	Host = "e.nbcb.com.cn",
			//	Expect100Continue = false,
			//};
			//httpItem.Header.Add("Accept-Language", "zh-CN");
			//httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			//var httpResult = httpHelper.GetHtml(httpItem);

			//Log4netAdapter.WriteInfo("HBcheckCodeFlag:html->>" + httpResult.Html.Trim());
			//保存登录参数
			RedisHelper.SetCache(token, postData.TrimEnd('&'), redisEncryptPackage);
		}

		#endregion

		#region 7. 平安银行
		public ActionResult PingAn(string token)
		{
			ViewBag.Token = token;
			return View();
		}


		public string showVerifyCode(FormCollection fc, string token)
		{
			Log4netAdapter.WriteInfo("showVerifyCode");

			var postData = "";
			foreach (var key in fc.AllKeys)
			{
				postData += (key + "=" + fc[key] + "&");
			}
			var redisCookiesPackage = ProjectEnums.WebSiteType.PinganBank.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			Log4netAdapter.WriteInfo("currentCookie ==> " + currentCookie);
			var httpItem = new HttpItem
			{
				URL = "https://bank.pingan.com.cn/ibp/portal/pc/showVerifyCode.do",
				Method = "POST",
				Postdata = postData.TrimEnd('&'),
				Cookie = currentCookie,

				Accept = "application/json, text/plain, */*",
				ContentType = "application/x-www-form-urlencoded;charset=utf-8",
				Referer = "https://bank.pingan.com.cn/ibp/bank/index.html#home/home/index",
				Host = "bank.pingan.com.cn",
				Expect100Continue = false,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			var httpResult = httpHelper.GetHtml(httpItem);

			Log4netAdapter.WriteInfo("showVerifyCode:html->>" + httpResult.Html.Trim());
			return httpResult.Html;
		}

		public void login(FormCollection fc, string token)
		{
			Log4netAdapter.WriteInfo("login");

			var postData = "";
			var redisEncryptPackage = ProjectEnums.WebSiteType.PinganBank.ToString() + Constants.REQEUST_ENCRYPT_PREFIX;
			foreach (var key in fc.AllKeys)
			{
				if (key == "cdata")
				{
					postData += (key + "=" + fc[key].ToUrlEncode() + "&");
				}
				else
				{
					postData += (key + "=" + fc[key] + "&");
				}
			}
			//保存登录参数
			RedisHelper.SetCache(token, postData.TrimEnd('&'), redisEncryptPackage);
		}

		#endregion

	}
}