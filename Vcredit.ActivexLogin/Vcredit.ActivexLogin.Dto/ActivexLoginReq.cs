using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Common;

namespace Vcredit.ActivexLogin.Dto
{
    public class ActivexLoginReq : BaseReq
    {
        public String Account { get; set; }

        [MinLength(6, ErrorMessage = "密码至少输入六位")]
        public String Password { get; set; }

        /// <summary>
        /// 额外参数
        /// </summary>
        public string AdditionalParam { get; set; }


    }

}
