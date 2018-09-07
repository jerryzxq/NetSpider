using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vcredit.Common.Helper;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.PluginManager;
using System.Configuration;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.WeiXin.RestService.Models;

namespace Vcredit.WeiXin.RestService.Operation
{
    public class MobileOpr
    {
        public string spiderServiceUrl = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["spiderServiceUrl"]);

        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        public IList<Call> GetCallsById(string mobileId)
        {
            IList<Call> callsList = null;
            try
            {
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = spiderServiceUrl + "/mobile/query/callsById/Json",
                    Postdata = string.Format("{{'id':'{0}'}}", mobileId).ToBase64()
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string callsResult = jsonService.GetResultFromParser(httpResults.Html, "Result");
                callsList = jsonService.DeserializeObject<IList<Call>>(callsResult);
                if (callsList == null)
                    callsList = new List<Call>();
            }
            catch (Exception e)
            {
            }
            return callsList;
        }

        public Mobile_summaryModel GetSummaryById(string mobileId)
        {
            Mobile_summaryModel entity = null;
            try
            {
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = spiderServiceUrl + "/mobile/query/summaryById/Json",
                    Postdata = string.Format("{{'id':'{0}'}}", mobileId).ToBase64()
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string callsResult = jsonService.GetResultFromParser(httpResults.Html, "Result");
                entity = jsonService.DeserializeObject<Mobile_summaryModel>(callsResult);
            }
            catch (Exception e)
            {
            }
            return entity;
        }
        public Mobile_summaryModel GetSummaryByIdcardAndMobile(string identitycard, string mobile)
        {
            Mobile_summaryModel entity = null;
            try
            {
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = spiderServiceUrl + "/mobile/query/summary/Json",
                    Postdata = string.Format("{{'identitycard':'{0}','mobile':'{1}'}}", identitycard, mobile).ToBase64()
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string callsResult = jsonService.GetResultFromParser(httpResults.Html, "Result");
                entity = jsonService.DeserializeObject<Mobile_summaryModel>(callsResult);
            }
            catch (Exception e)
            {
            }
            return entity;
        }

    }
}