
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using VVcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer
{
    public class PushData
    {
        public Vcredit.ExternalCredit.CommonLayer.SysEnums.SourceType SourceType { get; set; }
        public string Cert_No { get; set; }
        public string ReportSn { get; set; }
        public long? ReportId { get; set; }
        public DateTime? Report_Create_Time { get; set; }
        public RequestState State { get; set; }
        [JsonIgnore]
        public string BusType { get; set; }


    }
    class InputData
    {
        public string FK_APPId { get; set; }
        public string FK_ProductType { get; set; }
        public string FK_Param { get; set; }
    }
    class Token
    {
        public string accesstoken { get; set; }
        public string errorCode { get; set; }

    }
    class Respone
    {
        public string data { get; set; }
        public string errorCode { get; set; }
    }
    public class FKYResp<T>
    {

        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public T Result { get; set; }

    }

    public class CreditInfoPush
    {

        string CallBackurl;

        string GetTokenUrl;

        public static readonly CreditInfoPush current = new CreditInfoPush();
        public CreditInfoPush()
        {

            this.CallBackurl = ConfigurationManager.AppSettings["CallBackurl"];

            this.GetTokenUrl = ConfigurationManager.AppSettings["GetTokenUrl"];
            if (CallBackurl == null || GetTokenUrl == null)
                throw new Exception("地址不能为空");
             
        }
        BaseDao dao = new BaseDao();
        CRD_CD_CreditUserInfoEntity GetCreditWith(string Cert_no, int sourceType)
        {
            return dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.SourceType == (byte)sourceType && x.Cert_No == Cert_no).FirstOrDefault();
        }

        List<CRD_CD_CreditUserInfoEntity> GetExtCreditListWith(string batNO)
        {
            return dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.SourceType == (byte)SysEnums.SourceType.Trade && x.BatNo == batNO);
        }

        NameValueCollection GetConfigInfoByNameSpace(string nameSpace)
        {
            return ApolloConfig.GetPropertyNameValueCollection(nameSpace);
        }
        HttpResult PostDataToUrl(string url, string data)
        {
            HttpItem httpItem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                PostEncoding = Encoding.UTF8,
                Postdata = data,
                ContentType = "application/json",
                Accept = "application/json"

            };
            return new HttpHelper().GetHtml(httpItem);

        }
        string GetAppid(string cert_no, int sourcetype, ref  CRD_CD_CreditUserInfoEntity credit)
        {
            var bustTypeUrlList = GetConfigInfoByNameSpace("OrgCredit.PushUrl");
            credit = GetCreditWith(cert_no, (int)sourcetype);
            if (credit == null)
            {
                Log4netAdapter.WriteInfo(string.Format("cert_no {0},SourceType{1}无法获取状态", cert_no, sourcetype.ToString()));
                return string.Empty;
            }
            if (!bustTypeUrlList.AllKeys.Contains(credit.BusType))
            {
                Log4netAdapter.WriteInfo(string.Format("bustType:{0}没有推送链接", credit.BusType));
                return string.Empty;
            }
            return bustTypeUrlList[credit.BusType];
        }
        string GetUrl()
        {
            var value = RedisHelper.GetCache("Token", "PushCredit");
            if (value == null)
            {
                HttpItem httpItem = new HttpItem()
                {
                    URL = GetTokenUrl,
                    Method = "GET",

                };
                var result = new HttpHelper().GetHtml(httpItem);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var token = JsonConvert.DeserializeObject<Token>(result.Html);
                    if (token == null)
                    {
                        Log4netAdapter.WriteInfo("获取token为null");
                        return string.Empty;

                    }
                    if (!string.IsNullOrEmpty(token.accesstoken))
                    {
                        RedisHelper.SetCache("Token", token.accesstoken, "PushCredit", 18);
                        return CallBackurl + token.accesstoken;
                    }
                    else
                    {
                        Log4netAdapter.WriteInfo("获取token为空");
                        return string.Empty;
                    }
                }
                else
                {
                    Log4netAdapter.WriteInfo("获取token接口不通");
                    return string.Empty;

                }
            }
            else
            {
                return CallBackurl + value.ToString();
            }

        }
        private string GetData(string appid, PushData[] pushInfolist)
        {
            return JsonConvert.SerializeObject(new InputData { FK_APPId = appid, FK_ProductType = "FK/FOREIGNCREDITREPORT", FK_Param = JsonConvert.SerializeObject(pushInfolist) });
        }
        //public void PushCredit(PushData pushInfo)
        //{
        //    CRD_CD_CreditUserInfoEntity credit = null;
        //    try
        //    {
        //        var url = GetUrl();
        //        if (url == string.Empty)
        //            return;
        //        var appid = GetAppid(pushInfo.Cert_No, (int)pushInfo.SourceType, ref  credit);
        //        if (appid == string.Empty)
        //            return;
        //        var pushInfolist = new PushData[] { pushInfo };
        //        var httpResult = PostDataToUrl(url, GetData(appid, pushInfolist));
        //        if (httpResult.StatusCode == HttpStatusCode.OK)
        //        {
        //            var baseRes = JsonConvert.DeserializeObject<Respone>(httpResult.Html);
        //            if (baseRes.data != null)
        //            {
        //                var resp = JsonConvert.DeserializeObject<FKYResp<FKYResp<string>>>(baseRes.data);
        //                if (resp.StatusCode == 0 && resp.Result != null && resp.Result.StatusCode == 0)
        //                {
        //                    Log4netAdapter.WriteInfo(string.Format("bustType:{0},cert_NO:{1}", credit.BusType, pushInfo.Cert_No));
        //                }
        //                else
        //                {
        //                    Log4netAdapter.WriteInfo(httpResult.Html + "Cert_NO:" + pushInfo.Cert_No + "请求失败");
        //                }
        //            }
        //            else
        //            {
        //                Log4netAdapter.WriteInfo(pushInfo.Cert_No + "推送失敗");
        //            }

        //        }
        //        else
        //        {
        //            Log4netAdapter.WriteInfo(httpResult.Html + "Cert_NO:" + pushInfo.Cert_No + "http请求失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log4netAdapter.WriteError("BusType:" + credit == null ? string.Empty : credit.BusType + "SourceType：" + pushInfo.SourceType + ".Cert_NO:" + pushInfo.Cert_No + "推送出现异常", ex);
        //    }

        //}
        public void PushExtCredit(List<CRD_HD_REPORTEntity> reportList, List<string> errorCert_NoList, string batno)
        {
            try
            {
                if (errorCert_NoList != null && errorCert_NoList.Count != 0)
                {
                    reportList.Union(errorCert_NoList.Select(x => new CRD_HD_REPORTEntity
                    {
                        Cert_No = x,
                    }));
                }
                var creditList = GetExtCreditListWith(batno);
               // var bustTypeUrlList = GetConfigInfoByNameSpace("OrgCredit.PushUrl");
               // creditList = creditList.Where(x => bustTypeUrlList.AllKeys.Contains(x.BusType)).ToList();
                if (creditList.Count == 0)
                {
                    Log4netAdapter.WriteInfo("BatNo" + batno + "没有推送的数据");
                    return;
                }
                if (reportList.Count != creditList.Count)
                    Log4netAdapter.WriteInfo(string.Format("BatNo{0}的数量{1}不等于返回的数量{2}", batno, creditList.Count, reportList.Count));

                var pushInfoList = from r in reportList
                                   from c in creditList
                                   where r.Cert_No == c.Cert_No
                                   select new PushData
                                   {
                                       Cert_No = r.Cert_No,
                                       Report_Create_Time = r.Report_Create_Time,
                                       ReportId = (long)r.Report_Id,
                                       ReportSn = r.Report_Sn,
                                       State = (RequestState)c.State,
                                       SourceType =Vcredit.ExternalCredit.CommonLayer.SysEnums.SourceType.Trade,
                                       BusType = c.BusType,
                                   };

                var busTypeGroups = pushInfoList.GroupBy(x => x.BusType);
                foreach (var item in busTypeGroups)
                {
                    var httpResult = PostDataToUrl(GetUrl(), GetData(item.Key, item.ToArray<PushData>()));

                    if (httpResult.StatusCode == HttpStatusCode.OK)
                    {
                        var baseRes = JsonConvert.DeserializeObject<Respone>(httpResult.Html);
                        if (baseRes.data != null)
                        {
                            var resp = JsonConvert.DeserializeObject<FKYResp<FKYResp<string>>>(baseRes.data);
                            if (resp.StatusCode == 0 && resp.Result != null && resp.Result.StatusCode == 0)
                            {
                                Log4netAdapter.WriteInfo(item.Key + "推送成功" + batno);
                            }
                            else
                            {
                                Log4netAdapter.WriteInfo(item.Key + "请求失敗" + batno+httpResult.Html);
                            }
                        }
                        else
                        {
                            Log4netAdapter.WriteInfo(item.Key + "推送失败" + batno+httpResult.Html);
                        }
                    }
                    else
                    {
                        Log4netAdapter.WriteInfo(string.Format("外贸的bustType:{0}，BatNo:{1},推送失败：{2}", item.Key, batno, httpResult.Html));
                    }
                }
            }
            catch (Exception ex)
            {

                Log4netAdapter.WriteError("外贸的批次号" + batno + "推送出现异常", ex);
            }
             
        }
    }
}
