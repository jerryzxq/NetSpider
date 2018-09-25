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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.HzBank)]
	public class HzBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly HzBankExecutorImpl Instance = new HzBankExecutorImpl();

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new HzBankRequestEntity();
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
				URL = "https://ebank-public.hzbank.com.cn/perbank/logon.jsp",
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
			var httpItem = new HttpItem()
			{
				URL = "https://ebank-public.hzbank.com.cn/perbank/VerifyImage.srv",
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5",
				Referer = "https://ebank-public.hzbank.com.cn/perbank/logon.jsp",
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
			var entity = JsonConvert.DeserializeObject<HzBankEncryptEntity>(encryptStr);

			var _postData = string.Format(@"netType={0}&logonLanguage={1}&userType={2}&simCertDN={3}&password={4}&customerId={5}&sn={6}&dn={7}&userRemoteIP={8}&session_userRemoteIP={9}&endDate={10}&sequenceNumber={11}&checkCode={12}&clientInfo={13}&b2cFlag={14}&pageFlag={15}&broser={16}&sys={17}&info={18}&IPAddress={19}&MACAddress={20}&session_randomNum={21}&pwdKey={22}&session_machineNetwork={23}&session_machineDisk={24}&session_machineCPU={25}",
				entity.netType, entity.logonLanguage, entity.userType, entity.simCertDN, entity.password.ToUrlEncode(),
				entity.customerId, entity.sn, entity.dn, entity.userRemoteIP, entity.session_userRemoteIP, entity.endDate,
				entity.sequenceNumber, captcha, entity.clientInfo.ToUrlEncode(), entity.b2cFlag, entity.pageFlag, entity.broser,
				entity.sys,entity.info.ToUrlEncode(), entity.IPAddress,entity.MACAddress,entity.session_randomNum,entity.pwdKey,
				entity.session_machineNetwork.ToUrlEncode(), entity.session_machineDisk.ToUrlEncode(), entity.session_machineCPU.ToUrlEncode());

			var httpItem = new HttpItem
			{
				URL = "https://ebank-public.hzbank.com.cn/perbank/signIn.do",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://ebank-public.hzbank.com.cn/perbank/logon.jsp",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "ebank-public.hzbank.com.cn",
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
			if (doc.Select("form").First != null && doc.Select("form").Attr("action") == "welcome.do")
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

	public class HzBankEncryptEntity
	{
		public string netType { get; set; }
		public string logonLanguage { get; set; }
		public string userType { get; set; }
		public string simCertDN { get; set; }
		public string password { get; set; }
		public string customerId { get; set; }
		public string sn { get; set; }
		public string dn { get; set; }
		public string userRemoteIP { get; set; }
		public string session_userRemoteIP { get; set; }
		public string endDate { get; set; }
		public string sequenceNumber { get; set; }
		public string clientInfo { get; set; }
		public string b2cFlag { get; set; }
		public string pageFlag { get; set; }
		public string broser { get; set; }
		public string sys { get; set; }
		public string info { get; set; }
		public string IPAddress { get; set; }
		public string MACAddress { get; set; }
		public string session_randomNum { get; set; }
		public string pwdKey { get; set; }
		public string session_machineNetwork { get; set; }
		public string session_machineDisk { get; set; }
		public string session_machineCPU { get; set; }

	}

}
