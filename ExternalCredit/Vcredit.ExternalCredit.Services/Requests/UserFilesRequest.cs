using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Services.Requests
{
    /// <summary>
    /// 用户文件请求
    /// </summary>
    public class UserFilesRequest
    {
        /// <summary>
        /// 身份证号码    字符串
        /// </summary>
        public string IdentityNo { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// vbs ApplyID
        /// </summary>
        public int ApplyID { get; set; }
    }
}
