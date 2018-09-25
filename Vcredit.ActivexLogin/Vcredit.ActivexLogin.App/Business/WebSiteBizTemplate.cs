using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vcredit.ActivexLogin.Entity;
using Vcredit.ActivexLogin.App.Tools;
using System.Configuration;
using Vcredit.Common.Utility;
using System.Reflection;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using mshtml;
using System.Threading;
using static Vcredit.ActivexLogin.Common.ProjectEnums;

namespace Vcredit.ActivexLogin.App.Business
{
    public abstract class WebSiteBizTemplate
    {
        #region Properties

        protected WebSiteType siteType;

        /// <summary>
        /// 上次输入密码长度（后续退格）
        /// </summary>
        protected int _PreAccountLength = 0;

        /// <summary>
        /// 上次输入密码长度（后续退格）
        /// </summary>
        protected int _PrePwdLength = 0;

        /// <summary>
        /// 
        /// </summary>
        protected static Random rand = new Random();

        /// <summary>
        /// 默认密码获取方法名称
        /// </summary>
        protected static readonly string default_script_fun = "getEncrypt";

        /// <summary>
        /// redis 包前缀
        /// </summary>
        protected string redisPackage;

        /// <summary>
        /// 加密后 redis 包前缀
        /// </summary>
        protected string redisEncryptPackage;

        /// <summary>
        /// 当前 webbrowser HtmlDocument
        /// </summary>
        public HtmlDocument CURRENT_WEBBROWSER_DOCUMENT;

        /// <summary>
        /// DD
        /// </summary>
        public static CDD DD;

        /// <summary>
        /// 当前站点实体
        /// </summary>
        public WebSiteEntity CURRENT_WEBSITE;

        /// <summary>
        /// 消息队列 redis 包
        /// </summary>
        public string REDIS_QUEUE_PACKAGE;

		/// <summary>
		/// cookie redis 包前缀
		/// </summary>
		public string redisCookiesPackage;
		#endregion

		public WebSiteBizTemplate()
        {
            siteType = ((this.GetType().GetCustomAttribute(typeof(RestraintSiteAttribute)) as RestraintSiteAttribute).TargetWebSite);
            redisPackage = siteType.ToString();
            redisEncryptPackage = redisPackage + Constants.REQEUST_ENCRYPT_PREFIX;
            REDIS_QUEUE_PACKAGE = redisPackage + Constants.REQEUST_QUEUE_PREFIX;
			redisCookiesPackage= redisPackage + Constants.REQEUST_COOKIE_PREFIX;

			InitCurrentEntity();

			InitDD();
        }

        /// <summary>
        /// 初始化参数 webbrowser_DocumentComplete 候执行
        /// </summary>
        /// <param name="doc"></param>
        public virtual void Init(HtmlDocument doc)
        {
            CURRENT_WEBBROWSER_DOCUMENT = doc;

            AddScriptToPage(doc, "common.js");
            AddScriptToPage(doc, CURRENT_WEBSITE.JsFileName);
        }
        private void InitDD()
        {
            if (DD == null)
            {
                DD = new CDD();
                DD.LoadDllFile(ConfigurationManager.AppSettings["DDPath"]);
            }
        }

        /// <summary>
        /// webbrowser html 注入javascript（获取加密信息js） 
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <param name="jsfileName"></param>
        public void AddScriptToPage(HtmlDocument htmlDocument, string jsfileName)
        {
			if (String.IsNullOrEmpty(jsfileName))
				return;

			var jsText = string.Empty;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", jsfileName);
            if (File.Exists(path))
                jsText = File.ReadAllText(path);

            if (string.IsNullOrEmpty(jsText))
                return;

            var ele = htmlDocument.CreateElement("script");
            if (ele == null) return;

            ele.SetAttribute("type", "text/javascript");
            ele.SetAttribute("text", jsText);
            if (htmlDocument.Body != null)
                htmlDocument.Body.AppendChild(ele);
        }

        /// <summary>
        /// 加密操作主方法（各个site 自己实现）
        /// </summary>
        /// <param name="request"></param>
        public abstract void DoWork(string request, 
            SynchronizationContext m_SyncContext, 
            SendOrPostCallback SaveEncryptStringSafePost,
            SendOrPostCallback InvokeScriptSafePost);

        /// <summary>
        /// GetEncryptStringByIHTMLWindow2
        /// </summary>
        /// <param name="func_param"></param>
        /// <returns></returns>
        public string GetEncryptStringByIHTMLWindow2(string func_param)
        {
            var result = string.Empty;
            try
            {
                // 说明 直接执行页面注入的js无法执行
                // 采用下面的方式执行 js 获取密码
                var jsScript = "";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", CURRENT_WEBSITE.JsFileName);
                if (File.Exists(path))
                    jsScript = File.ReadAllText(path);

                var getEncryptScript = jsScript + default_script_fun + "('" + func_param + "');";
                IHTMLWindow2 win = (IHTMLWindow2)CURRENT_WEBBROWSER_DOCUMENT.Window.DomWindow;
                // win.execScript 返回值为 null；将密码保存到隐藏域，再获取隐藏域的值，从而获取密码
                win.execScript(getEncryptScript, "Javascript");
                IHTMLDocument2 doc2 = win.document;
                IHTMLDocument3 doc3 = (IHTMLDocument3)doc2;
                HTMLInputElement domEle = (HTMLInputElement)doc3.getElementById("hd_encrypt_string");
                if (domEle != null)
                {
                    Log4netAdapter.WriteInfo(string.Format("{0} 加密信息为： encrypt_string ==> {1}", redisPackage, domEle.value));

                    var removeScript = jsScript + "removeElement()";
                    win.execScript(removeScript, "Javascript");

                    result = domEle.value;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("获取加密密码出现异常", ex);
            }
            return result;
        }

        public string GetEncryptString(string func_param)
        {
            // 调用JavaScript的 getEncrypt 方法，并传入参数
            object[] objs = new object[] { func_param };
            var obj = CURRENT_WEBBROWSER_DOCUMENT.InvokeScript(default_script_fun, objs);
            var str = obj.ToString();
            return str;
        }

        /// <summary>
        /// 执行js
        /// </summary>
        /// <param name="requestObj"></param>
        public abstract void InvokeScriptSafePost(object requestObj);

        /// <summary>
        /// 获取加密信息并保存
        /// </summary>
        /// <param name="requestObj"></param>
        public abstract void SaveEncryptString(object requestObj);

		/// <summary>
		/// 初始化当前站点实体
		/// </summary>
		protected abstract void InitCurrentEntity();
    }
}
