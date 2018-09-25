using Newtonsoft.Json;
using System;
using System.Text;
using System.Xml;
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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CgbBank)]
	public class CgbBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly CgbBankExecutorImpl Instance = new CgbBankExecutorImpl();

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new CgbBankRequestEntity();
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
				URL = "https://ebanks.cgbchina.com.cn/perbank/",
				Method = "get",
				Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
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
				URL = "https://ebanks.cgbchina.com.cn/perbank/VerifyImage?update=" + r.Next(100),
				Method = "get",
				Cookie = currentCookie,
				ResultType = ResultType.Byte,
				ResultCookieType = ResultCookieType.String,

				Accept = "image/webp,image/apng,image/*,*/*;q=0.8",
				Referer = "https://ebanks.cgbchina.com.cn/perbank/",
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			RedisHelper.SetCache(token, currentCookie, redisCookiesPackage);

			return CommonFun.GetVercodeBase64(httpResult.ResultByte);
		}

		public BaseRes DoRealLogin(string token, string captcha)
		{
			var res = new BaseRes() { Token = token };

			var _postData = string.Format("verifyCode={0}", captcha.ToLower());
			currentCookie = RedisHelper.GetCache<string>(token, redisCookiesPackage);
			var httpItem = new HttpItem
			{
				URL = "https://ebanks.cgbchina.com.cn/perbank/checkImageCode.do?",
				Method = "POST",
				Accept = "text/javascript, text/html, application/xml, text/xml, */*",
				ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
				Referer = "https://ebanks.cgbchina.com.cn/perbank/",

				PostEncoding = Encoding.UTF8,
				Postdata = _postData,
				Cookie = currentCookie,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(httpResult.Html);
			XmlNode xmlObj = xmlDoc.SelectSingleNode("kColl/field [@id='isVerifyCodeTimeout']").Attributes["value"];
			if (xmlObj.Value == "true")
			{
				xmlObj = xmlDoc.SelectSingleNode("kColl/field [@id='flag']").Attributes["value"];
				if (xmlObj.Value != "0")
				{
					res.Result = JsonConvert.SerializeObject(
						new EncryptDataResultDto
						{
							Reason = EncryptStatus.Faild,
							ReasonDescription = "验证码错误，请重新输入"
						});
					return res;
				}
			}
			else
			{
				res.Result = JsonConvert.SerializeObject(
					new EncryptDataResultDto
					{
						Reason = EncryptStatus.Faild,
						ReasonDescription = "验证码已超时，请刷新验证码后再重新输入"
					});
				return res;
			}
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
			var entity = JsonConvert.DeserializeObject<CgbBankEncryptEntity>(encryptStr);

			_postData = string.Format("loginId={0}&logonType={1}&userPassword={2}&accPassword={3}&accountNo={4}&checkCode={5}&version={6}&isCreditCard={7}&isPass={8}&clientMacAdress={9}&clientMainboardNo={10}&clientHarddiskNo={11}&clientIP={12}&actionFlag={13}&content={14}&isCreditVersion={15}&hrefValue={16}",
				entity.loginId, entity.logonType, entity.userPassword, entity.accPassword, entity.accountNo, captcha, entity.version,
				entity.isCreditCard, entity.isPass, entity.clientMacAdress, entity.clientMainboardNo, entity.clientHarddiskNo,
				entity.clientIP, entity.actionFlag, entity.content, entity.isCreditVersion, entity.hrefValue.ToUrlEncode());

			httpItem = new HttpItem
			{
				URL = "https://ebanks.cgbchina.com.cn/perbank/OT0001.do",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://ebanks.cgbchina.com.cn/perbank/",
				UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
				Host = "ebanks.cgbchina.com.cn",
				PostEncoding = Encoding.UTF8,
				Postdata = _postData,
				Cookie = currentCookie,
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");

			httpResult = httpHelper.GetHtml(httpItem);
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
            throw new NotImplementedException();
        }
    }

	public class CgbBankEncryptEntity
	{
		public string loginId { get; set; }
		public string accountNo { get; set; }
		public string accPassword { get; set; }
		public string logonType { get; set; }
		public string isCreditCard { get; set; }
		public string actionFlag { get; set; }
		public string isCreditVersion { get; set; }
		public string content { get; set; }
		public string isPass { get; set; }
		public string clientIP { get; set; }
		public string clientMacAdress { get; set; }
		public string clientMainboardNo { get; set; }
		public string clientHarddiskNo { get; set; }
		public string hrefValue { get; set; }
		public string userPassword { get; set; }
		public string version { get; set; }

	}

}
