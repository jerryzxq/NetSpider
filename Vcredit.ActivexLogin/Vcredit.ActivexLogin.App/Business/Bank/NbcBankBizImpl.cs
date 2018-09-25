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
	/// 宁波银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.NbcBank)]
	public class NbcBankBizImpl : WebSiteBizTemplate
	{
		public NbcBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "宁波银行",
				Url = Constants.WebAppUrl + "/bank/nbc",
				//JsFileName = "bank/nbcbank.js",
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<NbcBankRequestEntity>(requestStr);

			DD.mov(1045, 180);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Account);
			Thread.Sleep(500);

			DD.mov(1045, 235);
			DD.DoMouseDoubleClick();
			DD.DoBackSpace(20);
			DD.str(request.Password);
			Thread.Sleep(5);

			int ddcode = DD.todc((int)Keys.Enter);
			DD.key(ddcode, 1);
			Thread.Sleep(500);

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
