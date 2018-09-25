using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.App.Tools;
using Vcredit.Common.Utility;
using Newtonsoft.Json;
using Vcredit.Framework.Queue.Redis;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Attributes;
using System.Reflection;

namespace Vcredit.ActivexLogin.App.Business
{
    /// <summary>
    /// 广州公积金
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.GuangZhouGjj)]
    public class GuangZhouGjjBizImpl : WebSiteBizTemplate
    {
        public GuangZhouGjjBizImpl() : base()
        {}
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteId = 1,
                SiteName = "广州公积金",
                Url = "https://gzgjj.gov.cn/wsywgr/",
                JsFileName = "guangzhou_gjj.js"
            };
        }

        public override void InvokeScriptSafePost(object requestObj)
        {
            var request = (GuangZhouGjjRequestEntity)requestObj;
            // 点击账号登录 radio
            object[] type = new object[] { (int)request.LoginType };
            CURRENT_WEBBROWSER_DOCUMENT.InvokeScript("changLoginType", type);
        }

        public override void DoWork(string requestStr,
            SynchronizationContext m_SyncContext,
            SendOrPostCallback SaveEncryptStringSafePost,
            SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<GuangZhouGjjRequestEntity>(requestStr);

            //切换登录类型
            m_SyncContext.Post(InvokeScriptSafePost, new object[] { siteType, request });

            if (request.LoginType == LoginType.Account)
            {
                DD.mov(500, 370);
                DD.DoMouseDoubleClick();
                DD.DoBackSpace();
                DD.str(request.Account);
                Thread.Sleep(5);

                DD.mov(500, 400);
                DD.DoMouseDoubleClick();
                DD.DoBackSpace();
                DD.str(request.Password);
                Thread.Sleep(5);

                InvokeEncryptScript(request, m_SyncContext, SaveEncryptStringSafePost);
            }
            else
            {
                DD.mov(500, 370);
                DD.DoMouseDoubleClick();
                DD.DoBackSpace();
                DD.str(request.Account);
                Thread.Sleep(5);

                DD.mov(500, 420);
                DD.DoMouseDoubleClick();
                DD.DoBackSpace();
                DD.str(request.Password);
                Thread.Sleep(5);

                InvokeEncryptScript(request, m_SyncContext, SaveEncryptStringSafePost);
            }
        }
        private void InvokeEncryptScript(GuangZhouGjjRequestEntity request, SynchronizationContext m_SyncContext, SendOrPostCallback SaveEncryptStringSafePost)
        {
            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (GuangZhouGjjRequestEntity)requestObj;
            var encryptStr = this.GetEncryptString(request.SRand);
            Log4netAdapter.WriteInfo(string.Format("GuangZhouGjjBizImpl 加密信息为： Account ==> {0}, {1}", request.Account, encryptStr));
            RedisHelper.SetCache(request.Token, encryptStr, redisEncryptPackage);
        }
    }
}
