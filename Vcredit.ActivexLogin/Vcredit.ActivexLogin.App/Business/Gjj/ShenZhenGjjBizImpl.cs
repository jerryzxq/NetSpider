﻿using System;
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
using Vcredit.ActivexLogin.Attributes;
using System.Reflection;

namespace Vcredit.ActivexLogin.App.Business
{
    /// <summary>
    /// 深圳公积金
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.ShenZhenGjj)]
    public class ShenZhenGjjBizImpl : WebSiteBizTemplate
    {
        public ShenZhenGjjBizImpl() : base()
        { }
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteId = 3,
                SiteName = "深圳公积金",
                Url = "https://nbp.szzfgjj.com/newui/login.jsp?transcode=pri",
                JsFileName = "shenzhen_gjj.js"
            };
        }

        public override void DoWork(string requestStr,
            SynchronizationContext m_SyncContext,
            SendOrPostCallback SaveEncryptStringSafePost,
            SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<ShenZhenGjjRequestEntity>(requestStr);

            DD.mov(740, 490);
            DD.DoMouseDoubleClick();
            DD.DoBackSpace();
            DD.str(request.Password);
            Thread.Sleep(5);

            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (ShenZhenGjjRequestEntity)requestObj;
            var str = this.GetEncryptString("");
            Log4netAdapter.WriteInfo(string.Format("ShenZhenGjjBizImpl 加密信息为： Account ==> {0}, {1}", request.Account, str));
            var jObj = JObject.Parse(str);
            jObj.Add("CardNo", request.Account);
            str = jObj.ToString();
            RedisHelper.SetCache(request.Token, str, redisEncryptPackage);

        }

        public override void InvokeScriptSafePost(object requestObj)
        {
            throw new NotImplementedException();
        }
    }
}