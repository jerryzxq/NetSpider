using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.Entity.Mongo.Log
{
    [BsonIgnoreExtraElements]
    public class ApplyLog : BaseMongoEntity
    {
        /// <summary>
        /// 会话令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// 步骤
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        /// 采集网站
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndDate { get; set; }

        private List<ApplyLogDtl> _logDtlList = new List<ApplyLogDtl>();
        /// <summary>
        /// 日志集合
        /// </summary>
        public List<ApplyLogDtl> LogDtlList
        {
            get { return _logDtlList; }
            set { _logDtlList = value; }
        }

        public ApplyLog()
        {
        }

        public ApplyLog(string startDate, string logType)
        {
            this.StartDate = startDate;
            this.LogType = logType;
        }

        public ApplyLog(string logType, string step, string website)
        {
            this.StartDate = DateTime.Now.ToString(Consts.DateFormatString9);
            this.LogType = logType;
            this.Step = step;
            this.Website = website;
        }
    }
}
