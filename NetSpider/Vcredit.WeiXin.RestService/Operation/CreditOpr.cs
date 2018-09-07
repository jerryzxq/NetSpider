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

namespace Vcredit.WeiXin.RestService.Operation
{
    public class CreditOpr
    {
        public string spiderServiceUrl = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["spiderServiceUrl"]);

        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口

        public CRD_HD_REPORTEntity GetSummaryByReportsn(string reportSn)
        {
            CRD_HD_REPORTEntity entity = null;
            try
            {
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = spiderServiceUrl + "/credit/query/summary",
                    Postdata = string.Format("{{'reportSn':'{0}'}}", reportSn).ToBase64()
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string result = jsonService.GetResultFromParser(httpResults.Html, "Result");
                entity = jsonService.DeserializeObject<CRD_HD_REPORTEntity>(result);
            }
            catch (Exception e)
            {
            }
            return entity;
        }
    }
}