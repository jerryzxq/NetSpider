using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;

namespace Vcredit.ExternalCredit.Services
{
    public interface IComplianAdditionalService
    {
        /// <summary>
        /// 根据Applyid获取授权文件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        UserFileRespne GetAuthenticationFile(UserFilesRequest request);
    }
}
