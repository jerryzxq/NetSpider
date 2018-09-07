using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.ProvidentFund
{
    public class CityLevel
    {
        /// <summary>
        /// 城市编号
        /// </summary>
        public string CityCode { get; set; }
        /// <summary>
        /// 城市名称
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否上线[1是，0否]
        /// </summary>
        public string WhetherOnline { get; set; }
    }
}
