using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Services.Responses
{
    public class UserFilesContent
    {
        /// <summary>
        /// 身份证正反面图片base64
        /// </summary>
        public string IdCardImg { get; set; }
        /// <summary>
        /// 授权文件base64
        /// </summary>
        public string InfoPowerSignatured { get; set; }
        /// <summary>
        /// 授权文件，文件类型
        /// </summary>
        public string InfoPowerSignaturedFileType { get; set; }
    }
    public class UserFileRespne
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSucceed { get; set; }
        /// <summary>
        /// 查询信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
