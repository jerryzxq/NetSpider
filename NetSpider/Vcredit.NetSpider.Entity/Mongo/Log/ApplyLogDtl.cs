using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Log
{
    public class ApplyLogDtl
    {

        /// <summary>
        /// 步骤
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        public ApplyLogDtl(string step,string title)
        {
            this.Step = step;
            this.Title = title;
        }

        public ApplyLogDtl(string title)
        {
            this.Title = title;
        }

        public ApplyLogDtl(){}
    }
}
