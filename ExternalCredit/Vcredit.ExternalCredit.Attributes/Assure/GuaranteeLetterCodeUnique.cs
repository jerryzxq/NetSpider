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
    /// 担保业务编号不能重复校验
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GuaranteeLetterCodeUniqueAttribute : ValidationAttribute
    {
        private static string DefaultErrorMessage = string.Format("担保业务编号已存在");

        public GuaranteeLetterCodeUniqueAttribute() : base(DefaultErrorMessage)
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

            var code = value.ToString();
            CRD_CD_AssureReportedInfoBusiness _business = new CRD_CD_AssureReportedInfoBusiness();
            var isUnique =  !_business.IsCodeExsit(code);

            return isUnique;
        }
    }
}
