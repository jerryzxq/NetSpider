using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
	/// <summary>
	/// 华夏银行
	/// </summary>
	[RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.HxBank)]
	public class HxBankBizImpl : WebSiteBizTemplate
	{
		public HxBankBizImpl() : base()
		{
		}
		protected override void InitCurrentEntity()
		{
			CURRENT_WEBSITE = new WebSiteEntity
			{
				SiteName = "华夏银行",
				Url = "https://sbank.hxb.com.cn/easybanking/login.do",
				JsFileName = "bank/hxbank.js"
			};
		}

		/// <summary>
		/// 初始化参数 webbrowser_DocumentComplete 候执行
		/// </summary>
		/// <param name="doc"></param>
		public override void Init(HtmlDocument doc)
		{
			CURRENT_WEBBROWSER_DOCUMENT = doc.Window.Frames["login"].Document;
			AddScriptToPage(CURRENT_WEBBROWSER_DOCUMENT, CURRENT_WEBSITE.JsFileName);
		}

		public override void DoWork(string requestStr,
									SynchronizationContext m_SyncContext,
									SendOrPostCallback SaveEncryptStringSafePost,
									SendOrPostCallback InvokeScriptSafePost)
		{
			var request = JsonConvert.DeserializeObject<HxBankRequestEntity>(requestStr);

			DD.mov(885, 300);
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
			var request = (HxBankRequestEntity)requestObj;
			var encryptString = GetEncryptString(request.Account+","+request.RandCode);
			//保存密码参数
			RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
		}

	}
}
