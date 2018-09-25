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
	/// 江苏银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.JsBank)]
	public class JsBankBizImpl : WebSiteBizTemplate
	{
		public JsBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "江苏银行",
				Url = "https://ebank.jsbchina.cn/newperbank/",
				JsFileName = "bank/jsbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<JsBankRequestEntity>(requestStr);

			m_SyncContext.Post(InvokeScriptSafePost, new object[] { siteType, request });

			DD.mov(970, 290);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(5);

			m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
		}

		public override void InvokeScriptSafePost(object requestObj)
		{
			var request = (JsBankRequestEntity)requestObj;
			//删除前台cookie
			if (!WindowApi.SuppressWininetBehavior())
				return;
			//获取后台cookie
			var currentCookie = RedisHelper.GetCache<string>(request.Token, redisCookiesPackage);
			//后台cookie传给前台
			CURRENT_WEBBROWSER_DOCUMENT.Cookie = currentCookie;
		}

		public override void SaveEncryptString(object requestObj)
		{
			var request = (JsBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.Account);
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
			//保存前台cookie
			RedisHelper.SetCache(request.Token, CURRENT_WEBBROWSER_DOCUMENT.Cookie, redisCookiesPackage);
		}

	}
}
