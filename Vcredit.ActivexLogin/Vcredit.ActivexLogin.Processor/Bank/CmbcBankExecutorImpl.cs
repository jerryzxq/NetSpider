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
using Newtonsoft.Json.Linq;

namespace Vcredit.ActivexLogin.Processor.Bank
{
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CmbcBank)]
	public class CmbcBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly CmbcBankExecutorImpl Instance = new CmbcBankExecutorImpl();
		private string _redisEntityPackage;

		public CmbcBankExecutorImpl() : base()
		{
			_redisEntityPackage = redisPackage + ":Entity";
		}

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new CmbcBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;

			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var RandNum = jObj.SelectToken("$..RandNum");
			if (RandNum == null || RandNum.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 RandNum 不能为空");
			entity.RandNum = RandNum.ToString();

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
			var captchaStr = "none";

			if (tup.Item3.ToLower() != "false")
			{
				captchaStr = GetCaptcha(tup.Item1);
			}

			var r = new
			{
				RandNum = tup.Item2,
			};

			var data = new VerCodeRes
			{
				Token = tup.Item1,
				Result = JsonConvert.SerializeObject(r),
				VerCodeBase64 = captchaStr
			};

			return data;
		}
		private Tuple<string, string, string> InitLoginPage()
		{
			var httpItem = new HttpItem()
			{
				URL = "https://nper.cmbc.com.cn/pweb/static/login.html",
				Method = "get",
				Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			//获取cookie&判断是否需要验证码
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			httpItem = new HttpItem()
			{
				URL = "https://nper.cmbc.com.cn/pweb/UserTokenCheckCtrl.do",
				Method = "POST",
				Accept = "application/json, text/plain, */*",
				ContentType = "application/json;charset=utf-8",
				Referer = "https://nper.cmbc.com.cn/pweb/static/login.html",
				Postdata = "{}",
				Cookie = currentCookie,
				ResultCookieType = ResultCookieType.String,
				Host = "nper.cmbc.com.cn",
				UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)",
			};
			httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			httpResult = httpHelper.GetHtml(httpItem);
			var jTokenShowFlag = JObject.Parse(httpResult.Html);
			var TokenShowFlag = "false";
			if (jTokenShowFlag.SelectToken("$.TokenShowFlag") != null)
			{
				TokenShowFlag = jTokenShowFlag.SelectToken("$.TokenShowFlag").ToString();
			}

			//获取随机码
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			httpItem = new HttpItem()
			{
				URL = "https://nper.cmbc.com.cn/pweb/GenerateRand.do",
				Method = "GET",
				Accept = "application/json, text/javascript, */*; q=0.01",
				Referer = "https://nper.cmbc.com.cn/pweb/static/login.html",
				Cookie = currentCookie,
				ResultCookieType = ResultCookieType.String,
				Host = "nper.cmbc.com.cn",
				UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)",
			};
			httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			httpResult = httpHelper.GetHtml(httpItem);

			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			string token = Guid.NewGuid().ToString("N");
			RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

			var jObj = JObject.Parse(httpResult.Html);
			var RandNum = "";
			if (jObj.SelectToken("$.RandNum") != null)
			{
				RandNum = jObj.SelectToken("$.RandNum").ToString();
			}
			return new Tuple<string, string, string>(token, RandNum, TokenShowFlag);
		}

		private string GetCaptcha(string token)
		{
			Random r = new Random();
			var httpItem = new HttpItem()
			{
				URL = "https://nper.cmbc.com.cn/pweb/GenTokenImg.do?random=" + r.Next(100),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "*/*",
				Referer = "https://nper.cmbc.com.cn/pweb/static/login.html",
				UserAgent= "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)"
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
			var entity = JsonConvert.DeserializeObject<CmbcBankEncryptEntity>(encryptStr);
			var entityReq = JsonConvert.DeserializeObject<CmbcBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
			entity.UserId = entityReq.Account;
			entity._vTokenName = captcha;
			var _postData = JsonConvert.SerializeObject(entity);

			var httpItem = new HttpItem
			{
				URL = "https://nper.cmbc.com.cn/pweb/clogin.do",
				Method = "POST",
				Accept = "application/json, text/plain, */*",
				ContentType = "application/json;charset=utf-8",
				Referer = "https://nper.cmbc.com.cn/pweb/static/login.html",
				UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)",
				Host = "nper.cmbc.com.cn",
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue = false,
			};
			httpItem.Header.Add("X-Requested-With", "XMLHttpRequest");
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			httpItem.ProxyIp = "127.0.0.1:8888";

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			if (httpResult.Html.Contains("SerialNo"))
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
	}

	public class CmbcBankEncryptEntity
	{
		public string PwdResult { get; set; }
		public string CspName { get; set; }
		public string BankId { get; set; }
		public string LoginType { get; set; }
		public string _locale { get; set; }
		public string UserId { get; set; }
		public string _vTokenName { get; set; }
		public string _UserDN { get; set; }
		public string _asii { get; set; }
		public string _targetPath { get; set; }

	}

}
