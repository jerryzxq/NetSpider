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
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.IcbcBank)]
	public class IcbcBankExecutorImpl : ExecutorTemplate, IActivexLoginExecutor
	{
		public static readonly IcbcBankExecutorImpl Instance = new IcbcBankExecutorImpl();
		private string _redisEntityPackage;

		public IcbcBankExecutorImpl() : base()
		{
			_redisEntityPackage = redisPackage + ":Entity";
		}

		public BaseRes SendOriginalData(ActivexLoginReq req)
		{
			var entity = new IcbcBankRequestEntity();
			entity.Token = req.Token;
			entity.Account = req.Account;
			entity.Password = req.Password;
			if (req.AdditionalParam.IsEmpty())
				throw new ArgumentNullException("AdditionalParam 不能为空");

			var jObj = JObject.Parse(req.AdditionalParam);
			var RandomId = jObj.SelectToken("$..RandomId");
			if (RandomId == null || RandomId.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 RandomId 不能为空");
			entity.RandomId = RandomId.ToString();

			var RandomCode = jObj.SelectToken("$..RandomCode");
			if (RandomCode == null || RandomCode.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 RandomCode 不能为空");
			entity.RandomCode = RandomCode.ToString();

			var Rules = jObj.SelectToken("$..Rules");
			if (Rules == null || Rules.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 Rules 不能为空");
			entity.Rules = Rules.ToString();

			var ChangeRules = jObj.SelectToken("$..ChangeRules");
			if (ChangeRules == null || ChangeRules.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 ChangeRules 不能为空");
			entity.ChangeRules = ChangeRules.ToString();

			var PublicKey = jObj.SelectToken("$..PublicKey");
			if (PublicKey == null || PublicKey.ToString().IsEmpty())
				throw new ArgumentException("AdditionalParam 参数中 PublicKey 不能为空");
			entity.PublicKey = PublicKey.ToString();

			entity.UrlParam = string.Format("?Token={0}&RandomId={1}&RandomCode={2}&Rules={3}&ChangeRules={4}&PublicKey={5}", entity.Token, entity.RandomId, entity.RandomCode, entity.Rules, entity.ChangeRules, entity.PublicKey);

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
			var r = new
			{
				RandomId = tup.Item2,
				RandomCode = tup.Item3,
				Rules = tup.Item4,
				ChangeRules = tup.Item5,
				PublicKey = tup.Item6
			};

			var data = new VerCodeRes
			{
				Token = tup.Item1,
				VerCodeBase64 = captchaStr,
				Result = JsonConvert.SerializeObject(r),
			};
			return data;
		}
		private Tuple<string, string, string, string, string, string> InitLoginPage()
		{
			var httpItem = new HttpItem()
			{
				URL = "https://epass.icbc.com.cn/login/login.jsp?StructCode=1&orgurl=0&STNO=50",
				Method = "get",
				Accept = "text/html, application/xhtml+xml, */*",
				Allowautoredirect = false,
				ResultCookieType = ResultCookieType.String,
			};
			var httpResult = httpHelper.GetHtml(httpItem);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);

			var randomId = doc.Select("form[name=form1] input[name=randomId]").Attr("value");
			var randomCode = Regex.Match(httpResult.Html, @"(?<=document.all.safeEdit1.setRandom\(').*(?='\))").Value;
			var rules = Regex.Match(httpResult.Html, @"(?<=document.all.safeEdit1.setRules\(').*(?='\))").Value;
			var changeRules = Regex.Match(httpResult.Html, @"(?<=document.all.safeEdit1.setChangeRules\(').*(?='\))").Value;
			var publicKey = Regex.Match(httpResult.Html, @"(?<=document.all.safeEdit1.setPublicKeyNew\(').*(?='\))").Value;
			var token = Guid.NewGuid().ToString("N");

			currentCookie = httpResult.Cookie;
			return new Tuple<string, string, string, string, string, string>(token, randomId, randomCode, rules, changeRules, publicKey);
		}

		private string GetCaptcha(string token, string randomId)
		{
			Random r = new Random();
			var httpItem = new HttpItem()
			{
				URL = string.Format("https://epass.icbc.com.cn/servlet/ICBCVerificationCodeImageCreate?randomId={0}&height=36&width=90", randomId),
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
			var entity = JsonConvert.DeserializeObject<IcbcBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
			var _postData = encryptStr.Replace("verifyCodeCn=", "verifyCodeCn=" + captcha);
			_postData = _postData.Replace("verifyCode=", "verifyCode=" + captcha);
			_postData = _postData.Replace("logonCardNum=", "logonCardNum=" + entity.Account);

			var httpItem = new HttpItem
			{
				URL = "https://epass.icbc.com.cn/servlet/ICBCINBSEstablishSessionServlet",
				Method = "POST",
				Accept = "text/html, application/xhtml+xml, */*",
				ContentType = "application/x-www-form-urlencoded",
				Referer = "https://epass.icbc.com.cn/login/login.jsp?StructCode=1&orgurl=0&STNO=50",
				UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)",
				Host = "epass.icbc.com.cn",
				Postdata = _postData,
				Cookie = currentCookie,
				Expect100Continue = false,
				Allowautoredirect = false
			};
			httpItem.Header.Add("Accept-Language", "zh-CN");
			httpItem.Header.Add("Accept-Encoding", "gzip, deflate");
			httpItem.ProxyIp = "127.0.0.1:8888";

			var httpResult = httpHelper.GetHtml(httpItem);
			currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
			Log4netAdapter.WriteInfo(httpResult.Html);
			var doc = NSoup.NSoupClient.Parse(httpResult.Html);
			//Step 1 校验步骤1
			if (doc.Select("form[name=form1]").First != null)
			{
				var dse_sessionId = doc.Select("form[name=form1] input[name=dse_sessionId]").Attr("value");
				var dse_applicationId = doc.Select("form[name=form1] input[name=dse_applicationId]").Attr("value");
				var dse_operationName = doc.Select("form[name=form1] input[name=dse_operationName]").Attr("value");
				var dse_pageId = doc.Select("form[name=form1] input[name=dse_pageId]").Attr("value");
				_postData = string.Format("dse_sessionId={0}&dse_applicationId={1}&dse_operationName={2}&dse_pageId={3}",
											dse_sessionId, dse_applicationId, dse_operationName, dse_pageId);
				httpItem.URL = "https://epass.icbc.com.cn/servlet/ICBCINBSReqServlet";
				httpItem.Referer = "https://epass.icbc.com.cn/servlet/ICBCINBSEstablishSessionServlet";
				httpItem.Postdata = _postData;
				httpItem.Cookie = currentCookie;

				httpResult = httpHelper.GetHtml(httpItem);
				doc = NSoup.NSoupClient.Parse(httpResult.Html);
				//Step 2 校验步骤2
				if (doc.Select("form[name=form1]").First != null)
				{
					dse_operationName = doc.Select("form[name=form1] input[name=dse_operationName]").Attr("value");
					dse_pageId = doc.Select("form[name=form1] input[name=dse_pageId]").Attr("value");
					var tranFlag = doc.Select("form[name=form1] input[name=tranFlag]").Attr("value");
					_postData = string.Format("dse_sessionId={0}&dse_applicationId={1}&dse_operationName={2}&dse_pageId={3}&tranFlag={4}",
											dse_sessionId, dse_applicationId, dse_operationName, dse_pageId, tranFlag);
					httpItem.Referer = "https://epass.icbc.com.cn/servlet/ICBCINBSReqServlet";
					httpItem.Postdata = _postData;
					httpItem.Cookie = currentCookie;
					httpResult = httpHelper.GetHtml(httpItem);
					doc = NSoup.NSoupClient.Parse(httpResult.Html);
					//Step 3 校验步骤3
					if (doc.Select("form[name=IntegrateForm]").First != null)
					{
						dse_operationName = doc.Select("form[name=IntegrateForm] input[name=dse_operationName]").Attr("value");
						dse_pageId = doc.Select("form[name=IntegrateForm] input[name=dse_pageId]").Attr("value");
						tranFlag = doc.Select("form[name=IntegrateForm] input[name=tranFlag]").Attr("value");
						var requestTokenid = doc.Select("form[name=IntegrateForm] input[name=requestTokenid]").Attr("value");
						var url = string.Format("https://epass.icbc.com.cn/servlet/ICBCINBSReqServlet?dse_sessionId={0}&requestTokenid={1}&dse_applicationId={2}&dse_operationName={3}&dse_pageId={4}&tranFlag={5}",
												dse_sessionId, requestTokenid, dse_applicationId, dse_operationName, dse_pageId, tranFlag);
						httpItem.URL = url;
						httpItem.Referer = "https://epass.icbc.com.cn/servlet/ICBCINBSEstablishSessionServlet";
						httpItem.Method = "GET";
						httpItem.Cookie = currentCookie;

						httpResult = httpHelper.GetHtml(httpItem);
						doc = NSoup.NSoupClient.Parse(httpResult.Html);
						//Step 4 U盾/动态密码认证
						if (doc.Select("form[name=IntegrateForm]").First != null)
						{
							dse_pageId = doc.Select("form[name=IntegrateForm] input[name=dse_pageId]").Attr("value");
							tranFlag = doc.Select("form[name=IntegrateForm] input[name=tranFlag]").Attr("value");
							dse_operationName = doc.Select("form[name=IntegrateForm] input[name=dse_operationName]").Attr("value");
							_postData = string.Format("dse_sessionId={0}&requestTokenid={1}&dse_applicationId={2}&dse_operationName={3}&dse_pageId={4}&tranFlag={5}",
													dse_sessionId, requestTokenid, dse_applicationId, dse_operationName, dse_pageId, tranFlag);
							httpItem.URL = "https://epass.icbc.com.cn/servlet/ICBCINBSReqServlet";
							httpItem.Referer = string.Format("https://epass.icbc.com.cn/servlet/ICBCINBSReqServlet?dse_sessionId={0}&requestTokenid={1}&dse_applicationId={2}&dse_operationName={3}&dse_pageId=3&tranFlag=0",
																dse_sessionId, requestTokenid, dse_applicationId, dse_operationName);
							httpItem.Method = "POST";
							httpItem.Postdata = _postData;
							httpItem.Cookie = currentCookie;

							httpResult = httpHelper.GetHtml(httpItem);
							doc = NSoup.NSoupClient.Parse(httpResult.Html);
							//Step 5 短信验证码认证
							if (doc.Select("form[name=cmForm]").First != null)
							{
								dse_pageId = doc.Select("form[name=cmForm] input[name=dse_pageId]").Attr("value");
								_postData = string.Format("dse_sessionId={0}&requestTokenid={1}&dse_applicationId={2}&dse_operationName={3}&dse_pageId={4}&tranFlag=6",
														dse_sessionId, requestTokenid, dse_applicationId, dse_operationName, dse_pageId);
								httpItem.Postdata = _postData;
								httpItem.Cookie = currentCookie;

								httpResult = httpHelper.GetHtml(httpItem);
								currentCookie = CookieUtil.CookieCombine(currentCookie, CookieUtil.ConvertResponseCookieToRequestCookie(httpResult.Cookie));
								if (httpResult.Html.Contains("短信验证码："))
								{
									res.Result = JsonConvert.SerializeObject(
												new EncryptDataResultDto
												{
													Reason = EncryptStatus.Success,
													ReasonDescription = "登陆成功",
													Data = httpResult.Html + " Cookie is ==> " + currentCookie
												});
									return res;
								}
							}
						}
					}
				}
			}
			res.Result = JsonConvert.SerializeObject(
						new EncryptDataResultDto
						{
							Reason = EncryptStatus.Faild,
							ReasonDescription = "登陆失败了",
							Data = httpResult.Html
						});

			return res;
		}

		public VerCodeRes RefreshCaptcha(string token)
		{
			var entity = JsonConvert.DeserializeObject<IcbcBankRequestEntity>(RedisHelper.GetCache<string>(token, _redisEntityPackage));
			var captchaStr = GetCaptcha(token, entity.RandomId);

			var data = new VerCodeRes
			{
				Token = token,
				VerCodeBase64 = captchaStr,
			};
			return data;
		}
	}

}
