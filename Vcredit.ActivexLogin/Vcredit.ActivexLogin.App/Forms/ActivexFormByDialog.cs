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
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.App
{
    public partial class ActivexFormByDialog : Form
    {
        public ActivexFormByDialog()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            InitFormPage();

            Thread thread1 = new Thread(() =>
            {
                ProcessMQ();
            });
            //thread1.SetApartmentState(ApartmentState.STA); // ProcessMQ 中 while 循环 线程设置成 STA 会有内存问题
            thread1.Start();
        }

        private void InitFormPage()
        {
            this.WindowState = FormWindowState.Normal;

            //定义一个MenuItem数组，并把此数组同时赋值给ContextMenu对象 
            MenuItem[] mnuItms = new MenuItem[3];
            mnuItms[0] = new MenuItem();
            mnuItms[0].Text = "显示窗口";
            mnuItms[0].Click += new EventHandler(this.notifyIcon1_showfrom);

            mnuItms[1] = new MenuItem("-");

            mnuItms[2] = new MenuItem();
            mnuItms[2].Text = "退出系统";
            mnuItms[2].Click += new EventHandler(this.ExitSelect);
            mnuItms[2].DefaultItem = true;
            var notifyiconMnu = new ContextMenu(mnuItms);

            //最小化托盘代码
            this.notifyIcon1 = new NotifyIcon(this.components);
            notifyIcon1.Icon = new Icon("Images/my.ico");
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += NotifyIcon1_MouseDoubleClick;
            notifyIcon1.ContextMenu = notifyiconMnu;
            this.SizeChanged += new EventHandler(this.Form2_SizeChanged);
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.ShowInTaskbar = true;
            }
        }

        private void ExitSelect(object sender, EventArgs e)
        {
            //隐藏托盘程序中的图标 
            notifyIcon1.Visible = false;
            //关闭系统 
            this.Close();
            this.Dispose(true);
        }

        private void notifyIcon1_showfrom(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void ProcessMQ()
        {
            var sites = Constants.GetShowSites();
            var requestStr = "";
            //ShowSite loginform = null;
            WebSiteBizTemplate biz;
            Action<ProjectEnums.WebSiteType, string> invokeAction = new Action<ProjectEnums.WebSiteType, string>(ProcessChildSite);
            while (true)
            {
                Thread.Sleep(50);
                foreach (var siteType in sites)
                {
                    try
                    {
                        biz = BusinessFactory.GenerateSiteBizV2(siteType);
                        requestStr = RedisHelper.Dequeue<string>(biz.REDIS_QUEUE_PACKAGE);
                    }
                    catch (Exception ex)
                    {
                        Log4netAdapter.WriteError("消费队列出现错误", ex);
                    }
                    if (!string.IsNullOrEmpty(requestStr))
                    {
                        //使用Invoke方法委托主线程，执行invokeAction方法
                        this.Invoke(invokeAction, siteType, requestStr);
                    }
                }
            };
        }

        private void ProcessChildSite(ProjectEnums.WebSiteType siteType, string requestStr)
        {
            Log4netAdapter.WriteInfo("加密请求参数为： ==> " + requestStr);

            var loginform = new SiteDialog();
            try
            {
                loginform.InitParam(siteType, requestStr);
                loginform.Show();
                loginform.Activate();
                while (loginform.loading)
                {
                    Application.DoEvents(); //等待本次加载完毕才执行下次循环.
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("ShowSite_页面处理异常", (ex));
            }
            finally
            {
                loginform.Close();
                loginform.Dispose();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Dispose();
            this.Close();
            Environment.Exit(Environment.ExitCode);
        }

    }
}
