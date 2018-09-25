using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.Entity.Bank;
using Vcredit.ActivexLogin.Entity.Credit;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.App.Business.Credit
{
    /// <summary>
    /// 人行网络版征信
    /// </summary>
    [RestraintSite(TargetWebSite = ProjectEnums.WebSiteType.RenHangNetWorkCredit)]
    public class RenHangNetWorkCreditBizImpl : WebSiteBizTemplate
    {
        public RenHangNetWorkCreditBizImpl() : base()
        {
        }
        protected override void InitCurrentEntity()
        {
            CURRENT_WEBSITE = new WebSiteEntity
            {
                SiteName = "人行网络版征信",
                Url = "https://ipcrs.pbccrc.org.cn/page/login/loginreg.jsp",
                JsFileName = "credit/RenHangNetWorkCredit.js"
            };
        }

        private int offsetLeft = 0;
        private int offsetTop = 0;

        public override void DoWork(string requestStr,
                                    SynchronizationContext m_SyncContext,
                                    SendOrPostCallback SaveEncryptStringSafePost,
                                    SendOrPostCallback InvokeScriptSafePost)
        {
            var request = JsonConvert.DeserializeObject<RenHangNetWorkCreditRequestEntity>(requestStr);

            var isInvoke = false;
            do
            {
                if (!isInvoke)
                {
                    m_SyncContext.Post(InvokeScriptSafePost, new object[] { siteType, request });
                    isInvoke = true;
                }
            } while (this.offsetLeft <= 0 && this.offsetTop <= 0);

            Log4netAdapter.WriteInfo(this.offsetLeft.ToString());
            Log4netAdapter.WriteInfo(this.offsetTop.ToString());

            DD.mov(this.offsetLeft, this.offsetTop);
            DD.DoMouseDoubleClick();
            DD.DoBackSpace(1);
            Thread.Sleep(100);
            DD.str(request.Password);
            Thread.Sleep(20);

            m_SyncContext.Post(SaveEncryptStringSafePost, new object[] { siteType, request });
        }

        public override void InvokeScriptSafePost(object requestObj)
        {
            this.GetOffSetPosition();
        }

        private void GetOffSetPosition()
        {
            //1. 执行js获取密码框焦点位置
            object[] objs = new object[] { };
            var obj = CURRENT_WEBBROWSER_DOCUMENT.InvokeScript("getPasswordPosition", objs);
            var positionJsonStr = obj.ToString();
            var jObj = JObject.Parse(positionJsonStr);
            this.offsetLeft = jObj.SelectToken("offsetLeft").ToString().ToInt(0) + 30;
            this.offsetTop = jObj.SelectToken("offsetTop").ToString().ToInt(0) + 20 + 40;
        }

        public override void SaveEncryptString(object requestObj)
        {
            var request = (RenHangNetWorkCreditRequestEntity)requestObj;
            var encryptString = GetEncryptString(request.RandomKey);
            //保存密码参数
            RedisHelper.SetCache(request.Token, encryptString, redisEncryptPackage);
        }

    }
}
