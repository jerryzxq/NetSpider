using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service
{
    public class PbccrcReportQueryApplyReq:BaseReq
    {
        public List<CRD_KbaQuestion> KbaQuestions { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerCode { get; set; }
        /// <summary>
        /// 银联卡认证码
        /// </summary>
        public string UnionpayCode { get; set; }
        /// <summary>
        /// 银联卡页面
        /// </summary>
        public string UnionHtml { get; set; }

    }
}
