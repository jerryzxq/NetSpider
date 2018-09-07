using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class SocialSecurityResult
    {
        /// <summary>
        /// 评分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 是否本人
        /// </summary>
        public int IsEqual { get; set; }
    }
}
