using Newtonsoft.Json;
using System.Threading;
using Vcredit.ActivexLogin.App.Tools;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
	/// <summary>
	/// 广发银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CgbBank)]
	public class CgbBankBizImpl : WebSiteBizTemplate
	{
		public CgbBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "广发银行",
				Url = "https://ebanks.cgbchina.com.cn/perbank/",
				JsFileName = "bank/cgbbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<CgbBankRequestEntity>(requestStr);

			m_SyncContext.Post(InvokeScriptSafePost, new object[] { siteType, request });
			DD.mov(945, 275);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Account);
			Thread.Sleep(5);

			DD.mov(945, 325);
			DD.DoMouseClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(5);
			
			m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
		}
		
		public override void InvokeScriptSafePost(object requestObj)
		{
			var request = (CgbBankRequestEntity)requestObj;
			//删除前台cookie
			if (!WindowApi.SuppressWininetBehavior())
				return;
			//获取后台cookie
			var redisCookiesPackage = siteType.ToString() + Constants.REQEUST_COOKIE_PREFIX;
			var currentCookie = RedisHelper.GetCache<string>(request.Token, redisCookiesPackage);
			//后台cookie传给前台
			CURRENT_WEBBROWSER_DOCUMENT.Cookie = currentCookie;
		}

		public override void SaveEncryptString(object requestObj)
		{
			var request = (CgbBankRequestEntity)requestObj;
			var encryptString = GetEncryptString("");
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
			//保存前台cookie
			RedisHelper.SetCache(request.Token, CURRENT_WEBBROWSER_DOCUMENT.Cookie, redisCookiesPackage);
		}

	}
}
