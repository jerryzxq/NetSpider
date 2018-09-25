using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vcredit.ActivexLogin.App.Tools;
using Vcredit.ActivexLogin.Common;
using Vcredit.Common.Utility;

namespace Vcredit.ActivexLogin.App
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            // 添加非UI上的异常.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Constants.IsDialog)
                Application.Run(new ActivexFormByDialog());
            else
                Application.Run(new ActivexForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                Log4netAdapter.WriteError("应用程序出错", ex);
            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show(" Error",
                        " Could not write the error to the log. Reason: "
                        + exc.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }
        }
    }
}
