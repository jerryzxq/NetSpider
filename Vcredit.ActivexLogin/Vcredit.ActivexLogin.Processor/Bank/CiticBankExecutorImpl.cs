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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CiticBank)]
	public class CiticBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly CiticBankExecutorImpl Instance = new CiticBankExecutorImpl();
		private string _redisEntityPackage;

		public CiticBankExecutorImpl() : base()
		{
			_redisEntityPackage = redisPackage + ":Entity";
		}

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new CiticBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;
			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var EMP_SID = jObj.SelectToken("$..EMP_SID");
			if (EMP_SID == null || EMP_SID.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 EMP_SID 不能为空");
			entity.EMP_SID = EMP_SID.ToString();

			entity.UrlParam = string.Format("?EMP_SID={0}&token={1}", entity.EMP_SID, entity.Token);

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
			var captchaStr = GetCaptcha(tup.Item1, tup.Item2);

			var r = new { EMP_SID = tup.Item2 };
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
				URL = "https://i.bank.ecitic.com/perbank6/signIn.do",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			var sEMP_SID = doc.Select("form[name=form2Logon] input[name=EMP_SID]").Attr("value");
			string token = Guid.NewGuid().ToString("N");

			currentCookie = CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie);
			return new Tuple<string, string>(token, sEMP_SID);
		}

		private string GetCaptcha(string token, string EMP_SID)
		{
			var httpItem = new HttpItem()
			{
				URL = "https://i.bank.ecitic.com/perbank6/dynPassword.do?cmd=verifyPin&length=4&EMP_SID=" + EMP_SID,
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://i.bank.ecitic.com/perbank6/signIn.do?logonCtrlFailNum=0",
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
			currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);

			var entity = JsonConvert.DeserializeObject<CiticBankEncryptEntity>(encryptStr);
			entity.session_userRemoteMAC = "BFEBFBFFGenuineIntel";

			var _postData = string.Format("opFlag={0}&logonType={1}&customerCtfType={2}&logonNo={3}&password={4}&verifyCode={5}&session_userRemoteIP={6}&session_userRemoteMAC={7}&session_certSubType={8}&session_tagFlag={9}&session_certHashcode={10}&logon_param={11}&computerId={12}&htd_param={13}&ca_id={14}&logonFlag={15}&browserFlag={16}&language={17}&EMP_SID={18}",
			entity.opFlag, entity.logonType, entity.customerCtfType, entity.logonNo, entity.password.ToUrlEncode(), captcha,
			entity.session_userRemoteIP, entity.session_userRemoteMAC, entity.session_certSubType, entity.session_tagFlag,
			entity.session_certHashcode, entity.logon_param, entity.computerId.ToUrlEncode(), entity.htd_param, entity.ca_id,
			entity.logonFlag, entity.browserFlag, entity.language, entity.EMP_SID);

			var httpItem = new HttpItem
			{
				//URL = "https://i.bank.ecitic.com/perbank6/signIn.do", //无验证码登录
				URL = "https://i.bank.ecitic.com/perbank6/commonSignIn.do",//验证码登录
				Method = "POST",
				Accept = "*/*",
				Referer = "https://i.bank.ecitic.com/perbank6/signIn.do",
				ContentType = "application/x-www-form-urlencoded",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "i.bank.ecitic.com",
				KeepAlive = true,
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
			if (doc.Title != null && doc.Title == "个人网银")
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
			var entity = JsonConvert.DeserializeObject<CiticBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
			var captchaStr = GetCaptcha(token, entity.EMP_SID);

			var data = new VerCodeRes
			{
				Token = token,
				VerCodeBase64 = captchaStr,
			};
			return data;
		}
	}

	public class CiticBankEncryptEntity
	{
		public string opFlag { get; set; }
		public string logonType { get; set; }
		public string customerCtfType { get; set; }
		public string logonNo { get; set; }
		public string password { get; set; }
		public string session_userRemoteIP { get; set; }
		public string session_userRemoteMAC { get; set; }
		public string session_certSubType { get; set; }
		public string session_tagFlag { get; set; }
		public string session_certHashcode { get; set; }
		public string logon_param { get; set; }
		public string computerId { get; set; }
		public string htd_param { get; set; }
		public string ca_id { get; set; }
		public string logonFlag { get; set; }
		public string browserFlag { get; set; }
		public string language { get; set; }
		public string EMP_SID { get; set; }

	}

}
