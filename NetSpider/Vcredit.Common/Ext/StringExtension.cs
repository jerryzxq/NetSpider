using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
namespace Vcredit.Common.Ext
{
    /// <summary>
    /// 对string的扩展
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 将字符串转为DateTime?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回DateTime，否则返回null</returns>
        public static DateTime? ToDateTime(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            DateTime v;
            if (!DateTime.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为DateTime，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回DateTime，否则返回null</returns>
        public static DateTime ToDateTime(this string s, DateTime defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            DateTime v;
            if (!DateTime.TryParse(s, out v)) return defaultValue;

            return v;
        }
        public static DateTime? ToDateTime(this string s, string format)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            DateTime v;
            if (!DateTime.TryParseExact(s, format, null, System.Globalization.DateTimeStyles.None, out v)) return null;

            return v;
        }
        /// <summary>
        /// 将字符串转为decimal?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回decimal，否则返回null</returns>
        public static decimal? ToDecimal(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            decimal v;
            if (!decimal.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为decimal，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回decimal，否则返回null</returns>
        public static decimal ToDecimal(this string s, decimal defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            decimal v;
            if (!decimal.TryParse(s, out v)) return defaultValue;

            return v;
        }

        /// <summary>
        /// 将字符串转为long?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回long，否则返回null</returns>
        public static long? ToLong(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            long v;
            if (!long.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为long，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回long，否则返回null</returns>
        public static long ToLong(this string s, long defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            long v;
            if (!long.TryParse(s, out v)) return defaultValue;

            return v;
        }

        /// <summary>
        /// 将字符串转为int?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回int，否则返回null</returns>
        public static int? ToInt(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            int v;
            if (!int.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为int，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回int，否则返回null</returns>
        public static int ToInt(this string s, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            int v;
            if (!int.TryParse(s, out v)) return defaultValue;

            return v;
        }

        /// <summary>
        /// 将字符串转为short?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回short，否则返回null</returns>
        public static short? ToShort(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            short v;
            if (!short.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为short，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回short，否则返回null</returns>
        public static short ToShort(this string s, short defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            short v;
            if (!short.TryParse(s, out v)) return defaultValue;

            return v;
        }

        /// <summary>
        /// 将字符串转为byte?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回byte，否则返回null</returns>
        public static byte? ToByte(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            byte v;
            if (!byte.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为Byte，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回Byte，否则返回null</returns>
        public static byte ToByte(this string s, byte defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            byte v;
            if (!byte.TryParse(s, out v)) return defaultValue;

            return v;
        }

        /// <summary>
        /// 将字符串转为bool?
        /// </summary>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回bool，否则返回null</returns>
        public static bool? ToBool(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            bool v;
            if (!bool.TryParse(s, out v)) return null;

            return v;
        }

        /// <summary>
        /// 将字符串转为bool，如无法转换则返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回bool，否则返回null</returns>
        public static bool ToBool(this string s, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;

            bool v;
            if (!bool.TryParse(s, out v)) return defaultValue;

            return v;
        }

        /// <summary>
        /// 将字符串转为枚举并返回枚举的整型值
        /// </summary>
        /// <typeparam name="T">必须为enum类型</typeparam>
        /// <param name="s"></param>
        /// <returns>如果可以被成功转换则返回int，否则返回null</returns>
        public static int? ToEnumValue<T>(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            try
            {
                return (int)Enum.Parse(typeof(T), s);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// 将字符串转为枚举并返回枚举的整型值
        /// </summary>
        /// <typeparam name="T">必须为enum类型</typeparam>
        /// <param name="s"></param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回int，否则返回null</returns>
        public static int ToEnumValue<T>(this string s, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            try
            {
                return (int)Enum.Parse(typeof(T), s);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 将字符串转为枚举并返回枚举的整型值
        /// </summary>
        /// <typeparam name="T">必须为enum类型</typeparam>
        /// <param name="s"></param>
        /// <param name="ignoreCase">true 为忽略大小写；false 为考虑大小写。</param>
        /// <returns>如果可以被成功转换则返回int，否则返回null</returns>
        public static int? ToEnumValue<T>(this string s, bool ignoreCase)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            try
            {
                return (int)Enum.Parse(typeof(T), s, ignoreCase);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// 将字符串转为枚举并返回枚举的整型值
        /// </summary>
        /// <typeparam name="T">必须为enum类型</typeparam>
        /// <param name="s"></param>
        /// <param name="ignoreCase">true 为忽略大小写；false 为考虑大小写。</param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns>如果可以被成功转换则返回int，否则返回null</returns>
        public static int ToEnumValue<T>(this string s, bool ignoreCase, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            try
            {
                return (int)Enum.Parse(typeof(T), s, ignoreCase);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 判断字符串是否为空或null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string s)
        {

            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// 流转对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T DeserializeXML<T>(this string content)
        {
            T obj = default(T);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                obj = (T)new XmlSerializer(typeof(T)).Deserialize(new StreamReader(ms, Encoding.UTF8));
            }
            return obj;
        }
        /// <summary>
        /// 字符串转BASE64
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToBase64(this string s)
        {
            if (s.IsEmpty())
                return s;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
        /// <summary>
        /// unicode编码
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToUnicode(this string s)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(s))
            {
                for (int i = 0; i < s.Length; i++)
                {
                    strResult.Append("\\u");
                    strResult.Append(((int)s[i]).ToString("x"));
                }
            }
            return strResult.ToString();
        }
        /// <summary>
        /// unicode解码
        /// </summary>
        /// <param name="s"></param>
        /// <param name="numberSytle"></param>
        /// <returns></returns>
        public static string FromUnicode(this string s, System.Globalization.NumberStyles numberSytle = System.Globalization.NumberStyles.AllowParentheses)
        {
            //最直接的方法Regex.Unescape(str);
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(s))
            {
                string[] strlist = s.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        int charCode = Convert.ToInt32(strlist[i], (int)numberSytle);
                        strResult.Append((char)charCode);
                    }
                }
                catch (FormatException ex)
                {
                    return Regex.Unescape(s);
                }
            }
            return strResult.ToString();
        }
        /// <summary>
        /// UrlEncode编码
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToUrlEncode(this string s, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                string t = s[i].ToString();
                if (t == " ")
                {
                    stringBuilder.Append("%20");
                    continue;
                }
                string k = HttpUtility.UrlEncode(t, encoding);
                if (t == k)
                {
                    stringBuilder.Append(t);
                }
                else
                {
                    stringBuilder.Append(k.ToUpper());
                }
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// ToUrlDecode解码
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToUrlDecode(this string s, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return HttpUtility.UrlDecode(s, encoding);
        }
        /// <summary>
        /// 削减字符，默认削减空格
        /// </summary>
        /// <param name="s"></param>
        /// <param name="trimStr">自定义字符</param>
        /// <returns></returns>
        public static string ToTrim(this string s, string trimStr = " ")
        {
            if (s.IsEmpty())
                return s;
            return s.Replace(trimStr, "");
        }
        /// <summary>
        /// 转换身份证为18位
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string IDCardTo18(this string s)
        {
            if (s.IsEmpty())
                return s;

            s = s.ToTrim();

            if (s.Length != 15 && s.Length != 17)
                return s;


            if (s.Length == 15)
            {
                s = s.Substring(0, 6) + "19" + s.Substring(6, 9);
            }
            int iS = 0;


            int[] iW = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };//加权因子常数
            string LastCode = "10X98765432";//校验码常数
            string newIDCard = s;//新身份证号

            //进行加权求和
            for (int i = 0; i < 17; i++)
            {
                iS += int.Parse(newIDCard.Substring(i, 1)) * iW[i];
            }

            //取模运算，得到模值
            int iY = iS % 11;
            //从LastCode中取得以模为索引号的值，加到身份证的最后一位，即为新身份证号。
            newIDCard += LastCode.Substring(iY, 1);
            return newIDCard;
        }

        /// <summary>
        /// js中的CharAt()方法
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string CharAt(this string s, int index)
        {
            if ((index >= s.Length) || (index < 0))
                return "";
            return s.Substring(index, 1);
        }
        /// <summary>
        /// 从字符串取所有数字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<decimal> GetNumerFromString(this string s)
        {
            var matches = Regex.Matches(s, @"\d+");
            List<decimal> result = new List<decimal>();
            for (var i = 0; i < matches.Count; i++)
            {
                result.Add(matches[i].ToString().ToDecimal().Value);
            }
            return result;

        }

        /// <summary>
        /// 字符串进行unicode编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUnicodeString(this string s)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        strResult.Append("\\u");
                        strResult.Append(((int)s[i]).ToString("x"));
                    }
                }
                catch (FormatException ex)
                {
                    return Regex.Unescape(s);
                }
            }
            return strResult.ToString();
        }
        /// <summary>
        /// unicode编码转正常字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FromUnicodeString(this string s)
        {
            //最直接的方法Regex.Unescape(str);
            StringBuilder strResult = new StringBuilder();
            if (s.IndexOf("\\u") == -1)
            {
                return s;
            }
            if (!string.IsNullOrEmpty(s))
            {
                string[] strlist = s.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        int charCode = Convert.ToInt32(strlist[i], 16);
                        strResult.Append((char)charCode);
                    }
                }
                catch (FormatException ex)
                {
                    return Regex.Unescape(s);
                }
            }
            return strResult.ToString();
        }
    }
}
