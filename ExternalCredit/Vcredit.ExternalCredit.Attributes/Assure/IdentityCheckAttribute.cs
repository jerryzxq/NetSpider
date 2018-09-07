using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.Attributes
{
    /// <summary>
    /// 身份证格式校验
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IdentityCheckAttribute : ValidationAttribute
    {
        private static string DefaultErrorMessage = "身份证格式错误";

        public IdentityCheckAttribute() : base(DefaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return DefaultErrorMessage;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var cardNo = value.ToString();
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
                        DefaultErrorMessage = "身份证校验码错误";
                        return false;
                    }
                }
            }
            else
            {
                DefaultErrorMessage = "身份证不符合15位或18位格式规范";
                return false;
            }
            return true;
        }


    }
}
