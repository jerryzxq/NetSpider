using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Mobile
{
    public class MobileReq : BaseReq
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string Vercode { get; set; }
        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Smscode { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Ip
        /// </summary>
        public string IPAddr { get; set; }
        /// <summary>
        /// 采集网站
        /// </summary>
        public string Website { get; set; }
        /// <summary>
        /// 服务密码
        /// </summary>
        public string Servecode { get; set; }
    }
}
