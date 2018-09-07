using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.WorkFlow
{
    public class Task
    {
        public String id { get; set; }
        public int sort { get; set; }
        public String url { get; set; }
        public String contentType { get; set; }
        public String postEncoding { get; set; }
        public String accept { get; set; }
        public String methed { get; set; }
        public String referer { get; set; }
        public String PostData { get; set; }
        public String processInstanceId { get; set; }
        public String name { get; set; }
        public String resultType { get; set; }
        public int waittime { get; set; }
        public int timeout { get; set; }
        private Dictionary<string, string> _Variable = new Dictionary<string, string>();
        public Dictionary<string, string> Variable
        {
            get { return _Variable; }
            set { _Variable = value; }
        }
        private List<Task> _nexttasks = new List<Task>();
        public List<Task> nexttasks
        {
            get { return _nexttasks; }
            set { this._nexttasks = value; }
        }

        private Dictionary<string, List<string>> _GatherData = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> GatherData
        {
            get { return _GatherData; }
            set { _GatherData = value; }
        }
    }
}
