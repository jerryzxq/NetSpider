using System;
using System.Collections;
using System.Linq;
using System.Net;

namespace Vcredit.ExtTrade.CommonLayer.helper
{
    public class CookieHelper
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
                            cookTemp.Expires = nameValuePairTemp[1] != string.Empty ? DateTime.Parse(nameValuePairTemp[1]) : DateTime.MinValue;
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

                if (string.IsNullOrEmpty(currentCc.Name) || string.IsNullOrEmpty(currentCc.Value))
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
    }
}
