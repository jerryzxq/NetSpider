using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using System.Text.RegularExpressions;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using MongoDB.Driver;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class OriginalHtmlMongo
    {
        BaseMongo baseMongo = null;
        public OriginalHtmlMongo()
        {
            baseMongo = new BaseMongo("netspider", "AnalysisMongoDB");
        }

        public void Save(OriginalHtml html)
        {
            baseMongo.Insert<OriginalHtml>(html, "mobile_html");
        }

        public void Update(OriginalHtml html)
        {
            try
            {
                var query = new QueryDocument { { "Website", html.Website }, { "LogType", html.LogType } };
                baseMongo.Remove<MobileSeting>(query, "mobile_html");
                Save(html);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public OriginalHtml GetByIdcardModile(string logType, string website)
        {
            OriginalHtml html = null;
            try
            {
                var query = Query.And(Query.EQ("LogType", logType), Query.EQ("Website", website));
                html = baseMongo.FindOne<OriginalHtml>(query, "mobile_html");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return html;
        }

    }
}
