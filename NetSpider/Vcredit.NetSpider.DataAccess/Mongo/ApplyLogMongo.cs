using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using MongoDB.Driver;
using MongoDB.Bson;
using Vcredit.NetSpider.Entity;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class ApplyLogMongo
    {
        BaseMongo baseMongo = null;
        public ApplyLogMongo()
        {
            baseMongo = new BaseMongo("netspiderlog", "CrawlerMongoDB");
        }
        public void Save(ApplyLog log)
        {
            //保存抓取信息
            baseMongo.Insert<ApplyLog>(log, "apply_log");
        }

        public void SaveList(List<ApplyLog> logs)
        {
            //保存抓取信息
            baseMongo.Insert<ApplyLog>(logs, "apply_log");
        }
        public List<ApplyLog> GetByToken(string Token)
        {
            List<ApplyLog> log = null;
            try
            {
                var query = Query.EQ("Token", Token);
                log = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return log;
        }

        public ApplyLog GetByErrDate(string ErrDate)
        {
            ApplyLog log = null;
            try
            {
                var query = Query.EQ("ErrDate", ErrDate);
                baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return log;
        }

        /// <summary>
        /// 获取网站不稳定
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetByCreateTime(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                var query = Query.And(
                     Query.EQ("LogType", logType)
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

        /// <summary>
        /// 获取程序异常日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetProExcLog(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                var query = Query.And(
                    Query.EQ("StatusCode", ServiceConsts.StatusCode_error)
                    , Query.EQ("LogType", logType)
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

        /// <summary>
        /// 获取网站改版日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetRevisionExcLog(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                List<BsonValue> steps = new List<BsonValue>();
                steps.Add(ServiceConsts.Step_Infor_Analysis);
                steps.Add(ServiceConsts.Step_Bill_Analysis);
                steps.Add(ServiceConsts.Step_Call_Analysis);
                steps.Add(ServiceConsts.Step_Sms_Analysis);
                steps.Add(ServiceConsts.Step_Net_Analysis);
                var query = Query.And(
                    Query.EQ("StatusCode", ServiceConsts.StatusCode_error)
                    , Query.In("Step", steps)
                    , Query.EQ("LogType", logType)
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

        /// <summary>
        /// 获取网站不稳定日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetWebExcLog(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                var query = Query.And(
                     Query.EQ("LogType", logType)
                    , Query.EQ("LogDtlList.StatusCode", ServiceConsts.StatusCode_httpfail)  //状态为httpfail表示网站不稳定
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

        /// <summary>
        /// 获取网站http请求日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetHttpLog(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                List<BsonValue> steps = new List<BsonValue>();
                steps.Add(ServiceConsts.Step_Infor_Analysis);
                steps.Add(ServiceConsts.Step_Bill_Analysis);
                steps.Add(ServiceConsts.Step_Call_Analysis);
                steps.Add(ServiceConsts.Step_Sms_Analysis);
                steps.Add(ServiceConsts.Step_Net_Analysis);
                var query = Query.And(
                     Query.EQ("LogType", logType)
                    , Query.NotIn("Step", steps)
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

        /// <summary>
        /// 获取网站升级日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetWebUpdateLog(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                var query = Query.And(
                     Query.EQ("LogType", logType)
                    , Query.EQ("LogDtlList.StatusCode", ServiceConsts.CrawlerStatusCode_SystemUpdate)  //状态为httpfail表示网站不稳定
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

        /// <summary>
        ///获取不同步骤不同时间的成功记录
        /// </summary>
        /// <param name="step">步骤</param>
        /// <param name="date">时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetByStep(string step, DateTime date)
        {
            List<ApplyLog> logs = null;
            try
            {
                var query = Query.And(
                     Query.EQ("LogDtlList.StatusCode", 0)
                    , Query.EQ("LogDtlList.Step", step)
                    , Query.GTE("LogDtlList.CreateTime", date.ToString(Consts.DateFormatString9))
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message); 
            }
            return logs;
        }

        /// <summary>
        /// 获取字段异常日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="staDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<ApplyLog> GetColmonExcLog(string logType, string staDate, string endDate)
        {
            List<ApplyLog> logs = null;
            try
            {
                var query = Query.And(
                    Query.EQ("LogDtlList.StatusCode", ServiceConsts.CrawlerStatusCode_ColmonError)
                    , Query.EQ("Step", ServiceConsts.Step_Column_Monitoring)
                    , Query.EQ("LogType", logType)
                    , Query.GTE("LogDtlList.CreateTime", staDate)
                    , Query.LT("LogDtlList.CreateTime", endDate)
                    );
                logs = baseMongo.Find<ApplyLog>(query, "apply_log");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return logs;
        }

    }
}
