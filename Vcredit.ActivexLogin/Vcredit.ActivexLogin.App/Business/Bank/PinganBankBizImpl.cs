using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
	/// <summary>
	/// 平安银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.PinganBank)]
	public class PinganBankBizImpl : WebSiteBizTemplate
	{
		public PinganBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "平安银行",
				Url = Constants.WebAppUrl + "/bank/pingan",
				//JsFileName = "bank/pinganbank.js",
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<PinganBankRequestEntity>(requestStr);

			Thread.Sleep(1000);
			DD.mov(735, 345);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(50);

			DD.mov(735, 420);
			DD.DoMouseClick();
			int ddcode = DD.todc((int)Keys.Enter);
			DD.key(ddcode, 1);

			m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
		}
		
		public override void InvokeScriptSafePost(object requestObj)
		{
		}

		public override void SaveEncryptString(object requestObj)
		{
		}

	}
}
