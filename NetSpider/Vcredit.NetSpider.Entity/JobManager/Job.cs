using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class Job 
    {
        [XmlAttribute]
        public string name { get; set; }
        /// <summary>
        /// 步骤数
        /// </summary>
        [XmlAttribute]
        public int level { get; set; }
        public String _fetchWay = "http";
        /// <summary>
        /// 采集方式
        /// </summary>
        [XmlAttribute]
        public String fetchWay
        {
            get { return _fetchWay; }
            set { this._fetchWay = value; }
        }
        /// <summary>
        /// 类别
        /// </summary>
        [XmlAttribute]
        public String type { get; set; }
        
        private int _flowType =(int)GlobalEnums.EnumFlowType.QueueExec;
        /// <summary>
        /// 流程类别1001：队列；1002-链式；
        /// </summary>
        [XmlAttribute]
        public int flowType
        {
            get { return _flowType; }
            set { this._flowType = value; }
        }
        /// <summary>
        /// 步骤集合
        /// </summary>
        public List<Step> Steps { get; set; }
    }
}
