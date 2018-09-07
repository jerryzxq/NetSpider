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
    public class SocialOpr
    {
        public string spiderServiceUrl = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["spiderServiceUrl"]);

        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        public ProvidentFundEntity GetGJJByIdentityCard(string identityCard)
        {
            ProvidentFundEntity entity = null;
            try
            {
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = spiderServiceUrl + "/gjj/query/data/Json",
                    Postdata = string.Format("{{'identityCard':'{0}'}}", identityCard).ToBase64()
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string result = jsonService.GetResultFromParser(httpResults.Html, "Result");
                entity = jsonService.DeserializeObject<ProvidentFundEntity>(result);
            }
            catch (Exception e)
            {
            }
            return entity;
        }

        public SocialSecurityEntity GetShebaoByIdentityCard(string identityCard)
        {
            SocialSecurityEntity entity = null;
            try
            {
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = spiderServiceUrl + "/shebao/query/data/Json",
                    Postdata = string.Format("{{'identityCard':'{0}'}}", identityCard).ToBase64()
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string result = jsonService.GetResultFromParser(httpResults.Html, "Result");
                entity = jsonService.DeserializeObject<SocialSecurityEntity>(result);
            }
            catch (Exception e)
            {
            }
            return entity;
        }
    }
}