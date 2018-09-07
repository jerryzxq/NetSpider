using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.ProvidentFund
{
    public class FormParam
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; }
        /// <summary>
        /// 参数编码
        /// </summary>
        public string ParameterCode { get; set; }
        /// <summary>
        /// 参数提示信息
        /// </summary>
        public string ParameterMessage { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Orderby { get; set; }
    }
}
