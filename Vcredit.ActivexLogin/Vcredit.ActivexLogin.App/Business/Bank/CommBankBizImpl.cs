using Newtonsoft.Json;
using System.Threading;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
	/// <summary>
	/// 交通银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CommBank)]
	public class CommBankBizImpl : WebSiteBizTemplate
	{
		public CommBankBizImpl() : base()
		{
		}

		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "交通银行",
				Url = Constants.WebAppUrl + "/bank/comm",
				JsFileName = "bank/commbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<CommBankRequestEntity>(requestStr);
			DD.mov(900, 255);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Account);
			Thread.Sleep(5);

			DD.mov(900, 305);
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
			GetEncryptString("");
		}

	}
}
