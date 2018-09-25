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
    /// 中国银行
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.BocBank)]
    public class BocBankBizImpl : WebSiteBizTemplate
    {
        public BocBankBizImpl() : base()
        { }
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteName = "中国银行",
                //Url = "https://ebsnew.boc.cn/boc15/login.html",
                Url = Constants.WebAppUrl + "/bank/boc",
                JsFileName = "bank/bocbank.js"
            };
        }

        public override void DoWork(string requestStr,
              SynchronizationContext m_SyncContext,
              SendOrPostCallback SaveEncryptStringSafePost,
              SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<BocBankRequestEntity>(requestStr);

            DD.mov(240, 70);
            //DD.mov(600, 390);
            DD.DoMouseClick();
            Thread.Sleep(50);
            DD.DoBackSpace(20);
            Thread.Sleep(50);
            DD.str(request.Password);
            Thread.Sleep(10);


            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (BocBankRequestEntity)requestObj;
            var encryptString = GetEncryptStringByIHTMLWindow2("");

            Log4netAdapter.WriteInfo("加密后信息为 ==> " + encryptString);
            RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
        }

        public override void InvokeScriptSafePost(object requestObj)
        {
            throw new NotImplementedException();
        }
    }
}
