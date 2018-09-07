using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vcredit.NetSpider.Entity.JobManager
{
    public class GatherData
    {
        [XmlAttribute]
        public string name { get; set; }
        /// <summary>
        /// 从参数传值过来
        /// </summary>
        [XmlAttribute]
        public string fromParameter { get; set; }
        private string _type = "add";
        /// <summary>
        /// 类型
        /// </summary>
        [XmlAttribute]
        public string type
        {
            get { return _type; }
            set { this._type = value; }
        }
        private string _dataType = "list";
        /// <summary>
        /// 返回值的数据类型
        /// </summary>
        [XmlAttribute]
        public string dataType
        {
            get { return _dataType; }
            set { this._dataType = value; }
        }
        /// <summary>
        /// 返回值是否输出
        /// </summary>
        [XmlAttribute]
        public bool output { get; set; }
        /// <summary>
        /// 返回值为stirng集合
        /// </summary>
        public List<string> results { get; set; }
        /// <summary>
        /// 返回值为datatable
        /// </summary>
        public DataTable GatherDataTable{ get; set; }
        /// <summary>
        /// 返回值的数据转换
        /// </summary>
        public List<Dataconvert> Dataconverts { get; set; }
        /// <summary>
        /// 返回值的筛选器集合
        /// </summary>
        public List<Selector> Selectors { get; set; }
        /// <summary>
        /// 返回值为datatable的列集合
        /// </summary>
        public List<GatherDataColumn> GatherDataColumns { get; set; }
    }
}
