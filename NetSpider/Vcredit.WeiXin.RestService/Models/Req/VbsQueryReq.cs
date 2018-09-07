using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class VbsQueryReq
    {
        /// <summary>
        /// 身份证号
        /// </summary>
        public string Identitycard { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        public string FileName { get; set; }
    }
}
