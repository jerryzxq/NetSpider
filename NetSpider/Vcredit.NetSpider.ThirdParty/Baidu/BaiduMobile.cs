using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.ThirdParty.Baidu
{
    public class BaiduMobile
    {
        //string url = "http://apis.baidu.com/apistore/mobilenumber/mobilenumber";
        string url = "http://apis.baidu.com/apistore/mobilenumber/mobilenumber";
        string apikey = "fd0b6e8fae97e65dab90f4343fdaa429";
        HttpResult httpResult = null;
        HttpHelper httpHelper = new HttpHelper();
        HttpItem httpItem = null;
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        public BaiduMobile()
        { }

        public string GetMobileInfo(string mobilenumber)
        {
            return GetMobileInfoV1(mobilenumber);
        }
        private string GetMobileInfoV1(string mobilenumber)
        {
            url = "http://apis.baidu.com/apistore/mobilenumber/mobilenumber?phone=" + mobilenumber;
            httpItem = new HttpItem
            {
                URL = url,
            };
            httpItem.Header.Add("apikey", apikey);

            httpResult = httpHelper.GetHtml(httpItem);

            return CommonFun.ClearFlag(jsonService.GetResultFromMultiNode(httpResult.Html, "retData"));
        }
        private string GetMobileInfoV2(string mobilenumber)
        {
            string strRet = string.Empty;
            try
            {
                url = "http://apis.baidu.com/chazhao/mobilesearch/phonesearch?phone=" + mobilenumber;
                httpItem = new HttpItem
                {
                    URL = url,
                };
                httpItem.Header.Add("apikey", apikey);
                httpResult = httpHelper.GetHtml(httpItem);
                string error = jsonService.GetResultFromMultiNode(httpResult.Html, "error");
                if (error == "0")
                {
                    mobieResV2 resV2 =jsonService.DeserializeObject<mobieResV2>(jsonService.GetResultFromMultiNode(httpResult.Html, "data"));
                    strRet = string.Format("{{\"supplier\": \"{0}\",\"province\": \"{1}\",\"city\": \"{2}\"}}",
                        resV2.Operator,
                        resV2.province,
                        resV2.city);
                }
            }
            catch (Exception)
            {
 
            }
            return strRet;
        }

        class mobieResV2
        {
            public string Operator{get;set;}
            public string province { get; set; }
            public string city { get; set; }
        }
        
    }
}
