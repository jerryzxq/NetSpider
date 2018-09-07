using System; 
using System.IO; 
using System.Text;

namespace VVcredit.ExtTrade.CommonLayer
{
    class LogRecorder
    {
        private const string LogFileName = "Log.txt";

        #region Write
        public static void Write(string msg)
        {
            msg = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "]" + msg + Environment.NewLine;
            WriteToFile(msg, false);
        }

        public static void Write(string msg, Exception e)
        {
            try
            {
                while (e.InnerException != null) e = e.InnerException;

                msg = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "]" + msg + Environment.NewLine + e.Message + e.StackTrace + Environment.NewLine;

                WriteToFile(msg, true);
            }
            catch
            {
            }
        }
        #endregion 

        #region WriteToFile
        private static void WriteToFile(string msg, bool isError)
        {
            try
            {
                string dic = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(dic))
                {
                    Directory.CreateDirectory(dic);
                }
                //一天一个日志
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".Log";
                fileName = Path.Combine(dic, fileName);

                System.IO.File.AppendAllText(fileName, msg, Encoding.GetEncoding("utf-8"));
                Console.WriteLine(msg);
            }
            catch { }

        }
        #endregion
    }
}
