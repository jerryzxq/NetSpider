using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.RestService.Models.Credit
{
    public class CreditReq
    {
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否本地籍
        /// </summary>
        public string IsLocal { get; set; }
        /// <summary>
        /// 教育程度
        /// </summary>
        public string Education { get; set; }
        /// <summary>
        /// 婚姻状态
        /// </summary>
        public string Marriage { get; set; }
        /// <summary>
        /// 是否交社保公积金
        /// </summary>
        public string IsPayfund { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string BusType { get; set; }

        /// <summary>
        /// ReportSn
        /// </summary>
        public string ReportSn { get; set; }
    }

}
