using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Vcredit.Common;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.HL
{
    public class qiqihaer : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.qqhr.gov.cn/";
        string fundCity = "hl_qiqihaer";
        ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
        ProvidentFundDetail detail = null;
        List<string> results = new List<string>();
        #endregion
        #region 私有变量
        decimal perAccounting = 0;//个人占比
        decimal comAccounting = 0;//公司占比
        decimal totalRate = 0;//总缴费比率
        decimal payRate = (decimal)0.08;
        int PaymentMonths = 0;
        Regex reg = new Regex(@"[\s;\&nbsp;\,;\%]");
        #endregion
        public Entity.Service.VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes vcRes = new VerCodeRes();
            string token = CommonFun.GetGuidID();
            vcRes.Token = token;
            try
            {
                vcRes.StatusCode = ServiceConsts.StatusCode_success;
                vcRes.StatusDescription = "所选城市无需初始化";
                CacheHelper.SetCache(token, cookies);
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
            string Url = string.Empty;
            string postdata = string.Empty;
            string cxyd = string.Empty;
            string cxydmc = string.Empty;
            string dbname = string.Empty;
            string zgzh = string.Empty;
            string dwbm = string.Empty;
            try
            {
                //获取缓存
                if (CacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(fundReq.Token);
                    CacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                //15位或18位身份证验证
                Regex regex = new Regex(@"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$");
                //if (regex.IsMatch(fundReq.Identitycard) == false)
                //{
                //    Res.StatusDescription = "输入的身份证号长度不对，或者号码不符合规定！15位号码应全为数字，18位号码末位可以为数字或X。";
                //    Res.StatusCode = ServiceConsts.StatusCode_fail;
                //    return Res;
                //}
                if (string.IsNullOrWhiteSpace(fundReq.Username))
                {
                    Res.StatusDescription = "用户名不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                if (string.IsNullOrWhiteSpace(fundReq.Password))
                {
                    Res.StatusDescription = "密码不能为空!";
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                #region 第一步,登陆
                Url = "http://www1.qqhr.gov.cn/zhuanti/Search_G.php";
                postdata = string.Format("select=code&text={0}&pws={1}&Submit=%CC%E1%BD%BB",fundReq.Username,fundReq.Password);
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Method = "Post",
                    Postdata=postdata,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
             
                #endregion
                #region 第二步,获取基本信息
                //在第一步中已经获取基本信息
                results = HtmlParser.GetResultFromParser(httpResult.Html, "//table[@bgcolor='#CCCCCC']/tr[2]/td/div", "");
                if (results.Count > 0)
                {
                    Res.ProvidentFundNo = results[0].Replace("\r","").Replace("\n","").Replace(" ","");  //公积金账号
                    Res.Name = results[1].Replace("\r", "").Replace("\n", "").Replace(" ", "");  //姓名
                    Res.CompanyName = results[2].Replace("\r", "").Replace("\n", "").Replace(" ", "");  //公司名称
                    Res.IdentityCard = results[3].Replace("\r", "").Replace("\n", "").Replace(" ", "");  //身份证号
                    Res.TotalAmount = results[4].Replace("\r", "").Replace("\n", "").Replace(" ", "").ToDecimal(0);  //余额
                    Res.Status = results[5].Replace("\r", "").Replace("\n", "").Replace(" ", "");  //状态
                }
                #endregion
                #region 第三步，**********查询缴费明细(暂无缴费明细)****************

                #endregion
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
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
