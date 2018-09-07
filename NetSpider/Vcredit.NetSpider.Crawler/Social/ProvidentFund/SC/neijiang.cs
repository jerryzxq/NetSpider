using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.SC
{
    /// <summary>
    /// 四川 内江 公积金
    /// </summary>
    public class neijiang : IProvidentFundCrawler
    {
        #region properties
        private readonly IPluginHtmlParser _htmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件

        private const string BaseUrl = "http://img.neijiang.gov.cn/";
        private const string City = "sc_neijiang";

        private CookieCollection _cookies = new CookieCollection();
        private readonly HttpHelper _httpHelper = new HttpHelper();
        private HttpResult _httpResult = null;
        private HttpItem _httpItem = null;
        #endregion

        #region 验证码数据初始化
        /// <summary>
        /// 验证码数据初始化
        /// </summary>
        /// <param name="fundReq"></param>
        /// <returns></returns>
        public VerCodeRes ProvidentFundInit(ProvidentFundReq fundReq = null)
        {
            throw new NotImplementedException();

            //var res = new VerCodeRes
            //{
            //    Token = CommonFun.GetGuidID(),
            //    StatusCode = ServiceConsts.StatusCode_success
            //};
            //var verCodeUrl = BaseUrl + "captcha/index?time=" + DateTime.Now.ToString("yyyyMMddHHmmss");
            //try
            //{
            //    _httpItem = new HttpItem
            //    {
            //        URL = verCodeUrl,
            //        Method = "GET",
            //        ResultType = ResultType.Byte,
            //        CookieCollection = _cookies,
            //        ResultCookieType = ResultCookieType.CookieCollection
            //    };
            //    _httpResult = _httpHelper.GetHtml(_httpItem);
            //    if (_httpResult.StatusCode != HttpStatusCode.OK)
            //    {
            //        res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
            //        res.StatusCode = ServiceConsts.StatusCode_fail;

            //        return res;
            //    }

            //    _cookies = CommonFun.GetCookieCollection(_cookies, _httpResult.CookieCollection);
            //    res.VerCodeBase64 = CommonFun.GetVercodeBase64(_httpResult.ResultByte);
            //    //保存验证码图片在本地
            //    FileOperateHelper.WriteVerCodeImage(res.Token, _httpResult.ResultByte);
            //    res.VerCodeUrl = CommonFun.GetVercodeUrl(res.Token);

            //    res.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
            //    res.StatusCode = ServiceConsts.StatusCode_success;
            //    res.StatusDescription = City + ServiceConsts.ProvidentFund_InitSuccess;
            //    CacheHelper.SetCache(res.Token, _cookies);
            //}
            //catch (Exception e)
            //{
            //    res.StatusCode = ServiceConsts.StatusCode_error;
            //    res.StatusDescription = City + ServiceConsts.ProvidentFund_InitError;
            //    Log4netAdapter.WriteError(City + ServiceConsts.ProvidentFund_InitError, e);
            //}
            //return res;
        }
        #endregion

        public ProvidentFundQueryRes ProvidentFundQuery(ProvidentFundReq fundReq)
        {
            throw new NotImplementedException();
        }
    }
}
