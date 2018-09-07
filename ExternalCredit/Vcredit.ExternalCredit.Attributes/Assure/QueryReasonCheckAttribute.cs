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
    /// 查询原因校验
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class QueryReasonCheckAttribute : ValidationAttribute
    {
        private static string DefaultErrorMessage = "查询原因字典不存在";

        public QueryReasonCheckAttribute() : base(DefaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return DefaultErrorMessage;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }
            var k = value.ToString();
            if (!CommonData.AssureQueryReason.Keys.Contains(k) || string.IsNullOrEmpty(CommonData.AssureQueryReason[k]))
            {
                DefaultErrorMessage = "请输入正确的查询原因，查询原因为：";
                foreach (var key in CommonData.AssureQueryReason.Keys)
                {
                    DefaultErrorMessage += key + ":" + CommonData.AssureQueryReason[key] + ";";
                }

                return false;
            }
            return true;

        }


    }
}
