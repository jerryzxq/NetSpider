using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Dto
{
    /// <summary>
    /// DataResponseDto
    /// </summary>
    public class DataResponseDto<T>
    {
        /// <summary>
        /// DataResponseDto
        /// </summary>
        public DataResponseDto()
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

        private string _StartTime = DateTime.Now.ToString();
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime
        {
            get { return _StartTime; }
            set { this._StartTime = value; }
        }

        private string _EndTime = DateTime.Now.ToString();
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime
        {
            get { return _StartTime; }
            set { this._StartTime = value; }
        }

        /// <summary>
        /// 总数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 页数
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
    }
}
