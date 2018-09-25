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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.HxBank)]
	public class HxBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly HxBankExecutorImpl Instance = new HxBankExecutorImpl();

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new HxBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;

			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var RandCode = jObj.SelectToken("$..RandCode");
			if (RandCode == null || RandCode.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 RandCode 不能为空");
			entity.RandCode = RandCode.ToString();

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
				RandCode = tup.Item2,
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
				URL = "https://sbank.hxb.com.cn/easybanking/login.do",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Host= "sbank.hxb.com.cn",
				UserAgent= "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = httpResult.Cookie;

			httpItem = new HttpItem()
			{
				URL = "https://sbank.hxb.com.cn/easybanking/jsp/indexComm.jsp",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Referer= "https://sbank.hxb.com.cn/easybanking/login.do",
				Host = "sbank.hxb.com.cn",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Allowautoredirect = false,
				Cookie= currentCookie,
				ResultCookieType = ResultCookieType.String,
			};
			httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));

			httpItem = new HttpItem()
			{
				URL = "https://sbank.hxb.com.cn/easybanking/jsp/login/login.jsp",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Referer= "https://sbank.hxb.com.cn/easybanking/jsp/indexComm.jsp",
				Host = "sbank.hxb.com.cn",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Allowautoredirect = false,
				Cookie = currentCookie,
				ResultCookieType = ResultCookieType.String,
			};
			httpResult = httpHelper.GetHtml(httpItem);
			var randCode = Regex.Match(httpResult.Html, @"(?<=pwd.SetPasswordEncryptionKey\(').*(?=',0,0\))").Value;
			string token = Guid.NewGuid().ToString("N");

			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);
			return new Tuple<string, string>(token, randCode);
		}

		private string GetCaptcha(string token)
		{
			Random r = new Random();
			var httpItem = new HttpItem()
			{
				URL = "https://sbank.hxb.com.cn/easybanking/validateservlet?rand="+r.Next(100),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
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
			var entity = JsonConvert.DeserializeObject<HxBankEncryptEntity>(encryptStr);

			var _postData = string.Format(@"loginVersion={0}&actionType={1}&realpass={2}&dn={3}&sn={4}&signHardId={5}&signPublicKey={6}&signEncryptCode={7}&signClearCode={8}&signCertInfo={9}&customerMacAddr={10}&loginWay={11}&idNoVal={12}&idTypeVal={13}&operator={14}&paraValue={15}&signCSPName={16}&keySN={17}&CFCAVersion={18}&idType={19}&alias={20}&idNo={21}&qy_mima={22}&validateNo={23}&qy_sut={24}",
				entity.loginVersion, entity.actionType, entity.realpass, entity.dn, entity.sn,entity.signHardId,
				entity.signPublicKey, entity.signEncryptCode, entity.signClearCode, entity.signCertInfo,
				entity.customerMacAddr, entity.loginWay, entity.idNoVal, entity.idTypeVal, entity._operator,
				entity.paraValue, entity.signCSPName, entity.keySN, entity.CFCAVersion, entity.idType,
				entity.alias, entity.idNo, entity.qy_mima, captcha, entity.qy_sut.ToUrlEncode());

			var httpItem = new HttpItem
			{
				URL = "https://sbank.hxb.com.cn/easybanking/login.do?",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://sbank.hxb.com.cn/easybanking/jsp/login/login.jsp",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "sbank.hxb.com.cn",
				Postdata = _postData,
				Cookie = currentCookie,
				Allowautoredirect = false,
				Expect100Continue = false
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));

			httpItem = new HttpItem()
			{
				URL = "https://sbank.hxb.com.cn/easybanking/PAccountWelcomePage/FormParedirectAction.do?actionType=entry",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Referer = "https://sbank.hxb.com.cn/easybanking/jsp/login/login.jsp",
				Host = "sbank.hxb.com.cn",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Allowautoredirect = false,
				Cookie = currentCookie,
				ResultCookieType = ResultCookieType.String,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			httpResult = httpHelper.GetHtml(httpItem);
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			if (doc.Title != null && doc.Title == "华夏银行网上个人银行")
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

	public class HxBankEncryptEntity
	{
		public string loginVersion { get; set; }
		public string actionType { get; set; }
		public string realpass { get; set; }
		public string dn { get; set; }
		public string sn { get; set; }
		public string signHardId { get; set; }
		public string signPublicKey { get; set; }
		public string signEncryptCode { get; set; }
		public string signClearCode { get; set; }
		public string signCertInfo { get; set; }
		public string customerMacAddr { get; set; }
		public string loginWay { get; set; }
		public string idNoVal { get; set; }
		public string idTypeVal { get; set; }
		public string _operator { get; set; }
		public string paraValue { get; set; }
		public string signCSPName { get; set; }
		public string keySN { get; set; }
		public string CFCAVersion { get; set; }
		public string idType { get; set; }
		public string alias { get; set; }
		public string idNo { get; set; }
		public string qy_mima { get; set; }
		public string qy_sut { get; set; }

	}

}
