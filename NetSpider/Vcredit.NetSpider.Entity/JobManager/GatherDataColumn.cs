using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class GatherDataColumn
    {
        [XmlAttribute]
        public string colName { get; set; }
        private string _start = "";
        /// <summary>
        /// 开始标记
        /// </summary>
        [XmlAttribute]
        public string start
        {
            get { return _start; }
            set { this._start = value; }
        }
        private string _end = "";
        /// <summary>
        /// 结束标记
        /// </summary>
        [XmlAttribute]
        public string end
        {
            get { return _end; }
            set { this._end = value; }
        }
        /// <summary>
        /// 正则获取数据的类别，枚举EnumLimitSign
        /// </summary>
        [XmlAttribute]
        public int limitSign { get; set; }
        /// <summary>
        /// 自定义正则表达式
        /// </summary>
        [XmlAttribute]
        public int RegionExpression { get; set; }
        /// <summary>
        /// 某参数值
        /// </summary>
        [XmlAttribute]
        public string fromparameter { get; set; }

    }
}
