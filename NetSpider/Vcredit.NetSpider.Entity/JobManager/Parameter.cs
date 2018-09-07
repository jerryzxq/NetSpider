using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class Parameter
    {
        [XmlAttribute]
        public string name { get; set; }
        public string _type = "fromreturn";
        /// <summary>
        /// 参数类别
        /// </summary>
        [XmlAttribute]
        public string type
        {
            get { return _type;}
            set { this._type=value;}
        }
        /// <summary>
        ///webservice传入参数的地址
        /// </summary>
        [XmlAttribute]
        public string address { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        [XmlAttribute]
        public string value { get; set; }
        public List<string> results { get; set; }
    }
}
