using mshtml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
    /// <summary>
    /// 农业银行
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.AbcBank)]
    public class AbcBankBizImpl : WebSiteBizTemplate
    {
        public AbcBankBizImpl() : base()
        { }
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteName = "农业银行",
                //Url = "https://perbank.abchina.com/EbankSite/startup.do",
                Url = Constants.WebAppUrl + "/bank/abc",
                JsFileName = "bank/abcbank.js"
            };
        }

        public override void DoWork(string requestStr,
              SynchronizationContext m_SyncContext,
              SendOrPostCallback SaveEncryptStringSafePost,
              SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<AbcBankRequestEntity>(requestStr);

            DD.mov(75, 75);
            //DD.mov(950, 340);
            DD.DoMouseDoubleClick();
            Thread.Sleep(100);
            DD.str(request.Password);
            Thread.Sleep(10);

            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (AbcBankRequestEntity)requestObj;
            var encryptString = GetEncryptStringByIHTMLWindow2(request.TimeStamp);

            Log4netAdapter.WriteInfo("加密后信息为 ==> " + encryptString);
            RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
        }


        public override void InvokeScriptSafePost(object requestObj)
        {
            throw new NotImplementedException();
        }
    }
}
