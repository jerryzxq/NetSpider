using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.Attributes
{
    /// <summary>
    /// 身份证号重新查询校验，判断是否请求过于频繁
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ReTryCheckAttribute : ValidationAttribute
    {
        private static string DefaultErrorMessage = string.Format("当前身份证号的用户请求过于频繁");

        public ReTryCheckAttribute() : base(DefaultErrorMessage)
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

            var cachParam = CachHelper.Get<string>(cardNo);
            if (cachParam == null)
            {
                CachHelper.Insert(cardNo, cardNo, 0.5); // 缓存有效期
                return true;
            }
            return false;
        }
    }
}
