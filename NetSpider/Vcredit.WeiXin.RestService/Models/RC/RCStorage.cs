using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity;

namespace Vcredit.WeiXin.RestService.Models
{
    public class RCStorage : BaseMongoEntity
    {
        public string BusType { get; set; }
        public string BusId { get; set; }
        public List<VBXML> Params { get; set; }
        public DecisionResult Result { get; set; }

        private string _CreateTime = DateTime.Now.ToString();
        public string CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
    }
}
