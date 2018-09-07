using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;

namespace Vcredit.ExternalCredit.Services
{
    /// <summary>
    /// 调用合规接口服务
    /// </summary>
    public interface ComplianceService
    {
        /// <summary>
        /// 查询是否已手写签名
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IsSignaturedResponse IsSignatured(IsSignaturedRequest request);

        /// <summary>
        /// 接口回调
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        CallBackResponse CallBack(CallBackRequest request);

		/// <summary>
		/// 合规申请接口（担保）
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		ApplyResponse Apply(ApplyRequest request);

		/// <summary>
		/// 合规申请接口（外贸）
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		ApplyResponse ApplyExternal(ApplyRequest request);
        /// <summary>
        /// 合规申请接口（成都）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ApplyResponse ApplyChengdu(ApplyRequest request);

    }
}
