using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Dto
{
    public class BaseRes 
    {
        private StatusCode _statusCode = StatusCode.Success;
        public StatusCode StatusCode
        {
            get { return _statusCode; }
            set { this._statusCode = value; }
        }

        public string StatusDescription { get; set; }

        public string Result { get; set; }

        public string Token { get; set; }

        ///// <summary>
        ///// 下一步
        ///// </summary>
        //public string NextProCode { get; set; }

        private string _startTime = DateTime.Now.ToString();
        public string StartTime
        {
            get { return _startTime; }
            set { this._startTime = value; }
        }

        private string _endTime = DateTime.Now.ToString();
        public string EndTime
        {
            get { return _endTime; }
            set { this._endTime = value; }
        }
    }

    /// <summary>
    /// 接口状态
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// 操作异常
        /// </summary>
        [Description("操作异常")]
        Error = 110,
        /// <summary>
        /// 操作失败
        /// </summary>
        [Description("操作失败")]
        Fail = 1,
        /// <summary>
        /// 
        /// </summary>
        [Description("HTTP请求失败")]
        Httpfail = 500,
        /// <summary>
        /// 操作成功
        /// </summary>
        [Description("操作成功")]
        Success = 0,

    }
}
