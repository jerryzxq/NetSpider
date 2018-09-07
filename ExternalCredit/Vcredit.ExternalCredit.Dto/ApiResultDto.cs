using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Vcredit.ExternalCredit.Dto
{
    /// <summary>
    /// api接口返回实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResultDto<T>
    {
        /// <summary>
        /// api接口返回实体
        /// </summary>
        public ApiResultDto()
        {
            StatusCode = StatusCode.Fail;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess
        {
            get { if (StatusCode == StatusCode.Success) return true; return false; }
        }

        /// <summary>
        /// 返回code
        /// </summary>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public T Result { get; set; }
    }

    /// <summary>
    /// 返回码
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        [Description("操作成功")]
        Success = 0,

        /// <summary>
        /// 操作失败
        /// </summary>
        [Description("操作失败")]
        Fail = 1,

        /// <summary>
        /// 操作异常
        /// </summary>
        [Description("操作异常")]
        Error = 110,

        /// <summary>
        /// HTTP请求失败
        /// </summary>
        [Description("HTTP请求失败")]
        Httpfail = 500,
    }
}