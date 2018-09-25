using Newtonsoft.Json;
using System.Threading;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
	/// <summary>
	/// 工商银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.IcbcBank)]
	public class IcbcBankBizImpl : WebSiteBizTemplate
	{
		public IcbcBankBizImpl() : base()
		{
		}

		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "工商银行",
				Url = Constants.WebAppUrl + "/bank/icbc",
				JsFileName = "bank/icbcbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<IcbcBankRequestEntity>(requestStr);
			DD.mov(65, 195);
			DD.DoMouseClick();
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
