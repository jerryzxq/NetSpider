using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class Selector
    {
        /// <summary>
        /// 选择器的选择表达式
        /// </summary>
        [XmlAttribute]
        public string selectRegulation { get; set; }
        /// <summary>
        /// 选择器的选择节点，空为html
        /// </summary>
        [XmlAttribute]
        public string selectAttribute { get; set; }
        private string _start = "";
        /// <summary>
        /// 选择器的开始标记
        /// </summary>
        [XmlAttribute]
        public string start
        {
            get { return _start; }
            set { this._start = value; }
        }
        private string _end = "";
        /// <summary>
        /// 选择器的结束标记
        /// </summary>
        [XmlAttribute]
        public string end
        {
            get { return _end; }
            set { this._end = value; }
        }
        private string _type = "html";
        /// <summary>
        /// 类别
        /// </summary>
        [XmlAttribute]
        public string type
        {
            get { return _type; }
            set { this._type = value; }
        }
    }
}
