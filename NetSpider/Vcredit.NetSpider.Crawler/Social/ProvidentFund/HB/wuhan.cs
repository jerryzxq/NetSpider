using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HB
{
     //province = "HB";
     //       city = "WuHan";
     //       username = "lizhi20120828";
     //       password = "fengli1984";
    /// <summary>
    /// 加密控件
    /// </summary>
    public class wuhan:IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "https://www.hkbchina.com/pcweb/";
        string fundCity = "hb_wuhan";
        string Url = string.Empty;
        #endregion
        #region 私有变量
        string _service = string.Empty;
        string _sid = string.Empty;
        List<string> _results = new List<string>();
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        string _postData = string.Empty;
        ProvidentFundDetail _detail = new ProvidentFundDetail();
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                Url = baseUrl + "GenTokenImg3.do?v=000001";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "get",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    vcRes.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    vcRes.StatusCode = ServiceConsts.StatusCode_fail;
                    return vcRes;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                vcRes.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);

                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                vcRes.StatusDescription = "请根据链接查询验证码，重新调用接口并传参";
                vcRes.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                //添加缓存
                CacheHelper.SetCache(token, cookies);
                //
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_error;
                vcRes.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return vcRes;
        }

        public Entity.Service.ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            Res.ProvidentFundCity = fundCity;
            try
            {   //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                {
                    Res.StatusDescription = ServiceConsts.UserNameOrPasswordEmpty;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步登录

                //var modulus = "83f9eb0b4ca6c1c73183ebadd595368dae4f5df02131457521a09c35042122d538856292c47c0d3253e3a19cf5ce7952df9d8ca557b96e020f26e4e0f25f169bdf5263596aefc7d8ac6331ce2fe54aa6bb5e5d77e83dd0f6a162c9614400432f5187440833119388903188b11d005ee547a956de58ac366343d4a48bde88ae35";
                //var exponent = "10001";
                //var password = RSAHelper.EncryptStringByRsaJS(fundReq.Username, modulus, exponent, "20");
                Url = baseUrl + "PublicQryGJJReg.do";
                _postData = string.Format("GjjLoginPassword={0}&_vTokenName={1}&ChannelId=05",fundReq.Password,fundReq.Vercode );
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata = _postData,
                    Encoding = Encoding.GetEncoding("utf-8"),
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                #endregion
            }
            catch (Exception e)
            {

                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }
    }
}
