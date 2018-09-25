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
	/// 中信银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CiticBank)]
	public class CiticBankBizImpl : WebSiteBizTemplate
	{
		public CiticBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "中信银行",
				Url = Constants.WebAppUrl + "/bank/citic",
				JsFileName = "bank/citicbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<CiticBankRequestEntity>(requestStr);
			Thread.Sleep(2500);

			DD.mov(940, 160);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Account);
			Thread.Sleep(5);

			DD.mov(1025, 230);
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
			var request = (CiticBankRequestEntity)requestObj;
			var encryptString = GetEncryptString("");
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
		}

	}
}
