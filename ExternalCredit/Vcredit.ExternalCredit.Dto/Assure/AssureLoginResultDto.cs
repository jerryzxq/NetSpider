using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.ExternalCredit.Dto
{
    /// <summary>
    /// 担保查询登陆数据实体
    /// </summary>
    public class AssureLoginResultDto
    {
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 登陆成功cookies
        /// </summary>
        public string Cookies { get; set; }
    }
}
