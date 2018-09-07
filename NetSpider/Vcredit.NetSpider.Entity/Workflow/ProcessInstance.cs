using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.WorkFlow
{
    public class ProcessInstance
    {
        public String id { get; set; }
        public String name { get; set; }
        public String processDefKey { get; set; }
        /// <summary>
        /// 抓取方式（socket、webrequest）
        /// </summary>
        public String fetchWay { get; set; }
        /// <summary>
        /// 流程类型，线性顺序、递归顺序
        /// </summary>
        public String type { get; set; }
        /// <summary>
        /// 流程的任务层级
        /// </summary>
        public int level { get; set; }
        public bool isEnd { get; set; }
        public int currentsort { get; set; }
        public int flowType { get; set; }
        private List<Task> _currenttasks = new List<Task>();
        /// <summary>
        /// 当前所用任务
        /// </summary>
        public List<Task> currenttasks
        {
            get { return _currenttasks; }
            set { _currenttasks = value; }
        }

        private Dictionary<string, List<string>> _GatherData = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> GatherData
        {
            get { return _GatherData; }
            set { _GatherData = value; }
        }
        private Dictionary<string, DataTable> _GatherDataTable = new Dictionary<string, DataTable>();
        public Dictionary<string, DataTable> GatherDataTable
        {
            get { return _GatherDataTable; }
            set { _GatherDataTable = value; }
        }
        private Dictionary<string, string> _Variable = new Dictionary<string, string>();
        public Dictionary<string, string> Variable
        {
            get { return _Variable; }
            set { _Variable = value; }
        }
    }
}
