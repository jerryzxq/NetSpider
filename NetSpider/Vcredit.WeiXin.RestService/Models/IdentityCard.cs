using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models
{
    public class IdentityCard
    {
        private int _StatusCode = 1;
        public int StatusCode
        {
            get { return _StatusCode; }
            set { this._StatusCode = value; }
        }
        public string StatusDescription { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public string Birthday { get; set; }
        /// <summary>
        /// 户籍地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public string Folk { get; set; }
        /// <summary>
        /// 签发机关
        /// </summary>
        public string IssueAuthority { get; set; }
        /// <summary>
        /// 有效期限
        /// </summary>
        public string ValidPeriod { get; set; }
    }
}
