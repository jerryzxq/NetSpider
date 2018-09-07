using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Vcredit.Framework.Server.Dto;

namespace Vcredit.Framework.Server.Dto.TaoBao
{
    public class TaoBaoLoginReq : BaseReq
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string UmToken { get; set; }
        public string UA { get; set; }

        public string Name { get; set; }
        public string IdentityCard { get; set; }
        public string Website { get; set; }

        public string Mobile { get; set; }
        public string Smscode { get; set; }
        public string Vercode { get; set; }
        public string ClientIp { get; set; }
        public string RedirectUrl { get; set; }
        /// <summary>
        /// cookie参数
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        ///请求抓取信息类型（见CrawlerType类），多种类型用逗号隔开
        /// </summary>
        public string CrawlerTypes { get; set; }
        /// <summary>
        /// 业务端提供Key
        /// </summary>
        public string Key { get; set; }
    }
}
