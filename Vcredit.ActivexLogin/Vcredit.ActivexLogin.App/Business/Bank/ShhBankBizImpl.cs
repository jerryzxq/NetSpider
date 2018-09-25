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
	/// 上海银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.ShhBank)]
	public class ShhBankBizImpl : WebSiteBizTemplate
	{
		public ShhBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "上海银行",
				Url = "https://ebanks.bankofshanghai.com/pweb/prelogin.do?LoginType=R&_locale=zh_CN",
				JsFileName = "bank/shhbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<ShhBankRequestEntity>(requestStr);

			DD.mov(960, 275);
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
			var request = (ShhBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.Account + "," + request.TS);
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
		}

	}
}
