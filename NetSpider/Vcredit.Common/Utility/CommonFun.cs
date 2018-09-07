/*********************************************  
 * * 功能描述： 有用的公共函数
 * * 创 建 人:  张志博
 * * 日    期:  2014/9/19
 * * 修 改 人:  
 * * 修改日期: 
 * * 修改描述:  
 * * 版    本:  1.0
 * *******************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;

namespace Vcredit.Common.Utility
{
    public class CommonFun
    {
        public static string GetGuidID()
        {
            //return "54820f25e87443acb4e8f08f1b0e4c18";
            string guid = Guid.NewGuid().ToString();
            guid = guid.Replace("-", "").ToLower();
            return guid;
        }
        /// <summary>
        /// 计算字符串表达式的通用类。
        /// 引用Microsoft.JScript.Eval和Microsoft.JScript.Vsa.VsaEngine来实现公式计算方法
        /// </summary>
        /// <param name="strequation">字符串表达式</param>
        /// <param name="paras">传入的参数</param>
        /// <returns></returns>
        public static object EvaluateFormulaExpression(string strequation, Dictionary<string, string> paras)
        {
            string repara = "repara";

            StringBuilder myCode = new StringBuilder();
            myCode.Append("function getValue() : double {");
            foreach (KeyValuePair<string, string> item in paras)
            {
                myCode.Append("var " + item.Key + " = " + item.Value + " ;");
            }
            myCode.Append("var " + repara + "=0;  with (Math){ ");
            myCode.Append(repara + "=" + strequation + "};");
            myCode.Append("  return " + repara + ";");
            myCode.Append("}");

            try
            {
                object ret = Microsoft.JScript.Eval.JScriptEvaluate(myCode + "getValue()",
                    Microsoft.JScript.Vsa.VsaEngine.CreateEngine());
                return ret;
            }
            catch
            {
                return "false";
            }
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <returns>创建的对象，失败返回 null</returns>
        public static object CreateObject(string AssemblyName, string ClassName)
        {
            object obj = null;
            try
            {
                Type objType = Type.GetType(ClassName + "," + AssemblyName, false);
                if (objType != null)
                {
                    obj = Activator.CreateInstance(objType);
                }
            }
            catch (Exception)
            {
            }
            return obj;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <returns>创建的对象，失败返回 null</returns>
        public static object CreateObject(string AssemblyName, string ClassName, object[] objs)
        {
            object obj = null;
            try
            {
                Type objType = Type.GetType(ClassName + "," + AssemblyName, false);
                if (objType != null)
                {
                    obj = Activator.CreateInstance(objType, objs);
                }
            }
            catch (Exception)
            {
            }
            return obj;
        }

        #region 过滤html标签
        public static string StripHTML(string stringToStrip)
        {
            stringToStrip = Regex.Replace(stringToStrip, "</p(?:\\s*)>(?:\\s*)<p(?:\\s*)>", "\n\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = Regex.Replace(stringToStrip, "", "\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = Regex.Replace(stringToStrip, "\"", "''", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = StripHtmlXmlTags(stringToStrip);
            return stringToStrip;
        }
        private static string StripHtmlXmlTags(string content)
        {
            return Regex.Replace(content, "<[^>]+>", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        #endregion

        #region 取中间字符串
        public static string GetMidStr(string Source, string StartStr, string EndStr)
        {
            try
            {
                if (String.IsNullOrEmpty(Source))
                {
                    return "";
                }
                Source = ClearFlag(Source);
                int StartPos = Source.IndexOf(StartStr, 0) + StartStr.Length;
                int EndPos;
                if (String.IsNullOrEmpty(EndStr))
                {
                    EndPos = Source.Length;
                }
                else
                {
                    EndPos = Source.IndexOf(EndStr, StartPos);
                }
                if (EndPos < StartPos)
                {
                    return "";
                }
                return Source.Substring(StartPos, EndPos - StartPos);
            }
            catch { return ""; }
        }
        public static string GetMidStrByRegex(string Source, string StartStr, string EndStr, bool IsWithStart = false)
        {
            string retStr = string.Empty;
            try
            {
                //string strCut = string.Format("(?<test>{0}).*?(?={1})", StartStr, EndStr);
                string strCut = string.Format("(?<={0}).*?(?={1})", StartStr, EndStr);
                if (!String.IsNullOrEmpty(Source))
                {
                    Regex re = new Regex(strCut, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    MatchCollection mc = re.Matches(CommonFun.ClearFlag(Source));
                    if (mc.Count > 0)
                    {
                        retStr = mc[0].Value;
                        if (!IsWithStart)
                        {
                            retStr = retStr.Replace(StartStr, "");
                        }
                    }
                }
                return retStr;
            }
            catch { return ""; }
        }

        public static string GetMidStrByRegex(string Source, string StartStr, bool IsWithStart = false)
        {
            string retStr = string.Empty;
            try
            {
                //string strCut = string.Format("(?<test>{0}).*?(?={1})", StartStr, EndStr);
                string strCut = string.Format("(?<={0}).*", StartStr);
                if (!String.IsNullOrEmpty(Source))
                {
                    Regex re = new Regex(strCut, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    MatchCollection mc = re.Matches(CommonFun.ClearFlag(Source));
                    if (mc.Count > 0)
                    {
                        retStr = mc[0].Value;
                        if (!IsWithStart)
                        {
                            retStr = retStr.Replace(StartStr, "");
                        }
                    }
                }
                return retStr;
            }
            catch { return ""; }
        }


        #endregion

        #region 字符串倒序输出
        public static string ReverseStr(string text)
        {
            return new string(text.ToCharArray().Reverse().ToArray());
        }
        #endregion

        /// <summary>
        /// 正则表达式转义
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RegexReplaceTrans(string str)
        {
            if (str == "" || str == null)
                return "";

            string conStr = "";
            if (Regex.IsMatch(str, @"[\$\*\[\]\?\\\(\)]"))
            {
                Regex re = new Regex(@"\\", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\\");
                re = null;

                re = new Regex(@"\$", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\$");
                re = null;

                //re = new Regex(@"\.", RegexOptions.IgnoreCase);
                //str = re.Replace(str, @"\.");
                //re = null;

                re = new Regex(@"\*", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\*");
                re = null;

                re = new Regex(@"\[", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\[");
                re = null;

                re = new Regex(@"\]", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\]");
                re = null;

                re = new Regex(@"\?", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\?");
                re = null;

                re = new Regex(@"\(", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\(");
                re = null;

                re = new Regex(@"\)", RegexOptions.IgnoreCase);
                str = re.Replace(str, @"\)");
                re = null;

                conStr = str;

            }
            else
            {
                conStr = str;
            }
            return conStr;
        }
        /// <summary>
        /// 将字符串中的字符（转义的）替换为XML格式字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TransStringToXml(string str)
        {
            if (str == "" || str == null)
                return "";

            string conStr = "";
            if (Regex.IsMatch(str, "['\"<>&]"))
            {
                Regex re = new Regex("&", RegexOptions.IgnoreCase);
                str = re.Replace(str, "&amp;");
                re = null;

                re = new Regex("<", RegexOptions.IgnoreCase);
                str = re.Replace(str, "&lt;");
                re = null;

                re = new Regex(">", RegexOptions.IgnoreCase);
                str = re.Replace(str, "&gt;");
                re = null;

                re = new Regex("'", RegexOptions.IgnoreCase);
                str = re.Replace(str, "&apos;");
                re = null;

                re = new Regex("\"", RegexOptions.IgnoreCase);
                str = re.Replace(str, "&quot;");
                re = null;
                conStr = str;

            }
            else
            {
                conStr = str;
            }
            return conStr;
        }
        /// <summary>
        /// 将XML字符串中的字符替换为常规字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TransXmlToString(string str)
        {

            str = str.Replace("&lt;", "<");

            str = str.Replace("&gt;", ">");

            str = str.Replace("&apos;", "&apos;");

            str = str.Replace("&quot;", "\"");

            str = str.Replace("&amp;", "&");

            return str;

        }
        //去除字符串的回车换行符号
        /// <summary>
        /// 去除字符串的回车换行符号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ClearFlag(string str)
        {
            str = Regex.Replace(str, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            str = Regex.Replace(str, "\\n", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            str.Replace(@"\r\n", "");
            return str;
        }
        /// <summary>
        /// 获取访问服务器的客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            string userIP = string.Empty;

            try
            {
                if (System.Web.HttpContext.Current == null || System.Web.HttpContext.Current.Request == null || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return userIP;

                //CDN加速后取到的IP   
                userIP = System.Web.HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(userIP))
                {
                    return userIP;
                }

                userIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];


                if (!String.IsNullOrEmpty(userIP))
                    return userIP;

                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    userIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (userIP == null)
                        userIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    userIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                if (string.Compare(userIP, "unknown", true) == 0)
                    return System.Web.HttpContext.Current.Request.UserHostAddress;
                return userIP;
            }
            catch { }

            return userIP;
        }
        /// <summary>
        /// cookie合并
        /// </summary>
        /// <param name="OldCookies"></param>
        /// <param name="NewCookies"></param>
        /// <returns></returns>
        public static CookieCollection GetCookieCollection(CookieCollection OldCookies, CookieCollection NewCookies)
        {
            if (NewCookies == null)
            {
                NewCookies = new CookieCollection();
            }
            if (OldCookies != null)
            {
                for (int i = 0; i < OldCookies.Count; i++)
                {
                    if (NewCookies.Count > 0)
                    {
                        for (int j = 0; j < NewCookies.Count; j++)
                        {
                            if (NewCookies[j].Name != OldCookies[i].Name)
                            {
                                NewCookies.Add(OldCookies[i]);
                            }
                        }
                    }
                    else
                    {
                        NewCookies.Add(OldCookies[i]);
                    }
                }
            }

            return NewCookies;
        }

        /// <summary>
        /// 合并CookieString
        /// </summary>
        /// <param name="oldCookieStr">旧cookie字符串</param>
        /// <param name="newCookieStr">新cookie字符串</param>
        /// <returns></returns>
        public static string GetCookieStringNew(string oldCookieStr, string newCookieStr)
        {
            string retCookie = string.Empty;
            Dictionary<string, string> cookieDics = new Dictionary<string, string>();
            string[] strArr = null;
            string[] oldCookieArr = null;
            string[] newCookieArr = null;
            try
            {
                if (newCookieStr.IsEmpty())
                {
                    return oldCookieStr;
                }
                //旧cookie初始化
                if (!oldCookieStr.IsEmpty())
                {
                    oldCookieArr = oldCookieStr.Split(';');
                    foreach (string strItem in oldCookieArr)
                    {
                        if (!strItem.IsEmpty())
                        {
                            strArr = strItem.Split('=');
                            if (strArr.Length < 2)
                            {
                                continue;
                            }
                            if (strArr.Length > 2)
                            {
                                strArr[1] = strItem.Replace(strArr[0] + "=", "");
                            }
                            if (cookieDics.Keys.Contains(strArr[0]))
                            {
                                cookieDics.Remove(strArr[0]);
                                cookieDics.Add(strArr[0], strArr[1]);
                            }
                            else
                            {
                                cookieDics.Add(strArr[0], strArr[1]);
                            }
                        }

                    }
                }
                string tempStr = string.Empty;
                newCookieArr = newCookieStr.Split(';');
                foreach (string strItem in newCookieArr)
                {
                    if (strItem.ToLower().IndexOf("domain") == -1 && (!strItem.ToLower().StartsWith("expires") && !strItem.ToLower().EndsWith("gmt")))
                    {
                        tempStr = strItem.ToTrim("path=/;");
                        tempStr = strItem.ToTrim("Path=/;");
                        tempStr = tempStr.ToTrim("path=/,");
                        tempStr = tempStr.ToTrim("Path=/,");
                        tempStr = tempStr.ToTrim("path=/");
                        tempStr = tempStr.ToTrim("Path=/");
                        tempStr = tempStr.ToTrim("HttpOnly,");
                        tempStr = tempStr.ToTrim("httponly,");
                        tempStr = tempStr.ToTrim("secure,");
                        tempStr = tempStr.ToTrim("Secure,");
                        if (strItem.ToLower().IndexOf("expires") != -1)
                        {
                            tempStr = strItem.Substring(strItem.ToLower().IndexOf(",") + 1);
                            if (tempStr.ToLower().IndexOf(",") != -1)
                            {
                                tempStr = tempStr.Substring(tempStr.ToLower().IndexOf(",") + 1);
                                strArr = tempStr.Split('=');
                                if (strArr.Length < 2)
                                {
                                    continue;
                                }
                                if (strArr.Length > 2)
                                {
                                    string key2 = strArr[1].Substring(strArr[1].ToLower().LastIndexOf(",") + 1);
                                    strArr[1] = strArr[1].Remove(strArr[1].ToLower().LastIndexOf(","));
                                    if (key2 != "")
                                    {
                                        if (cookieDics.Keys.Contains(key2))
                                        {
                                            cookieDics.Remove(key2);
                                        }
                                        cookieDics.Add(key2, strArr[2]);
                                    }

                                }
                            }
                            else
                                continue;
                        }
                        strArr = tempStr.Split('=');
                        if (strArr.Length < 2)
                        {
                            continue;
                        }
                        if (strArr.Length > 2)
                        {
                            strArr[1] = tempStr.Replace(strArr[0] + "=", "");
                        }
                        if (cookieDics.Keys.Contains(strArr[0]))
                        {
                            cookieDics.Remove(strArr[0]);
                            cookieDics.Add(strArr[0], strArr[1]);
                        }
                        else
                        {
                            cookieDics.Add(strArr[0], strArr[1]);
                        }
                    }
                }
                foreach (KeyValuePair<string, string> dic in cookieDics)
                {
                    retCookie += dic.Key + "=" + dic.Value + ";";
                }
            }
            catch (Exception)
            {

            }
            return retCookie;
        }

        /// <summary>
        /// 合并CookieString
        /// </summary>
        /// <param name="oldCookieStr">旧cookie字符串</param>
        /// <param name="newCookieStr">新cookie字符串</param>
        /// <returns></returns>
        public static string GetCookieString(string oldCookieStr, string newCookieStr)
        {
            string retCookie = string.Empty;
            Dictionary<string, string> cookieDics = new Dictionary<string, string>();
            string[] strArr = null;
            string[] oldCookieArr = null;
            string[] newCookieArr = null;
            try
            {
                if (newCookieStr.IsEmpty())
                {
                    return oldCookieStr;
                }
                //旧cookie初始化
                if (!oldCookieStr.IsEmpty())
                {
                    oldCookieArr = oldCookieStr.Split(';');
                    foreach (string strItem in oldCookieArr)
                    {
                        if (!strItem.IsEmpty())
                        {
                            strArr = strItem.Split('=');
                            if (strArr.Length < 2)
                            {
                                continue;
                            }
                            cookieDics.Add(strArr[0], strArr[1]);
                        }

                    }
                }
                string tempStr = string.Empty;
                newCookieArr = newCookieStr.Split(';');
                foreach (string strItem in newCookieArr)
                {
                    if (strItem.IndexOf("domain") == -1 && strItem.IndexOf("expires") == -1)
                    {
                        tempStr = strItem.ToTrim("path=/;");
                        tempStr = tempStr.ToTrim("path=/,");
                        tempStr = tempStr.ToTrim("Path=/,");
                        tempStr = tempStr.ToTrim("path=/");
                        tempStr = tempStr.ToTrim("HttpOnly,");
                        tempStr = tempStr.ToTrim("secure,");
                        strArr = tempStr.Split('=');
                        if (strArr.Length < 2)
                        {
                            continue;
                        }
                        if (cookieDics.Keys.Contains(strArr[0]))
                        {
                            cookieDics.Remove(strArr[0]);
                            cookieDics.Add(strArr[0], strArr[1]);
                        }
                        else
                        {
                            cookieDics.Add(strArr[0], strArr[1]);
                        }

                    }
                }
                foreach (KeyValuePair<string, string> dic in cookieDics)
                {
                    retCookie += dic.Key + "=" + dic.Value + ";";
                }
            }
            catch (Exception)
            {

            }
            return retCookie;
        }
        /// <summary>
        /// cookie合并
        /// </summary>
        /// <param name="OldCookies"></param>
        /// <param name="NewCookies"></param>
        /// <returns></returns>
        public static CookieCollection GetCookieCollectionEQ(CookieCollection OldCookies, CookieCollection NewCookies)
        {
            if (NewCookies == null)
            {
                NewCookies = new CookieCollection();
            }
            if (NewCookies.Count > 0)
            {

                for (int j = 0; j < NewCookies.Count; j++)
                {
                    if (OldCookies[NewCookies[j].Name] != null)
                    {
                        OldCookies[NewCookies[j].Name].Value = NewCookies[j].Value;
                    }
                    else
                    {
                        OldCookies.Add(NewCookies[j]);
                    }
                }
            }
            return OldCookies;
        }
        /// <summary>
        /// 删除特定的cookie
        /// </summary>
        /// <param name="OldCookies"></param>
        /// <param name="CookieName"></param>
        /// <returns></returns>
        public static CookieCollection RemoveCookie(CookieCollection OldCookies, string CookieName)
        {
            CookieCollection NewCookies = new CookieCollection();
            if (OldCookies != null)
            {
                for (int i = 0; i < OldCookies.Count; i++)
                {
                    if (OldCookies[i].Name != CookieName)
                        NewCookies.Add(OldCookies[i]);
                }
            }

            return NewCookies;
        }
        public static string GetVercodeBase64(byte[] imagebyte)
        {
            if (imagebyte == null || imagebyte.Count() == 0) return String.Empty;
            return Convert.ToBase64String(imagebyte);
        }
        /// <summary>
        /// 得到2个日期的指定格式间隔
        /// </summary>
        /// <param name="dt1">日期1</param>
        /// <param name="dt2">日期2</param>
        /// <param name="dateformat">间隔格式: y:年 M:月 d:天 h:时 m:分 s:秒 fff:毫秒</param>
        /// <returns>间隔 long型</returns>
        public static int GetIntervalOf2DateTime(DateTime dt1, DateTime dt2, string dateformat)
        {
            try
            {
                int interval = 0;
                //int interval = dt1.Ticks - dt2.Ticks;
                //DateTime dt11;
                //DateTime dt22;

                switch (dateformat)
                {
                    case "fff"://毫秒
                        //interval /= 10000;
                        interval = (dt1 - dt2).Milliseconds;
                        break;
                    case "s"://秒
                        //interval /= 10000000;
                        interval = (dt1 - dt2).Seconds;
                        break;
                    case "m"://分钟
                        //interval /= 600000000;
                        interval = (dt1 - dt2).Hours;
                        break;
                    case "h"://小时
                        //interval /= 36000000000;
                        interval = (dt1 - dt2).Hours;
                        break;
                    case "d"://天
                        //interval /= 864000000000;
                        interval = (dt1 - dt2).Days;
                        break;
                    case "M"://月
                        //dt11 = (dt1.CompareTo(dt2) >= 0) ? dt2 : dt1;
                        //dt22 = (dt1.CompareTo(dt2) >= 0) ? dt1 : dt2;
                        //interval = -1;
                        //while (dt22.CompareTo(dt11) >= 0)
                        //{
                        //    interval++;
                        //    dt11 = dt11.AddMonths(1);
                        //}
                        interval = (dt1.Year - dt2.Year) * 12 + dt1.Month - dt2.Month;
                        break;
                    case "y"://年
                        //dt11 = (dt1.CompareTo(dt2) >= 0) ? dt2 : dt1;
                        //dt22 = (dt1.CompareTo(dt2) >= 0) ? dt1 : dt2;
                        //interval = -1;
                        //while (dt22.CompareTo(dt11) >= 0)
                        //{
                        //    interval++;
                        //    dt11 = dt11.AddMonths(1);
                        //}
                        //interval /= 12;
                        interval = (dt1.Year - dt2.Year);
                        break;
                }
                return interval;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// 生成图片链接
        /// </summary>
        /// <param name="token">图片名称</param>
        /// <param name="Postfix">图片后缀</param>
        /// <returns></returns>
        public static string GetVercodeUrl(string token, string Postfix = ".jpg")
        {
            string url = string.Empty;
            try
            {
                url = AppSettings.localUrl + "/files/vercode/" + token + Postfix;
            }
            catch (Exception)
            { }
            return url;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 01, 01, 00, 00, 00, 0000);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
        /// <summary>
        /// MD5　32位加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMd5Str(string ConvertString)
        {
            string pwd = string.Empty;
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(ConvertString));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 

                pwd = pwd + s[i].ToString("x2");

            }
            return pwd;
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="encode">加密采用的编码方式</param>
        /// <param name="source">待加密的明文</param>
        /// <returns></returns>
        public static string GetEncodeBase64(Encoding encode, string source)
        {
            string decode = string.Empty;
            byte[] bytes = encode.GetBytes(source);
            try
            {
                decode = Convert.ToBase64String(bytes);
            }
            catch
            {
                decode = source;
            }
            return decode;
        }

        /// <summary>
        /// 获取省份简称
        /// </summary>
        /// <param name="mobile">省份名称</param>
        /// <returns>省份简称</returns>
        public static string GetProvinceCode(string province)
        {
            string region = string.Empty;
            switch (province)
            {
                case "北京": region = "BJ"; break;
                case "上海": region = "SH"; break;
                case "天津": region = "TJ"; break;
                case "重庆": region = "CQ"; break;
                case "山东": region = "SD"; break;
                case "安徽": region = "AH"; break;
                case "福建": region = "FJ"; break;
                case "甘肃": region = "GS"; break;
                case "广东": region = "GD"; break;
                case "广西": region = "GX"; break;
                case "贵州": region = "GZ"; break;
                case "海南": region = "HI"; break;
                case "河北": region = "HE"; break;
                case "河南": region = "HA"; break;
                case "黑龙江": region = "HL"; break;
                case "湖北": region = "HB"; break;
                case "湖南": region = "HN"; break;
                case "吉林": region = "JL"; break;
                case "江苏": region = "JS"; break;
                case "江西": region = "JX"; break;
                case "辽宁": region = "LN"; break;
                case "内蒙古": region = "NM"; break;
                case "宁夏": region = "NX"; break;
                case "青海": region = "QH"; break;
                case "山西": region = "SX"; break;
                case "陕西": region = "SN"; break;
                case "四川": region = "SC"; break;
                case "西藏": region = "XZ"; break;
                case "新疆": region = "XJ"; break;
                case "云南": region = "YN"; break;
                case "浙江": region = "ZJ"; break;
                case "澳门": region = "MO"; break;
                case "香港": region = "HK"; break;
                case "台湾": region = "TW"; break;
                default: break;
            }
            return region;
        }

        /// <summary>
        /// 获取省份简称
        /// </summary>
        /// <param name="mobile">省份名称</param>
        /// <returns>省份简称</returns>
        public static string GetProvinceName(string provinceCode)
        {
            string region = string.Empty;
            switch (provinceCode)
            {
                case "BJ": region = "北京"; break;
                case "SH": region = "上海"; break;
                case "TJ": region = "天津"; break;
                case "CQ": region = "重庆"; break;
                case "SD": region = "山东"; break;
                case "AH": region = "安徽"; break;
                case "FJ": region = "福建"; break;
                case "GS": region = "甘肃"; break;
                case "GD": region = "广东"; break;
                case "GX": region = "广西"; break;
                case "GZ": region = "贵州"; break;
                case "HI": region = "海南"; break;
                case "HE": region = "河北"; break;
                case "HA": region = "河南"; break;
                case "HL": region = "黑龙江"; break;
                case "HB": region = "湖北"; break;
                case "HN": region = "湖南"; break;
                case "JL": region = "吉林"; break;
                case "JS": region = "江苏"; break;
                case "JX": region = "江西"; break;
                case "LN": region = "辽宁"; break;
                case "NM": region = "内蒙古"; break;
                case "NX": region = "宁夏"; break;
                case "QH": region = "青海"; break;
                case "SX": region = "山西"; break;
                case "SN": region = "陕西"; break;
                case "SC": region = "四川"; break;
                case "XZ": region = "西藏"; break;
                case "XJ": region = "新疆"; break;
                case "YN": region = "云南"; break;
                case "ZJ": region = "浙江"; break;
                case "MO": region = "澳门"; break;
                case "HK": region = "香港"; break;
                case "TW": region = "台湾"; break;
                default: break;
            }
            return region;
        }

        #region 手机实名认证判断
        /// <summary>
        /// 校验是否实名
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <param name="opIdCard">待校验身份证号</param>
        /// <param name="name">姓名</param>
        /// <param name="opName">待校验姓名</param>
        /// <returns></returns>
        public static bool IsAuth(string idCard, string opIdCard, string name, string opName)
        {
            bool result_check_id_card = true;
            bool result_check_name = true;
            #region
            #region 大小写转换
            if (!idCard.IsEmpty())
            {
                idCard = idCard.ToUpper().ToTrim();
            }
            if (!opIdCard.IsEmpty())
            {
                opIdCard = opIdCard.ToUpper().ToTrim();
            }
            #endregion
            #region 过滤空格
            if (!name.IsEmpty())
            {
                name = name.ToTrim();
            }
            if (!opName.IsEmpty())
            {
                opName = opName.ToTrim();
            }
            #endregion
            #region 广东移动，身份证号为“未身份确认”问题处理
            if (opIdCard == "未身份确认")
            {
                opIdCard = "";
            }
            #endregion
            #endregion
            if (!opIdCard.IsEmpty() && !opName.IsEmpty())
            {
                result_check_id_card = CheckIdCardPartlyMatch(idCard, opIdCard);
                result_check_name = CheckNamePartlyMatch(name, opName);
            }
            else if (!opName.IsEmpty())
            {
                result_check_name = CheckNamePartlyMatch(name, opName);
                result_check_id_card = true;
            }
            else if (!opIdCard.IsEmpty())
            {
                result_check_name = true;
                result_check_id_card = CheckIdCardPartlyMatch(idCard, opIdCard);
            }
            else
            {
                result_check_name = false;
                result_check_id_card = false;
            }

            if (result_check_name && result_check_id_card)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 校验身份证号是否实名
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <param name="opIdCard">待校验身份证号</param>
        /// <returns></returns>
        private static bool CheckIdCardPartlyMatch(string idCard, string opIdCard)
        {
            bool is_math = true;
            if (idCard.IsEmpty() || opIdCard.IsEmpty())
                return false;
            string pattern = @"(\d|X|x)";
            string pattern1 = @"(\d)";
            if (!Regex.IsMatch(opIdCard, pattern))
                return false;
            string[] start_num = Regex.Split(opIdCard, pattern, RegexOptions.IgnoreCase);
            string[] result = list_rm_null(start_num);
            string mask_rules = string.Empty;
            if (result.Length > 0)
            {
                foreach (string each_num in result)
                {
                    if (Regex.IsMatch(each_num, pattern1))
                        mask_rules += each_num;
                    else if ((each_num.Contains("X") || each_num.Contains("x")) && result[result.Count() - 1] == each_num)
                        mask_rules += each_num;
                    else
                        mask_rules += ".+?";
                }
                is_math = Regex.IsMatch(idCard, mask_rules + "$");
            }

            return is_math;
        }

        /// <summary>
        /// 校验姓名是否实名
        /// </summary>
        /// <param name="name">姓名</param>
        /// <param name="opName">待校验姓名</param>
        /// <returns></returns>
        private static bool CheckNamePartlyMatch(string name, string opName)
        {
            bool is_math = true;
            if (name.IsEmpty() || opName.IsEmpty())
                return false;
            string pattern = @"([\u4e00-\u9fa5])";
            string pattern1 = @"([\u4e00-\u9fa5])";
            if (!Regex.IsMatch(opName, pattern1))
                return false;
            string[] start_num = Regex.Split(opName, pattern, RegexOptions.IgnoreCase);
            string[] result = list_rm_null(start_num);
            string mask_rules = string.Empty;
            if (result.Length > 0)
            {
                foreach (string each_one in result)
                {
                    var search_result = Regex.IsMatch(each_one, pattern1);
                    if (search_result)
                        mask_rules += each_one;
                    else
                        mask_rules += ".+?";
                }
                is_math = Regex.IsMatch(name, mask_rules);
            }

            return is_math;
        }

        private static string[] list_rm_null(string[] list_param)
        {
            List<string> no_null_list = new List<string>();
            if (list_param.Length > 0)
            {
                foreach (string str in list_param)
                {
                    if (!str.IsEmpty())
                    {
                        no_null_list.Add(str);
                    }
                }
            }
            return no_null_list.ToArray();
        }

        #endregion

        /// <summary> 
        /// 发送邮件程序 
        /// </summary> 
        /// <param name="subject">标题</param> 
        /// <param name="body">内容</param> 
        /// <param name="fujian">附件</param> 
        /// <param name="mailPriority">邮件级别</param>
        /// <returns>ok</returns> 
        public static string SendMail(string subject, string body, string fujian = "", MailPriority mailPriority = MailPriority.Normal)
        {
            var server = ConfigurationManager.AppSettings["SenderServerHost"];
            var port = ConfigurationManager.AppSettings["port"].ToInt(25);
            var from = ConfigurationManager.AppSettings["SenderUsername"];
            var password = ConfigurationManager.AppSettings["SenderPassword"];
            var recipients = ConfigurationManager.AppSettings["Recipients"];
            return SendMail(subject, body, server, port, from, password, recipients, true, 10000, fujian, mailPriority);
        }

        /// <summary> 
        /// 发送邮件程序 
        /// </summary> 
        /// <param name="subject">标题</param> 
        /// <param name="body">内容</param> 
        /// <param name="senderServerHost">邮件服务器</param> 
        /// <param name="port">端口</param> 
        /// <param name="senderUsername">发件人</param> 
        /// <param name="senderPassword">密码</param> 
        /// <param name="recipients">收件人</param> 
        /// <param name="isBodyHtml">是否HTML形式发送 </param> 
        /// <param name="timeout">超时时间</param> 
        /// <param name="fujian">附件</param> 
        /// <param name="mailPriority">邮件级别</param>
        /// <returns>ok</returns> 
        public static string SendMail(string subject, string body, string senderServerHost, int port, string senderUsername, string senderPassword, string recipients, bool isBodyHtml = true, int timeout = 10000, string fujian = "", MailPriority mailPriority = MailPriority.Normal)
        {
            try
            {
                var charrecipients = recipients.Split(',');
                MailMessage mail = new MailMessage();//邮件发送类
                mail.From = new MailAddress(senderUsername, senderUsername);//是谁发送的邮件
                //发送给谁 
                foreach (string to in charrecipients)
                    mail.To.Add(to);
                mail.Subject = subject;//标题 
                mail.BodyEncoding = Encoding.Default; //内容编码 
                mail.Priority = mailPriority;//发送优先级
                mail.Body = body;//邮件内容 
                mail.IsBodyHtml = isBodyHtml;//是否HTML形式发送 
                //附件 
                if (fujian.Length > 0)
                    mail.Attachments.Add(new Attachment(fujian));
                SmtpClient smtp = new SmtpClient(senderServerHost, port);//邮件服务器和端口 
                smtp.UseDefaultCredentials = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;//指定发送方式
                smtp.Credentials = new System.Net.NetworkCredential(senderUsername, senderPassword);  //指定登录名和密码 
                smtp.Timeout = timeout;//超时时间 
                smtp.Send(mail);
                return "ok";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }

        /// <summary> 
        /// 将XX时XX分XX秒 转换成秒 
        /// </summary> 
        /// <param name="dateString">日期值</param> 
        /// <returns>秒数</returns> 
        public static int ConvertDate(string dateString)
        {
            try
            {

                var s = Regex.Replace(dateString, "\\D+", ",");
                var numbertList = s.TrimEnd(',').Split(',');
                var totalSecond = 0;
                if (numbertList.Count() == 3)
                {
                    totalSecond = int.Parse(numbertList[0]) * 3600 + int.Parse(numbertList[1]) * 60 + int.Parse(numbertList[2]);
                }
                if (numbertList.Count() == 2)
                {
                    totalSecond = int.Parse(numbertList[0]) * 60 + int.Parse(numbertList[1]);
                }
                if (numbertList.Count() == 1)
                {
                    totalSecond = int.Parse(numbertList[0]);
                }

                return totalSecond;
            }
            catch (Exception exp)
            {
                return 0;
            }
        }


        /// <summary> 
        /// 将XX mbXX kb 转换成kb
        /// </summary> 
        /// <param name="dateString">日期值</param> 
        /// <returns>kb数</returns> 
        public static int ConvertGPRS(string flowString)
        {
            try
            {
                var s = Regex.Replace(flowString, "\\D+", ",");
                var totalFlow = 0;
                var numbertList = s.Split(',');
                if (s.Contains(".00"))
                {
                    numbertList = s.TrimEnd(',').TrimEnd('0').TrimEnd(',').Split(',');
                }
                else
                {
                    numbertList = s.TrimEnd(',').Split(',');
                }

                if (numbertList.Count() == 1)
                {
                    totalFlow += int.Parse(numbertList[0]);
                }
                if (numbertList.Count() == 2)
                {
                    totalFlow += int.Parse(numbertList[0]) * 1024 + int.Parse(numbertList[1]);
                }
                if (numbertList.Count() == 3)
                {
                    totalFlow += int.Parse(numbertList[0]) * 1024 * 1024 + int.Parse(numbertList[1]) * 1024 + int.Parse(numbertList[2]);
                }
                return totalFlow;
            }
            catch (Exception exp)
            {
                return 0;
            }
        }

        #region string转化成GBK
        /// <summary>
        /// string转化成GBK
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ConvertToGbk(string source)
        {
            //转换得到编码的字节流  
            var utf16 = Encoding.Unicode;
            var u16Bytes = utf16.GetBytes(source);

            var gb = Encoding.GetEncoding("gbk");
            var gbytes = Encoding.Convert(utf16, gb, u16Bytes);

            var sb = new StringBuilder();
            for (var i = 0; i < gbytes.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(gbytes[i], 16));
            }
            return (sb.ToString());


            //Encoding utf8 = Encoding.UTF8;  
            //byte[] u8bytes = Encoding.Convert(utf16, utf8, u16bytes);  

            //Encoding b5 = Encoding.GetEncoding("big5");  
            //byte[] bbytes = Encoding.Convert(utf16, b5, u16bytes);  
        }
        #endregion
    }
}
