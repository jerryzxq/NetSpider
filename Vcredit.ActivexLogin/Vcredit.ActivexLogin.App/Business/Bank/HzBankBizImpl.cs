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
	/// 杭州银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.HzBank)]
	public class HzBankBizImpl : WebSiteBizTemplate
	{
		public HzBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "杭州银行",
				Url = "https://ebank-public.hzbank.com.cn/perbank/logon.jsp",
				JsFileName = "bank/hzbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<HzBankRequestEntity>(requestStr);

			m_SyncContext.Post(InvokeScriptSafePost, new object[] { siteType, request });

			DD.mov(790, 300);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(5);
			
			m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
		}
		
		public override void InvokeScriptSafePost(object requestObj)
		{
			var request = (HzBankRequestEntity)requestObj;
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
			var request = (HzBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.Account);
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
			//保存前台cookie
			//RedisHelper.SetCache(request.Token, CURRENT_WEBBROWSER_DOCUMENT.Cookie, redisCookiesPackage);
		}

	}
}
