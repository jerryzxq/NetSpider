using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models.Req
{
    class JxlPustDataReq
    {
        /// <summary>
        /// 推送数据
        /// </summary>
        public string JSON_INFO { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string CUST_NAME { get; set; }
        /// <summary>
        /// 用户手机号码
        /// </summary>
        public string APP_PHONE_NO { get; set; }
        /// <summary>
        /// 用户身份证号码
        /// </summary>
        public string APP_IDCARD_NO { get; set; }
    }
}
