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
using Newtonsoft.Json.Linq;
using Vcredit.ActivexLogin.Common;
using mshtml;
using System.IO;
using Vcredit.ActivexLogin.Attributes;
using System.Reflection;

namespace Vcredit.ActivexLogin.App.Business
{
    /// <summary>
    /// 武汉公积金
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.WuHanGjj)]
    public class WuHanGjjBizImpl : WebSiteBizTemplate
    {
        public WuHanGjjBizImpl() : base()
        { }
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteId = 4,
                SiteName = "武汉公积金",
                Url = "https://whgjj.hkbchina.com/portal/pc/login.html",
                JsFileName = "wuhan_gjj.js"
            };
        }

        public override void DoWork(string requestStr, 
            SynchronizationContext m_SyncContext , 
            SendOrPostCallback SaveEncryptStringSafePost,
            SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<WuHanGjjRequestEntity>(requestStr);

            // activex 控件可能有时间限制，这里双击密码框并删除，目前可以解决问题
            DD.mov(980, 376);
            Thread.Sleep(rand.Next(100, 210));
            DD.DoMouseDoubleClick();
            Thread.Sleep(rand.Next(100, 210));
            DD.DoBackSpace(1, rand.Next(160, 180));
            DD.str(request.Password);
            Thread.Sleep(5);

            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (WuHanGjjRequestEntity)requestObj;
            var encryptString = GetEncryptStringByIHTMLWindow2(CommonFun.GetTimeStamp());
            var str = new { LoginPasswordObj = encryptString, LoginId = request.Account };
            RedisHelper.SetCache(request.Token, JsonConvert.SerializeObject(str), redisEncryptPackage);
        }

        public override void InvokeScriptSafePost(object requestObj)
        {
            throw new NotImplementedException();
        }
    }
}
