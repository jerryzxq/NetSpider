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
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.App.Business.Bank
{
    /// <summary>
    /// 招商银行
    /// 编译成功的exe需要通过vs命令提示符执行：
    /// editbin.exe /NXCOMPAT:NO E:\01_project\01_netspider\SVN_PROJECT\branches\dotnet\Vcredit.ActivexLogin\Vcredit.ActivexLogin.App\bin\Debug\Vcredit.ActivexLogin.App.exe
    /// 详细参见：http://blog.csdn.net/charlessimonyi/article/details/30479131
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.CmbBank)]
    public class CmbBankBizImpl : WebSiteBizTemplate
    {
        public CmbBankBizImpl() : base()
        { }
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteName = "招商银行",
                Url = "https://pbsz.ebank.cmbchina.com/CmbBank_GenShell/UI/GenShellPC/Login/Login.aspx",
                JsFileName = "bank/cmbbank.js"
            };
        }

        public override void DoWork(string requestStr,
              SynchronizationContext m_SyncContext,
              SendOrPostCallback SaveEncryptStringSafePost,
              SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<CmbBankRequestEntity>(requestStr);

            DD.mov(900, 300);
            DD.DoMouseClick();
            DD.DoBackSpace(32);
            //DD.str(request.Account);
            foreach (var item in request.Account)
            {
                Thread.Sleep(rand.Next(20, 80));
                DD.str(item.ToString());
            }
            _PreAccountLength = request.Account.Length;
            Thread.Sleep(5);

            DD.mov(900, 355);
            DD.DoMouseClick();
            DD.DoBackSpace(32);
            //DD.str(request.Password);
            foreach (var item in request.Password)
            {
                Thread.Sleep(rand.Next(20, 80));
                DD.str(item.ToString());
            }
            _PrePwdLength = request.Password.Length;
            Thread.Sleep(15);

            DD.mov(rand.Next(730, 800), rand.Next(30, 655));
            DD.DoMouseClick();
            Thread.Sleep(250 * rand.Next(4, 12));

            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (CmbBankRequestEntity)requestObj;
            var encryptString = GetEncryptStringByIHTMLWindow2(request.GenShell_ClientNo);
            RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
        }

        public override void InvokeScriptSafePost(object requestObj)
        {
            throw new NotImplementedException();
        }
    }
}
