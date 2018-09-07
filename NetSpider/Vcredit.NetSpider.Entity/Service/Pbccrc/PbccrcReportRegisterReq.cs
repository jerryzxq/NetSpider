using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class PbccrcReportRegisterReq: LoginReq
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        public string certNo { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string certType { get; set; }
        /// <summary>
        /// 查询码
        /// </summary>
        public string querycode { get; set; }
        /// <summary>
        /// 确认密码
        /// </summary>
        public string confirmpassword { get; set; }
        /// <summary>
        /// 电子邮件
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string mobileTel { get; set; }
    }
}
