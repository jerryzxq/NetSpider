using Newtonsoft.Json;
using System.Threading;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
	/// <summary>
	/// 光大银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CebBank)]
	public class CebBankBizImpl : WebSiteBizTemplate
	{
		public CebBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "光大银行",
				Url = "https://www.cebbank.com/per/prePerlogin.do?_locale=zh_CN",
				JsFileName = "bank/cebbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<CebBankRequestEntity>(requestStr);

			//DD.mov(821, 275);
			//DD.DoMouseDoubleClick();
			//DD.DoBackSpace(20);
			//DD.str(request.Account);
			//Thread.Sleep(5);

			DD.mov(821, 308);
			DD.DoMouseClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(5);

			//m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
		}

		public override void InvokeScriptSafePost(object requestObj)
		{
		}

		public override void SaveEncryptString(object requestObj)
		{
			var request = (CebBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.UrlParam);
			////保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
			////保存前台cookie
			//var strCookie = FullWebBrowserCookie.GetCookieInternal(CURRENT_WEBBROWSER_DOCUMENT.Url, true);
			//RedisHelper.SetCache(request.Token, strCookie, redisCookiesPackage);
		}

	}
}
