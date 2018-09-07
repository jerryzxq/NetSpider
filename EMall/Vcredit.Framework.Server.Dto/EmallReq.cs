using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    public class EmallReq : BaseReq
    {
        /// <summary>
        ///请求抓取信息类型（见CrawlerType类），多种类型用逗号隔开
        /// </summary>
        public string CrawlerTypes { get; set; }
        public string IdentityCard { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Smscode { get; set; }
        public string Username { get; set; }
        public string Vercode { get; set; }
        public string Website { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
    }
}
