using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Vcredit.Common.Constants;

namespace Vcredit.Common.Utility
{

    /// 检查数据是否合法
    /// </summary>
    public class Chk
    {
        private Chk()
        {
            //
            // 这个类提供静态功能函数，禁止构造实例
            //
        }

        /// <summary>
        /// 检查是否为空
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static bool IsDbNull(object objValue)
        {
            try
            {
                return Convert.IsDBNull(objValue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string DBNull2Str(object objValue)
        {
            if (IsDbNull(objValue))
            {
                return "";
            }
            else
            {
                return objValue.ToString();
            }
        }

        public static bool DBNull2Bit(object objValue)
        {
            bool Default = false;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (bool)objValue;
            }
        }

        public static bool DBNull2Bit(object objValue, bool fDefault)
        {
            bool Default = fDefault;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (bool)objValue;
            }
        }

        public static int DBNull2Int(object objValue)
        {
            int Default = 0;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (int)objValue;
            }
        }

        public static int DBNull2TinyInt(object objValue)
        {
            int Default = 0;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return Convert.ToInt32(objValue);
            }
        }


        public static int DBNull2Int(object objValue, int iDefault)
        {
            int Default = iDefault;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (int)objValue;
            }
        }

        public static Decimal DBNull2Decimal(object objValue)
        {
            Decimal Default = 0;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (Decimal)objValue;
            }
        }

        public static Decimal DBNull2Decimal(object objValue, Decimal iDefault)
        {
            Decimal Default = iDefault;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (Decimal)objValue;
            }
        }

        public static Double DBNull2Double(object objValue)
        {
            Double Default = 0;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return Convert.ToDouble(objValue);
            }
        }

        public static Double DBNull2Double(object objValue, Double iDefault)
        {
            Double Default = iDefault;

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return Convert.ToDouble(objValue);
            }
        }


        public static DateTime DBNull2Date(object objValue)
        {
            DateTime Default = Convert.ToDateTime(Consts.DefaultNullDate);

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (DateTime)objValue;
            }
        }

        public static DateTime DBNull2Date(object objValue, string dDefault)
        {
            DateTime Default = Convert.ToDateTime(dDefault);

            if (IsDbNull(objValue))
            {
                return Default;
            }
            else
            {
                return (DateTime)objValue;
            }
        }



        /// <summary>
        /// 检查是否是有效的日期
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static bool IsDate(object objValue)
        {
            try
            {
                Convert.ToDateTime(objValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否是数字
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static bool IsNumeric(object objValue)
        {
            try
            {
                Convert.ToDecimal(objValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int Null2Int(object objValue)
        {
            return Null2Int(objValue, 0);
        }

        public static int Null2Int(object objValue, int iDefault)
        {
            int Default = iDefault;

            if (objValue == null || objValue.ToString() == string.Empty)
            {
                return Default;
            }
            else
            {
                return Convert.ToInt32(objValue);
            }
        }

        public static Decimal Null2Decimal(object objValue)
        {
            return Null2Decimal(objValue, 0);
        }

        public static Decimal Null2Decimal(object objValue, Decimal iDefault)
        {
            Decimal Default = iDefault;

            if (objValue == null || objValue.ToString() == string.Empty)
            {
                return Default;
            }
            else
            {
                return Convert.ToDecimal(objValue);
            }
        }


        public static Double Null2Double(object objValue)
        {
            return Null2Double(objValue, 0);
        }

        public static Double Null2Double(object objValue, Double iDefault)
        {
            Double Default = iDefault;

            if (objValue == null || objValue.ToString() == string.Empty)
            {
                return Default;
            }
            else
            {
                return Convert.ToDouble(objValue);
            }
        }

        public static string IsNull(Object ob)
        {
            if (ob == null)
            {
                return string.Empty;
            }
            return ob.ToString();
        }

        public static int IsNullInt(Object ob)
        {
            if (ob == null || ob.ToString() == string.Empty)
            {
                return 0;
            }
            return int.Parse(ob.ToString());
        }

        public static byte IsNullByte(Object ob)
        {
            if (ob == null || ob.ToString() == string.Empty)
            {
                return 0;
            }
            return Convert.ToByte(ob.ToString());
        }

        public static DateTime IsNullDate(Object ob)
        {
            DateTime Default = Convert.ToDateTime(Consts.DefaultNullDate);
            try
            {
                if (ob == null || ob.ToString() == string.Empty)
                    return Default;

                return Convert.ToDateTime(ob);
            }
            catch
            {
                return Default;
            }
        }

        /// <summary>
        /// 检查远程主机是否能Ping通
        /// </summary>
        /// <param name="sHost"></param>
        /// <returns></returns>
        public static bool CmdPing(string sHost)
        {
            try
            {
                bool IsLink;
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                string sPingrst = string.Empty;
                process.StartInfo.Arguments = "ping " + sHost + " -n 1";
                process.Start();
                process.StandardInput.AutoFlush = true;
                string temp = "ping " + sHost + " -n 1";
                process.StandardInput.WriteLine(temp);
                process.StandardInput.WriteLine("exit");
                string strRst = process.StandardOutput.ReadToEnd();
                if (strRst.IndexOf("(0% loss)") != -1)
                {
                    sPingrst = "连接";
                    IsLink = true;
                }
                else if (strRst.IndexOf("Destination host unreachable") != -1)
                {
                    sPingrst = "无法到达目的主机";
                    IsLink = false;
                }
                else if (strRst.IndexOf("Request timed out") != -1)
                {
                    sPingrst = "超时";
                    IsLink = false;
                }
                else if (strRst.IndexOf("Unknown host") != -1)
                {
                    sPingrst = "无法解析主机";
                    IsLink = false;
                }
                else
                {
                    sPingrst = strRst;
                    IsLink = false;
                }
                process.Close();
                //return sPingrst;
                return IsLink;
            }
            catch (Exception ex)
            {
                //return ex.ToString();
                return false;
            }
        }
    }

}
