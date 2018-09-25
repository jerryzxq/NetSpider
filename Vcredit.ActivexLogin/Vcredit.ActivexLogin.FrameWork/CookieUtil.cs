using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;

namespace Vcredit.ActivexLogin.FrameWork
{
    public class CookieUtil
    {
        #region 将Response字符串格式cookie转化成cookieCollection
        /// <summary>
        /// 将Response字符串格式cookie转化成cookieCollection
        /// </summary>
        /// <param name="strHeader"></param>
        /// <param name="strHost"></param>
        /// <returns></returns>
        public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost = "")
        {
            var cc = new CookieCollection();
            if (strHeader == string.Empty) return cc;

            var al = ConvertCookieHeaderToArrayList(strHeader);
            cc = ConvertCookieArraysToCookieCollection(al, strHost);

            return cc;
        }
        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            var strCookTemp = strCookHeader.Split(',');
            var al = new ArrayList();
            var i = 0;
            var n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                {
                    al.Add(strCookTemp[i]);
                }
                i = i + 1;
            }
            return al;
        }
        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            var cc = new CookieCollection();

            var alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (var i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                var intEachCookPartsCount = strEachCookParts.Length;
                var strCNameAndCValue = string.Empty;
                var strPNameAndPValue = string.Empty;
                string[] nameValuePairTemp;

                var cookTemp = new Cookie();
                for (var j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            var firstEqual = strCNameAndCValue.IndexOf("=", StringComparison.Ordinal);
                            var firstName = strCNameAndCValue.Substring(0, firstEqual);
                            var allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName.Trim();
                            cookTemp.Value = allValue.Trim();
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            nameValuePairTemp = strPNameAndPValue.Split('=');
                            cookTemp.Path = nameValuePairTemp[1] != string.Empty ? nameValuePairTemp[1] : "/";
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            nameValuePairTemp = strPNameAndPValue.Split('=');
                            cookTemp.Domain = nameValuePairTemp[1] != string.Empty ? nameValuePairTemp[1] : strHost;
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("expires", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            nameValuePairTemp = strPNameAndPValue.Split('=');
                            var datetime = DateTime.MinValue;
                            if (nameValuePairTemp[1] != string.Empty)
                            {
                                if (DateTime.TryParse(nameValuePairTemp[1], out datetime))
                                {
                                    cookTemp.Expires = datetime;
                                }
                            }


                        }
                        continue;
                    }

                    cookTemp.HttpOnly = strEachCookParts[j].IndexOf("httponly", StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (cookTemp.Path == string.Empty)
                    cookTemp.Path = "/";
                if (cookTemp.Domain == string.Empty)
                    cookTemp.Domain = strHost;

                var existcookie = cc[cookTemp.Name];
                if (string.IsNullOrEmpty(cookTemp.Value)
                    && existcookie != null
                    && (cookTemp.Domain.Equals(existcookie.Domain))
                    && (cookTemp.Path.Equals(existcookie.Path)))
                {
                    cookTemp.Value = existcookie.Value;
                }

                cc.Add(cookTemp);
            }
            return cc;
        }
        #endregion

        #region 将cookieCollection转化成Request请求格式的cookie（格式为 name=value;name=value;......），注意每个cookie以;(英文分号)隔开
        /// <summary>
        /// 将cookieCollection转化成Request请求格式的cookie（格式为 name=value;name=value;......），注意每个Request cookie以;(英文分号)隔开
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static string ConvertCookieCollectionToRequestCookie(CookieCollection cookies)
        {
            if (cookies == null || cookies.Count <= 0)
                throw new ArgumentException("cookies");

            var result = string.Empty;
            var enumerator = cookies.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentCc = enumerator.Current as Cookie;

                if (currentCc == null)
                    continue;

                if (string.IsNullOrEmpty(currentCc.Name))
                    continue;

                result += string.Format("{0}={1};", currentCc.Name, currentCc.Value);
            }
            return result;
        }
        #endregion

        #region 将Response格式的cookie转化成Request请求格式的cookie（格式为 name=value;name=value;......），注意每个Request cookie以;(英文分号)隔开
        /// <summary>
        /// 将Response格式的cookie转化成Request请求格式的cookie（格式为 name=value;name=value;......），注意每个Request cookie以;(英文分号)隔开
        /// </summary>
        /// <param name="strResponseCookie"></param>
        /// <returns></returns>
        public static string ConvertResponseCookieToRequestCookie(string strResponseCookie)
        {
            if (string.IsNullOrEmpty(strResponseCookie))
                return string.Empty;

            var cc = GetAllCookiesFromHeader(strResponseCookie);

            return ConvertCookieCollectionToRequestCookie(cc);
        }
        #endregion

        #region 将Request格式Cookie（格式为 name=value;name=value;...... 英文逗号分隔）合并，以新的覆盖旧的
        /// <summary>
        /// 将Request格式Cookie（格式为 name=value;name=value;...... 英文逗号分隔）合并，以新的覆盖旧的
        /// </summary>
        /// <param name="oldCookie"></param>
        /// <param name="newCookie"></param>
        /// <returns></returns>
        public static string CookieCombine(string oldCookie, string newCookie)
        {
            if (string.IsNullOrEmpty(oldCookie))
                return newCookie;

            if (string.IsNullOrEmpty(newCookie))
                return oldCookie;

            var oldList = oldCookie.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var newList = newCookie.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var combineList = newCookie.Split(';').ToList();
            foreach (var oldcc in oldList)
            {
                if (string.IsNullOrEmpty(oldcc))
                    continue;

                var exist =
                    newList.FirstOrDefault(x => !string.IsNullOrEmpty(x) && x.Split('=')[0].Trim().Equals(oldcc.Split('=')[0].Trim()));

                if (exist == null)
                    combineList.Add(oldcc);
            }
            combineList = combineList.Where(x => !string.IsNullOrEmpty(x)).ToList();
            return string.Join(";", combineList);
        }
        #endregion

        #region Request请求格式的cookie（格式为 name=value;name=value;......） ===》 HttpCookieCollection
        /// <summary>
        /// Request请求格式的cookie（格式为 name=value;name=value;......） ===》 HttpCookieCollection
        /// </summary>
        /// <param name="strCookie"></param>
        /// <returns></returns>
        public static HttpCookieCollection FillInCookiesCollection(string strCookie)
        {
            // Parse the cookie server variable.
            // Format: c1=k1=v1&k2=v2; c2=...

            int l = (strCookie != null) ? strCookie.Length : 0;
            int i = 0;
            int j;
            char ch;

            HttpCookie lastCookie = null;

            HttpCookieCollection cookieCollection = new HttpCookieCollection();
            while (i < l)
            {
                // find next ';' (don't look to ',' as per 91884)
                j = i;
                while (j < l)
                {
                    ch = strCookie[j];
                    if (ch == ';')
                        break;
                    j++;
                }

                // create cookie form string
                String cookieString = strCookie.Substring(i, j - i).Trim();
                i = j + 1; // next cookie start

                if (cookieString.Length == 0)
                    continue;

                HttpCookie cookie = CreateCookieFromString(cookieString);

                // some cookies starting with '$' are really attributes of the last cookie
                if (lastCookie != null)
                {
                    String name = cookie.Name;

                    // add known attribute to the last cookie (if any)
                    if (name != null && name.Length > 0 && name[0] == '$')
                    {
                        if (EqualsIgnoreCase(name, "$Path"))
                            lastCookie.Path = cookie.Value;
                        else if (EqualsIgnoreCase(name, "$Domain"))
                            lastCookie.Domain = cookie.Value;

                        continue;
                    }
                }

                // regular cookie
                cookieCollection.Add(cookie);
                lastCookie = cookie;

                // goto next cookie
            }

            return cookieCollection;
        }
        private static bool EqualsIgnoreCase(string s1, string s2)
        {
            if (String.IsNullOrEmpty(s1) && String.IsNullOrEmpty(s2))
            {
                return true;
            }
            if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
            {
                return false;
            }
            if (s2.Length != s1.Length)
            {
                return false;
            }
            return 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
        }
        private static bool EqualsIgnoreCase(string s1, int index1, string s2, int index2, int length)
        {
            return String.Compare(s1, index1, s2, index2, length, StringComparison.OrdinalIgnoreCase) == 0;
        }
        public static HttpCookie CreateCookieFromString(String s)
        {
            HttpCookie c = new HttpCookie("");

            int l = (s != null) ? s.Length : 0;
            int i = 0;
            int ai, ei;
            bool firstValue = true;
            int numValues = 1;

            // Format: cookiename[=key1=val2&key2=val2&...]

            while (i < l)
            {
                //  find next &
                ai = s.IndexOf('&', i);
                if (ai < 0)
                    ai = l;

                // first value might contain cookie name before =
                if (firstValue)
                {
                    ei = s.IndexOf('=', i);

                    if (ei >= 0 && ei < ai)
                    {
                        c.Name = s.Substring(i, ei - i);
                        i = ei + 1;
                    }
                    else if (ai == l)
                    {
                        // the whole cookie is just a name
                        c.Name = s;
                        break;
                    }

                    firstValue = false;
                }

                // find '='
                ei = s.IndexOf('=', i);

                if (ei < 0 && ai == l && numValues == 0)
                {
                    // simple cookie with simple value
                    c.Value = s.Substring(i, l - i);
                }
                else if (ei >= 0 && ei < ai)
                {
                    // key=value
                    c.Values.Add(s.Substring(i, ei - i), s.Substring(ei + 1, ai - ei - 1));
                    numValues++;
                }
                else
                {
                    // value without key
                    c.Values.Add(null, s.Substring(i, ai - i));
                    numValues++;
                }

                i = ai + 1;
            }

            return c;
        }
        #endregion

        /// <summary>
        /// 将Response字符串格式cookie转化成cookieCollection
        /// </summary>
        /// <param name="strHeader"></param>
        /// <param name="strHost"></param>
        /// <returns></returns>
        public static CookieCollection GetAllCookies(string cookies, string domain)
        {
            var cc = new CookieCollection();
            if (cookies == string.Empty) return cc;
            var cookesplit = cookies.Split(';');
            for (var i = 0; i < cookesplit.Length; i++)
            {

                if (cookesplit[i].Split('=').Count() > 1)
                {
                    string s1 = cookesplit[i].Substring(0, cookesplit[i].IndexOf("=") + 1);
                    string s2 = cookesplit[i].Substring(cookesplit[i].IndexOf("=") + 1);
                    //var strCookie = cookesplit[i].Split('=');

                    var cookie = new Cookie();
                    cookie.Name = s1.TrimEnd('=').Trim();
                    cookie.Value = s2.Trim();
                    cookie.Domain = domain;
                    cc.Add(cookie);

                }

            }

            return cc;
        }

        #region 根据字符串Cookie获取CookieContainer
        /// <summary>
        /// 将Response字符串格式cookie转化成CookieContainer
        /// </summary>
        /// <param name="strHeader"></param>
        /// <param name="strHost"></param>
        /// <returns></returns>
        public static CookieContainer GetCookieContainerByCookies(string cookies, string domain)
        {
            var cc = new CookieContainer();
            if (cookies == string.Empty) return cc;
            var cookieIEnumber = cookies.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Where(c => c.IndexOf('=') > 0);
            if (cookieIEnumber.Any())
            {
                foreach (var Item in cookieIEnumber.Select(c => new KeyValuePair<string, string>(c.Split('=')[0].Trim(), c.Split('=')[1].Trim())))
                {
                    var cookie = new Cookie();
                    cookie.Name = Item.Key;
                    cookie.Value = Item.Value;
                    cookie.Domain = domain;
                    cc.Add(cookie);
                }
            }

            return cc;
        }

        #endregion 

        #region 根据CookieContainer获取所有的字符串Cookie
        /// <summary>
        /// 将ResponseCookieContainer转化成字符串格式cookie
        /// </summary>
        /// <param name="strHeader"></param>
        /// <param name="strHost"></param>
        /// <returns></returns>
        public static string GetCookieContainerByCookies(CookieContainer cookies)
        {
            return string.Join(";", GetCookieList(cookies).Select(c => c.Name + "=" + c.Value.ToString()));
        }
        #endregion 

        public static List<Cookie> GetCookieList(CookieContainer cookies)
        {
            List<Cookie> listCookies = new List<Cookie>();
            Hashtable table = (Hashtable)cookies.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cookies, new object[] { });

            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) listCookies.Add(c);
            }
            return listCookies;
        }

    }
}
