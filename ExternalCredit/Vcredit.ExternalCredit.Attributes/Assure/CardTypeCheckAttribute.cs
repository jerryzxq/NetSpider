using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.Attributes
{
    /// <summary>
    /// 证件类型格式校验
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CardTypeCheckAttribute : ValidationAttribute
    {
        private static string DefaultErrorMessage = "证件类型不存在！";

        public CardTypeCheckAttribute() : base(DefaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return DefaultErrorMessage;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            var k = value.ToString();
            if (!CommonData.certTypeDic.Keys.Contains(k) || string.IsNullOrEmpty(CommonData.certTypeDic[k]))
            {
                DefaultErrorMessage = "请输入正确的证件类型，证件类型为：";
                foreach (var key in CommonData.certTypeDic.Keys)
                {
                    DefaultErrorMessage += key + ":" + CommonData.certTypeDic[key] + ";";
                }

                return false;
            }
            return true;
        }


    }
}
