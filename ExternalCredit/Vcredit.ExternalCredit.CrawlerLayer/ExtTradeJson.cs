using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using ServiceStack.Html;
using Vcredit.Common.Helper;
using Newtonsoft.Json;
using Vcredit.ExtTrade.CommonLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer
{
    class AliPayKafkaResult<T>
    {
        public string topic { get; set; }
        public string time { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }

        public T data { get; set; }
    }
    class ExtTradeJson
    {
        /// <summary>
        /// 外贸原始征信json
        /// </summary>
        public string ExtJson { get; set; }
        /// <summary>
        /// key:reporot_sn  value :report_id
        /// </summary>
        public Dictionary<string,long> ReportIdList{ get; set; }
    }
    class SengData
    {
        public static void SendAcction<T>(Func<T> func,string batNo)
        {
           // System.Threading.Tasks.Task.Factory.StartNew(() =>
           // {
                try
                {
                    Log4netAdapter.WriteInfo("BatNo开始上传"+batNo);
                    T result = func();
                    SendKafka<T>(new AliPayKafkaResult<T>() { data = result },batNo);
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("BatNo:" + batNo + "上传出现异常",ex);
                }
           // }).ContinueWith(e => e.Dispose());
        }

        public static void SendKafka<T>(AliPayKafkaResult<T> paydata,string batNo)
        {

            HttpHelper helper = new HttpHelper();
            paydata.topic = ConfigData.SendKafkaTopic;
            var httpItem = new HttpItem()
            {

                URL = ConfigData.SendKafkaService + "/kafka/sendData",
                Postdata = JsonConvert.SerializeObject(paydata),
                Method = "post",
                PostEncoding = Encoding.UTF8,
                ContentType = "application/json;",
                Timeout=30000

            };
            var crawlerResult = helper.GetHtml(httpItem);
            BaseRes Res = JsonConvert.DeserializeObject<BaseRes>(crawlerResult.Html);
            var result = Res.StatusCode == (int)SysEnums.ServiceStatus.Success;
            Log4netAdapter.WriteInfo("BatNo:"+batNo+"结束上传");
            if (!result)
            {
                Log4netAdapter.WriteInfo("BatNo:" + batNo + "上传失败"+Res.StatusDescription);
            }

        }
    }
}
