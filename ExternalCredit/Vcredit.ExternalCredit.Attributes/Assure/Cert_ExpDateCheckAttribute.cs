using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.Attributes
{
    /// <summary>
    /// 证件过期验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
     
    public class Cert_ExpDateCheckAttribute:ValidationAttribute
    {
        private static  string DefaultErrorMessage = "身份证已经过期！";

        public  Cert_ExpDateCheckAttribute() : base(DefaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return DefaultErrorMessage;
        }

        public override bool IsValid(object value)
        {
      
            var expDate = Convert.ToDateTime(value);
            if(DateTime.MinValue ==expDate)
            {
                DefaultErrorMessage = "没有参数身份证过期日期（Cert_ExpDate）或者不合法";
                return false;
            }
            if (expDate.AddDays(-(ConfigData.CertReviseDays - 1)) < DateTime.Now)
            {
                DefaultErrorMessage = "身份证已经过期！";
                return false;
            }
            return true;
        }
    }
}
