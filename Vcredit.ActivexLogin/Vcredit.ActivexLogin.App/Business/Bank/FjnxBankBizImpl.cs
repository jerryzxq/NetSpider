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
	/// 福建农信
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.FjnxBank)]
	public class FjnxBankBizImpl : WebSiteBizTemplate
	{
		public FjnxBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "福建农信",
				Url = "https://www.fj96336.com:8443/pweb/prelogin.do?LoginType=R&_locale=zh_CN&BankId=9999",
				JsFileName = "bank/fjnxbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<FjnxBankRequestEntity>(requestStr);

			DD.mov(450, 335);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Account);
			Thread.Sleep(5);

			DD.mov(450, 370);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(5);
			
			m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
		}
		
		public override void InvokeScriptSafePost(object requestObj)
		{
		}

		public override void SaveEncryptString(object requestObj)
		{
			var request = (FjnxBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.TS);
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
		}

	}
}
