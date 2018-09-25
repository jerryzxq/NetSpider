using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Vcredit.ActivexLogin.App.Business;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using static Vcredit.ActivexLogin.Common.ProjectEnums;

namespace Vcredit.ActivexLogin.App
{
    public partial class ActivexForm : Form
    {
        private Dictionary<WebSiteType, bool> completeDic = new Dictionary<WebSiteType, bool>();
        private Dictionary<WebSiteType, WebSiteBizTemplate> webSiteBizDic = new Dictionary<WebSiteType, WebSiteBizTemplate>();

        /// <summary>
        /// UI线程的同步上下文
        /// </summary>
        SynchronizationContext m_SyncContext = null;

        public ActivexForm()
        {
            InitializeComponent();
            //获取UI线程同步上下文
            m_SyncContext = SynchronizationContext.Current;
        }

        private void ActivexLoginFormByShowSite_Load(object sender, EventArgs e)
        {
            this.Text = "控件破解";
            this.tabControl1.Dock = DockStyle.Fill;
            // 隐藏tab标签代码
            //this.tabControl1.Appearance = TabAppearance.FlatButtons;
            //this.tabControl1.SizeMode = TabSizeMode.Fixed;
            //this.tabControl1.ItemSize = new Size(0, 1);
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = Constants.DoTopMost;
            this.FormClosed += ActivexLoginFormByShowSite_FormClosed;

            this.AddWebBrowser();
            this.DoMainWork();
        }

        private void DoMainWork()
        {
            var thread1 = new Thread(() =>
            {
                WaitBrowserComplete();
                DoConsumerQueue();
            });
            thread1.Start();
        }

        /// <summary>
        /// 保存加密后信息
        /// </summary>
        /// <param name="state"></param>
        private void SaveEncryptStringSafePost(object state)
        {
            var objs = (object[])state;

            var siteType = (WebSiteType)objs[0];
            var biz = webSiteBizDic[siteType];
            var requestObj = objs[1];
            biz.SaveEncryptString(requestObj);
        }

        /// <summary>
        /// 保存加密后信息
        /// </summary>
        /// <param name="state"></param>
        private void InvokeScriptSafePost(object state)
        {
            var objs = (object[])state;
            var siteType = (WebSiteType)objs[0];
            var biz = webSiteBizDic[siteType];
            var requestObj = objs[1];
            biz.InvokeScriptSafePost(requestObj);
        }

        /// <summary>
        /// 消费队列
        /// </summary>
        private void DoConsumerQueue()
        {
            WebSiteBizTemplate biz = null;
            string requestStr = null;
            while (true)
            {
                // 请求参数放在不同的消息队列，循环消费处理
                foreach (var siteType in webSiteBizDic.Keys)
                {
                    biz = webSiteBizDic[siteType];
                    try
                    {
                        requestStr = RedisHelper.Dequeue<string>(biz.REDIS_QUEUE_PACKAGE);
                    }
                    catch (Exception ex)
                    {
                        Log4netAdapter.WriteError("消费队列出现错误", ex);
                    }
                    if (!string.IsNullOrEmpty(requestStr))
                    {
                        Log4netAdapter.WriteInfo("加密请求参数为： ==> " + requestStr);

                        m_SyncContext.Post(ActivateForm, null);
                        m_SyncContext.Post(ShowCurrentSitePage, siteType);

                        try
                        {
                            biz.DoWork(requestStr, m_SyncContext, SaveEncryptStringSafePost, InvokeScriptSafePost);
                        }
                        catch (Exception ex)
                        {
                            Log4netAdapter.WriteError("Business DoWork 方法出现错误！", ex);
                        }

                        requestStr = null;
                    }
                }
            }
        }

        /// <summary>
        /// 激活当前窗口
        /// </summary>
        /// <param name="state"></param>
        private void ActivateForm(object state)
        {
            if (this.WindowState != FormWindowState.Maximized)
            {
                this.Show();
                this.WindowState = FormWindowState.Maximized;
                this.Activate();
            }
        }

        /// <summary>
        /// tab页切换
        /// </summary>
        /// <param name="siteTypeObj"></param>
        private void ShowCurrentSitePage(object siteTypeObj)
        {
            var siteType = (WebSiteType)siteTypeObj;
            foreach (TabPage item in this.tabControl1.TabPages)
            {
                if (item.Name == this.GetNameBySiteType(siteType))
                    item.Show();
                else
                    item.Hide();
            }
        }

        /// <summary>
        /// 等待webbrowser全部加载完毕
        /// </summary>
        private void WaitBrowserComplete()
        {
            while (true)
            {
                Thread.Sleep(200);
                try
                {
                    int completeCount = 0;
                    foreach (var item in completeDic.Values)
                    {
                        if (item)
                            completeCount++;
                    }
                    bool allComplete = (completeCount == completeDic.Keys.Count);
                    if (allComplete)
                    {
                        Log4netAdapter.WriteInfo("all webbrowser is completed");
                        break;
                    }
                }
                catch (Exception)
                { }
            }
        }

        private void ActivexLoginFormByShowSite_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }

        /// <summary>
        /// 动态添加webbrowser
        /// </summary>
        private void AddWebBrowser()
        {
            var sites = Constants.GetShowSites();

            foreach (var siteType in sites)
            {
                var biz = BusinessFactory.GenerateSiteBizV2(siteType);

                var tabPage = new TabPage();
                tabPage.Name = this.GetNameBySiteType(siteType);
                tabPage.Text = (siteType).ToString();

                var webBrowser = new WebBrowser();
                webBrowser.Name = this.GetNameBySiteType(siteType);
                webBrowser.Navigate(biz.CURRENT_WEBSITE.Url);
                webBrowser.Dock = DockStyle.Fill;
                webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

                tabPage.Controls.Add(webBrowser);

                this.tabControl1.TabPages.Add(tabPage);

                webSiteBizDic.Add(siteType, biz);
                completeDic.Add(siteType, false);
            }
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

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var webBrowser = sender as WebBrowser;
            try
            {
                var siteType = (WebSiteType)webBrowser.Name.ToInt(0);
                var biz = webSiteBizDic[siteType];
                var iscmp = CheckIsCompleted(webBrowser, e);
                Log4netAdapter.WriteInfo("CheckIsCompleted is ==> " + iscmp);
                if (iscmp)
                {
                    biz.Init(webBrowser.Document);
                    completeDic[siteType] = true;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("SiteType：{0}， WebBrowser_DocumentCompleted 出现异常", webBrowser.Name), ex);
            }
        }

        private string GetNameBySiteType(WebSiteType siteType)
        {
            return ((int)siteType).ToString();
        }

    }
}
