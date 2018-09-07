using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class Dataconvert
    {
        /// <summary>
        /// 转换类型
        /// </summary>
        [XmlAttribute]
        public string type { get; set; }
        /// <summary>
        /// 原数据
        /// </summary>
        [XmlAttribute]
        public string olddata { get; set; }
        /// <summary>
        /// 新数据
        /// </summary>
        [XmlAttribute]
        public string newdata { get; set; }
        /// <summary>
        /// 需要包含的字符串
        /// </summary>
        [XmlAttribute]
        public string containata { get; set; }
    }
}
