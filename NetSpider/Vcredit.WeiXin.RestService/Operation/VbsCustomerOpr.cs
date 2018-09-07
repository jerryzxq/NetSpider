using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.PluginManager;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.Common.Ext;
using System.Net;

namespace Vcredit.WeiXin.RestService.Operation
{
    public class VbsCustomerOpr
    {
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string serverUrl = ConfigurationHelper.GetAppSetting("vbsService");
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口

        /// <summary>
        /// 根据客户身份证，查询此客户的所有VBS历史订单号
        /// </summary>
        /// <param name="identityNo">客户身份证</param>
        /// <returns></returns>
        public List<int> GetCustomerHistoryRecord(string identityNo)
        {
            List<int> BidList = new List<int>();
            try
            {
                serverUrl += "/GateWay/VcreditNPGateway.asmx/GetHistoryRecord";
                httpItem = new HttpItem
                {
                    URL = serverUrl,
                    Method = "POST",
                    Postdata = "identityNo=" + identityNo
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    List<BusinessOverview> businessList = jsonService.DeserializeObject<List<BusinessOverview>>(jsonService.GetResultFromParser(httpResult.Html, "Content"));

                    if (businessList != null)
                    {
                        BidList = businessList.Select(o => o.Bid).ToList();
                    }
                }
            }
            catch (Exception e)
            { }

            return BidList;
        }

        public int CheckCustomer(string identityNo, string customerName)
        {
            int isblack = 1;
            try
            {
                serverUrl += "/GateWay/VcreditNPGateway.asmx/CheckCustomer";
                httpItem = new HttpItem
                {
                    URL = serverUrl,
                    Method = "POST",
                    Postdata = string.Format("identityNo={0}&customerName={1}", identityNo, customerName.ToUrlEncode())
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode == HttpStatusCode.OK)
                {
                    int result = jsonService.GetResultFromParser(httpResult.Html, "Content").ToInt(0);

                    Log4netAdapter.WriteInfo(string.Format("VBS_CheckCustomer—>身份证号:{0},姓名:{1},检测结果:{2}", identityNo, customerName, result));
                    if (result <= 0)
                    {
                        isblack = 0;
                    }
                }
                else
                {
                    isblack = -1;
                }

                return isblack;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(string.Format("VBS_CheckCustomer,ERROR—>身份证号:{0},姓名:{1}", identityNo, customerName),e);
                throw new Exception(e.Message);
            }
        }
    }
}
