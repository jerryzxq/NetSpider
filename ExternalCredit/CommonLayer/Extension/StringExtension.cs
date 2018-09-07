using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Vcredit.ExtTrade.CommonLayer
{
    public static class StringExtension
    {
        public static int ToNotNullInt(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return CommonData.defaultNumValue;

            int v;
            if (!int.TryParse(s, out v)) return CommonData.defaultNumValue;

            return v;
        }
        public static decimal ToNotNullDecimal(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return CommonData.defaultNumValue;

            decimal v;
            if (!decimal.TryParse(s, out v)) return CommonData.defaultNumValue;

            return v;
        }
        public static DateTime ToNotNullDateTime(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return CommonData.defaultTime;

            DateTime v;
            if (!DateTime.TryParse(s, out v)) return CommonData.defaultTime;

            return v;
        }
        public static bool IsContianBlankSpace(this string s)
        {
            if (s.IndexOf(" ") >= 0)
                return true;
            return false;
        }
        public static  string DeleteBlankSpace(this string s)
        {
            return s.Replace(" ", "");
        }
        public static bool IsValidIdentity(this  string cardNo)
        {
            Regex regCard = new Regex(@"(^\d{15}$)|(^\d{17}(\d|X)$)"); //正则式，可验证15或18位身份证
            if (regCard.IsMatch(cardNo))  //验证身份证合法性
            {
                if (cardNo.Length == 18)
                {
                    string CheckCode = "10X98765432";
                    //加权因子
                    int[] Factors = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
                    //计算加权和
                    int sum = 0;
                    for (int i = 0; i < 17; i++)
                    {
                        sum += (cardNo[i] - '0') * Factors[i];  //48是‘0’的ascii码
                    }
                    char c = CheckCode[(sum % 11)];
                    if (cardNo[17] != c)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public static bool IsValidChineseWord(this string word)
        {
            char[] c = word.ToCharArray();
            bool res = true;
            for (int i = 0; i < c.Length; i++)
            {
                if (!(c[i] >= 0x4e00 && c[i] <= 0x9fbb))
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
   
    }
}
