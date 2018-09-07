using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class Step 
    {
        [XmlAttribute]
        public int sort { get; set; }
        [XmlAttribute]
        public string url { get; set; }
        [XmlAttribute]
        public string contentType { get; set; }
        [XmlAttribute]
        public string postEncoding { get; set; }
        [XmlAttribute]
        public string accept { get; set; }
        [XmlAttribute]
        public string methed { get; set; }
        [XmlAttribute]
        public string referer { get; set; }
        [XmlAttribute]
        public string PostData { get; set; }
        [XmlAttribute]
        public int waittime { get; set; }
        /// <summary>
        /// 判断此步是否完成表达式
        /// </summary>
        [XmlAttribute]
        public string isFinishExpr { get; set; }
        /// <summary>
        /// 判断此步是否进入表达式
        /// </summary>
        [XmlAttribute]
        public string isEnterExpr { get; set; }
        /// <summary>
        /// 判断此步是否跳出表达式
        /// </summary>
        [XmlAttribute]
        public string isSkipExpr { get; set; }
        /// <summary>
        /// 判断整个流程是否结束表达式
        /// </summary>
        [XmlAttribute]
        public string isFlowEndExpr { get; set; }
        [XmlAttribute]
        public int timeout { get; set; }

        private string _resultType = "string";
        [XmlAttribute]
        public string resultType
        {
            get { return _resultType; }
            set { this._resultType = value; }
        }
        /// <summary>
        /// 返回值集合
        /// </summary>
        public List<GatherData> GatherDatas { get; set; }
        /// <summary>
        /// 参数集合
        /// </summary>
        public List<Parameter> Parameters { get; set; }
        public List<Step> ExecutSteps { get; set; }
    }
}
