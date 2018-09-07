using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Log;


namespace Vcredit.NetSpider.Crawler
{
    public class LogHelper : HttpHelper
    {

        /// <summary>
        /// 根据传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="item">参数类对象</param>
        /// <param name="appLog">日志记录类</param>
        /// <param name="step">数据采集步骤</param>
        /// <returns>返回HttpResult类型</returns>
        public HttpResult GetHtmlLog(HttpItem item, ApplyLog appLog, string title)
        {
            //返回参数
            HttpResult result = new HttpResult();
            result = GetHtml(item);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                ApplyLogDtl logDtl = new ApplyLogDtl(title);
                logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
                logDtl.StatusCode = ServiceConsts.StatusCode_httpfail;
                logDtl.Description = logDtl.Title + "异常：" + ServiceConsts.StatusDescription_httpfail;
                appLog.LogDtlList.Add(logDtl);
            }
            return result;
        }
    }
}
