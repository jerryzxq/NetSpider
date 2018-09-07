using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Services.Responses
{
    /// <summary>
    /// 是否已签名 response
    /// </summary>
    public class IsSignaturedResponse
    {
        /// <summary>
        /// 查询是否成功  布尔类型
        /// </summary>
        public bool IsSeccess { get; set; }

        /// <summary>
        /// 消息  字符串
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否已签名    布尔类型
        /// </summary>
        public bool IsSignature { get; set; }

        /// <summary>
        /// 签名时间   字符串  (格式：yyyy-MM-dd HH:mm:ss)   
        /// </summary>
        public DateTime? SignatureTime { get; set; }
    }
}
