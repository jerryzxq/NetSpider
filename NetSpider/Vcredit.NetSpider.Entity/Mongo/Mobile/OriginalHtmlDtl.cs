using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    public class OriginalHtmlDtl
    {
        /// <summary>
        /// 步骤
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        /// 原始网页
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string UpdateTime { get; set; }
    }
}
