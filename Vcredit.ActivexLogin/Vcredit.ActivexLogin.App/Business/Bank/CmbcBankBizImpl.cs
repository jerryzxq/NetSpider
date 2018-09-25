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
	/// 民生银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CmbcBank)]
	public class CmbcBankBizImpl : WebSiteBizTemplate
	{
		public CmbcBankBizImpl() : base()
		{
		}

		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "民生银行",
				Url = "https://nper.cmbc.com.cn/pweb/static/login.html",
				JsFileName = "bank/cmbcbank.js"
			};
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<CmbcBankRequestEntity>(requestStr);

			DD.mov(880, 375);
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
			var request = (CmbcBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.RandNum);
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
		}

	}
}
