using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    public class DataRes<T>
    {
        private int _StatusCode = 1;
        public int StatusCode
        {
            get { return _StatusCode; }
            set { this._StatusCode = value; }
        }
        public string StatusDescription { get; set; }
        private string _StartTime = DateTime.Now.ToString();
        public string StartTime
        {
            get { return _StartTime; }
            set { this._StartTime = value; }
        }
        private string _EndTime = DateTime.Now.ToString();
        public string EndTime
        {
            get { return _StartTime; }
            set { this._StartTime = value; }
        }
        public T Result { get; set; }

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
