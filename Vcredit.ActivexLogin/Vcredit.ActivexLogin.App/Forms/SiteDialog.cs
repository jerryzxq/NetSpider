using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vcredit.ActivexLogin.App.Business;
using Vcredit.ActivexLogin.Common;
using Vcredit.Common.Utility;
using static Vcredit.ActivexLogin.Common.ProjectEnums;

namespace Vcredit.ActivexLogin.App
{
    public partial class SiteDialog : Form
    {
        internal bool loading = true;
        private WebSiteBizTemplate biz;
        private bool webbrowser_isComplete = false;
        private string requestStr = string.Empty;
		private WebSiteType currentSiteType;
		private int counter = 0;

		/// <summary>
		/// UI线程的同步上下文
		/// </summary>
		SynchronizationContext m_SyncContext = null;
        public SiteDialog()
        {
			counter = 0;
			InitializeComponent();
            //获取UI线程同步上下文
            m_SyncContext = SynchronizationContext.Current;
            InitForm();
        }

        private void InitForm()
        {
            this.Text = "控件破解";
            this.tabControl1.Dock = DockStyle.Fill;
            //// 隐藏tab标签代码
            //this.tabControl1.Appearance = TabAppearance.FlatButtons;
            //this.tabControl1.SizeMode = TabSizeMode.Fixed;
            //this.tabControl1.ItemSize = new Size(0, 1);
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = Constants.DoTopMost;
        }

        internal void InitParam(WebSiteType siteType, string request)
        {
            requestStr = request;
            biz = BusinessFactory.GenerateSiteBizV2(siteType);
			currentSiteType = siteType;

			InitWebBrowser(siteType);
            DoMainWork();
        }

        private void InitWebBrowser(WebSiteType siteType)
        {
            var jsoObj = JObject.Parse(requestStr);
            var urlParamObj = jsoObj.SelectToken("$..UrlParam");

            var tabPage = new TabPage();
            tabPage.Name = this.GetNameBySiteType(siteType);
            tabPage.Text = (siteType).ToString();

            var webBrowser = new WebBrowser();
            webBrowser.Name = this.GetNameBySiteType(siteType);
            webBrowser.Navigate(biz.CURRENT_WEBSITE.Url + urlParamObj.ToString());
			webBrowser.Dock = DockStyle.Fill;
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            webBrowser.ScriptErrorsSuppressed = true;

            tabPage.Controls.Add(webBrowser);
            this.tabControl1.TabPages.Add(tabPage);
        }

        private void InvokeScriptSafePost(object state)
        {
            var objs = (object[])state;
            var requestObj = objs[1];
            biz.InvokeScriptSafePost(requestObj);
        }

        private void SaveEncryptStringSafePost(object state)
        {
            var objs = (object[])state;

            //var siteType = (WebSiteType)objs[0];
            var requestObj = objs[1];
            biz.SaveEncryptString(requestObj);

            loading = false;
        }

        private string GetNameBySiteType(WebSiteType siteType)
        {
            return ((int)siteType).ToString();
        }

        private bool CheckIsCompleted(WebBrowser webBrowser, WebBrowserDocumentCompletedEventArgs e)
        {
            string browserUrl = webBrowser.Url.ToString();
            // 检查未赋值或空值
            if (String.IsNullOrEmpty(browserUrl))
                return false;
            // 是否为空白页
            if (browserUrl.Equals("about:blank"))
                return false;
            // 状态为完成
            if (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                return false;
            // 检查事件url和webBrowser的url
            if (e.Url.ToString() != browserUrl)
                return false;
            if (webBrowser.DocumentText == "")
                return false;
			
			return true;
        }

		/// <summary>
		/// 宁波银行加载完成判断
		/// </summary>
		private bool CheckIsCompleted_NB(WebBrowser webBrowser, WebBrowserDocumentCompletedEventArgs e)
		{
			counter++;
			if (5 == counter)
			{
				return true;
			}
			return false;
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var webBrowser = sender as WebBrowser;
            try
            {
				var iscmp = false;
				switch (currentSiteType)
				{
					case WebSiteType.NbcBank:
						iscmp = CheckIsCompleted_NB(webBrowser, e);
						break;
					default:
						iscmp = CheckIsCompleted(webBrowser, e);
						break;
				}
				Log4netAdapter.WriteInfo("CheckIsCompleted is ==>"+ iscmp);
				if (iscmp)
                {
                    biz.Init(webBrowser.Document);
                    webbrowser_isComplete = true;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("WebBrowser_DocumentCompleted 出现异常"), ex);
            }
        }

        private void DoMainWork()
        {
            var thread1 = new Thread(() =>
            {
                WaitBrowserComplete();
                BusinessDoWork();
            });
            thread1.Start();
        }

        private void BusinessDoWork()
        {
            try
            {
                if (!string.IsNullOrEmpty(requestStr))
                {
                    biz.DoWork(requestStr, m_SyncContext, SaveEncryptStringSafePost, InvokeScriptSafePost);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("DoWork出现错误", ex);
            }
        }

        private void WaitBrowserComplete()
        {
            while (!webbrowser_isComplete)
            {
                Thread.Sleep(50);
            }
        }
    }
}
