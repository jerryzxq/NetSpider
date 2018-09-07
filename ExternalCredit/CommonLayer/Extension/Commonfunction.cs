using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CommonLayer.Extension
{
    public class Commonfunction
    {
        /// <summary>
        /// 通过身份证号判断年龄是否在18和60周岁之间
        /// </summary>
        /// <param name="IDCard">身份证号</param>
        /// <returns></returns>
        public static bool AgeIsBetween18And60(string IDCard,byte  startAge =18,byte  endAge=60)
        {
            bool result = true;
            var dateStr = GetDateWithIDCard(IDCard);
            DateTime dt = new DateTime();
            if (DateTime.TryParse(dateStr, out dt))
            {
                var nowDate = DateTime.Now;
                var _18Years = nowDate.AddYears(-startAge);
                var _60Years = nowDate.AddYears(-endAge);
                if (dt >= _60Years && dt <= _18Years)
                    result = true;
                else
                    result = false;

            }
            return result;

        } 
        public static string GetDateWithIDCard(string IDCard)
        {
            string BirthDay = " ";
            string strYear;
            string strMonth;
            string strDay;
            if (IDCard.Length == 15)
            {
                strYear = IDCard.Substring(6, 2);
                strMonth = IDCard.Substring(8, 2);
                strDay = IDCard.Substring(10, 2);
                BirthDay = "19" + strYear + "- " + strMonth + "- " + strDay;
            }
            if (IDCard.Length == 18)
            {
                strYear = IDCard.Substring(6, 4);
                strMonth = IDCard.Substring(10, 2);
                strDay = IDCard.Substring(12, 2);
                BirthDay = strYear + "- " + strMonth + "- " + strDay;
            }

            return BirthDay;
        }
        public static string GetMidStr(string Source, string StartStr, string EndStr)
        {
            try
            {
                if (String.IsNullOrEmpty(Source))
                {
                    return "";
                }
                Source = ClearFlag(Source);
                var startIndex = Source.IndexOf(StartStr, 0);
                if (startIndex == -1)
                    return "";
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
        /// 为真时获取10位时间戳,为假时获取13位时间戳
        /// </summary>
        /// <param name="bflag"></param>
        /// <returns></returns>
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
        public static string DecodeFromBase64(string base64str)
        {
            byte[] bpath = Convert.FromBase64String(base64str);
            return System.Text.Encoding.UTF8.GetString(bpath);
        }
        public static string EncodeToBase64(string base64str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(base64str);
            return Convert.ToBase64String(bytes);
        }
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            using (System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }
        /// <summary>
        /// 随机获取字符串
        /// </summary>
        /// <param name="count">获取字符串的长度</param>
        /// <returns></returns>
        /// <summary>
        /// 生成指定长度的随机字符串
        /// </summary>
        /// <param name="intLength">随机字符串长度</param>
        /// <param name="booNumber">生成的字符串中是否包含数字</param>
        /// <param name="booSign">生成的字符串中是否包含符号</param>
        /// <param name="booSmallword">生成的字符串中是否包含小写字母</param>
        /// <param name="booBigword">生成的字符串中是否包含大写字母</param>
        /// <returns></returns>
        public static string GetRandomizer(int intLength, bool booNumber, bool booSign, bool booSmallword, bool booBigword)
        {
            //定义  
            Random ranA = new Random(GetRandomSeed());
            //  Random ranA = new Random();
            int intResultRound = 0;
            int intA = 0;
            string strB = "";

            while (intResultRound < intLength)
            {
                //生成随机数A，表示生成类型  
                //1=数字，2=符号，3=小写字母，4=大写字母  

                intA = ranA.Next(1, 5);

                //如果随机数A=1，则运行生成数字  
                //生成随机数A，范围在0-10  
                //把随机数A，转成字符  
                //生成完，位数+1，字符串累加，结束本次循环  

                if (intA == 1 && booNumber)
                {
                    intA = ranA.Next(0, 10);
                    strB = intA.ToString() + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //如果随机数A=2，则运行生成符号  
                //生成随机数A，表示生成值域  
                //1：33-47值域，2：58-64值域，3：91-96值域，4：123-126值域  

                if (intA == 2 && booSign == true)
                {
                    intA = ranA.Next(1, 5);

                    //如果A=1  
                    //生成随机数A，33-47的Ascii码  
                    //把随机数A，转成字符  
                    //生成完，位数+1，字符串累加，结束本次循环  

                    if (intA == 1)
                    {
                        intA = ranA.Next(33, 48);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=2  
                    //生成随机数A，58-64的Ascii码  
                    //把随机数A，转成字符  
                    //生成完，位数+1，字符串累加，结束本次循环  

                    if (intA == 2)
                    {
                        intA = ranA.Next(58, 65);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=3  
                    //生成随机数A，91-96的Ascii码  
                    //把随机数A，转成字符  
                    //生成完，位数+1，字符串累加，结束本次循环  

                    if (intA == 3)
                    {
                        intA = ranA.Next(91, 97);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=4  
                    //生成随机数A，123-126的Ascii码  
                    //把随机数A，转成字符  
                    //生成完，位数+1，字符串累加，结束本次循环  

                    if (intA == 4)
                    {
                        intA = ranA.Next(123, 127);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                }

                //如果随机数A=3，则运行生成小写字母  
                //生成随机数A，范围在97-122  
                //把随机数A，转成字符  
                //生成完，位数+1，字符串累加，结束本次循环  

                if (intA == 3 && booSmallword == true)
                {
                    intA = ranA.Next(97, 123);
                    strB = ((char)intA).ToString() + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //如果随机数A=4，则运行生成大写字母  
                //生成随机数A，范围在65-90  
                //把随机数A，转成字符  
                //生成完，位数+1，字符串累加，结束本次循环  

                if (intA == 4 && booBigword == true)
                {
                    intA = ranA.Next(65, 89);
                    strB = ((char)intA).ToString() + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
            }
            return strB;

        }

        /// <summary>
        /// 校验身份证号码
        /// </summary>
        /// <param name="cardNo"></param>
        /// <returns></returns>
        public static bool IdentityCardIsValid(string cardNo)
        {
            string str;
            Regex regCard = new Regex(@"(^\d{15}$)|(^\d{17}(\d|X)$)"); //正则式，可验证15或18位身份证
            if (regCard.IsMatch(cardNo))  //验证身份证合法性
            {
                if (cardNo.Length == 18)
                {
                    str = cardNo.Substring(6, 8);
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
                return false;

            return true;
        }

    }
}
