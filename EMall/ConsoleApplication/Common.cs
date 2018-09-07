using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ConsoleApplication
{
    public static class Common
    {
        public static Image NowImg;
        public static string uuid;
        public static string codeAns;
        public static string hidden;
        public static string hidden2;
        //unicode转换中文
        public static string UnitoChse(string str)
        {
            string outStr = "";
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        //将unicode字符转为10进制整数，然后转为char中文字符  
                        outStr += (char)int.Parse(strlist[i], System.Globalization.NumberStyles.HexNumber);
                    }
                }
                catch (FormatException ex)
                {
                    outStr = ex.Message;
                }
                return outStr;
            }
            else
            {
                return "";
            }
        }
        //取出中间文本
        public static string QuChuZhongJian(string YuanWenBen, string QianWenBen, string HouWenBen)
        {
            int a = YuanWenBen.IndexOf(QianWenBen);
            if (a >= 0)
            {
                int b = YuanWenBen.Substring(a + QianWenBen.Length).IndexOf(HouWenBen);
                if (b >= 0)
                {
                    string j = YuanWenBen.Substring(a + QianWenBen.Length, b);
                    return j;
                }
            }
            return "";
        }
        //为真时获取10位时间戳,为假时获取13位时间戳
        public static string GetTimeStamp(bool bflag)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string ret = string.Empty;
            if (bflag)
                ret = Convert.ToInt64(ts.TotalSeconds).ToString();
            else
                ret = Convert.ToInt64(ts.TotalMilliseconds).ToString();

            return ret;
        }
    }
}
