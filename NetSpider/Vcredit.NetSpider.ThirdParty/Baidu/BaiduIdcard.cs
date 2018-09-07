using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.ThirdParty.Baidu
{
    public class BaiduIdcard
    {
        string url = "http://apis.baidu.com/apistore/idservice/id";
        string apikey = "fd0b6e8fae97e65dab90f4343fdaa429";
        HttpResult httpResult = null;
        HttpHelper httpHelper = new HttpHelper();
        HttpItem httpItem = null;
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        public BaiduIdcard()
        { }

        public string GetAddress(string cardno)
        {
            url += "?id=" + cardno;
            httpItem = new HttpItem
            {
                URL = url,
            };
            httpItem.Header.Add("apikey", apikey);

            httpResult = httpHelper.GetHtml(httpItem);

            return jsonService.GetResultFromMultiNode(httpResult.Html, "retData:address"); ;
        }
    }
}
