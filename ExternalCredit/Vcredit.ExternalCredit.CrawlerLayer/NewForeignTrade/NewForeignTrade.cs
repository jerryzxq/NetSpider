using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.Common.Utility;
using System.IO;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.Common.Helper;
using Vcredit.ExtTrade.ModelLayer.Common;
namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public class NewForeignTrade
    {

        readonly NewForeignCommon common = new NewForeignCommon();
        protected readonly CRD_CD_CreditUserInfoBusiness credit = new CRD_CD_CreditUserInfoBusiness();
        readonly ForeignComBus fcb = new ForeignComBus();
        const string interfaceCode = "4103";
        public void GetForeignCredit()
        {
            var queryList = GetqueryQequest();// new  List<QueryResult>(){new QueryResult()};
            foreach (var query in queryList)
            {
                try
                {
                    string json = DealingRequest(query);// System.IO.File.ReadAllText(@"D:\json.txt", Encoding.GetEncoding("GBK"));

                    if(json == string.Empty)
                    {
                        continue;
                    }
                    var message = SavetoDb(fcb, json, query.batchNo, query.brNo);
                    if (message.Item1.ErrorDataList != null && message.Item1.ErrorDataList.Count != 0)
                    {
                        credit.UpdateListStateByBatchNoCertNo(UpdateQueryFailData(message.Item1.ErrorDataList), query.batchNo);
                    }
                    List<string> errorList=null;
                    if(message.Item1.ErrorDataList!=null)
                        errorList =message.Item1.ErrorDataList.Select(x=>x.Cert_NO).ToList();
                    if (message.Item2.Count != 0 || (message.Item1.ErrorDataList != null && message.Item1.ErrorDataList.Count != 0))
                        CreditInfoPush.current.PushExtCredit(message.Item2, errorList, query.batchNo);//推送数据
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("新外贸接口出现异常,批次号：" + query.batchNo, ex);
                }
            }
        }
        private string DealingRequest(QueryResult queryResult)
        {
            var resp = common.GetRequestResult(interfaceCode, JsonConvert.SerializeObject(queryResult));
            string json = string.Empty;
            string log = string.Empty;
            if (resp == null)
            {
                log = "webservice连接出现异常";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(resp.content))
                {
                    json = resp.content;
                }
                log = "批次号：" + queryResult.batchNo + "，请求描述信息：" + resp.respDesc + ",代码：" + resp.respCode+",身份证号："+queryResult.idNo;
            }
            Log4netAdapter.WriteInfo(log);
            // Log4netAdapter.WriteInfo("报文："+json);
            return json;
        }

        protected virtual List<QueryResult> GetqueryQequest()
        {
            List<QueryResult> queList = new List<QueryResult>();
            foreach (var item in credit.GetBatchNos())
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                QueryResult query = new QueryResult()
                {
                    batchNo = item,
                    brNo = ConfigData.orgCode
                };
                queList.Add(query);
            }
            return queList;
        }
        private List<CRD_CD_CreditUserInfoEntity> UpdateQueryFailData(List<ErrorData> errorlist)
        {
            List<CRD_CD_CreditUserInfoEntity> creditlist = new List<CRD_CD_CreditUserInfoEntity>();
            foreach (var item in errorlist)
            {
                CRD_CD_CreditUserInfoEntity entity = new CRD_CD_CreditUserInfoEntity()
                {
                    State = (byte)RequestState.QueryFail,
                    Error_Reason = item.ErrorDes + item.ErrorCode,
                    Cert_No = item.Cert_NO,
                    Cert_Type = item.Cert_Type,
                    UpdateTime = DateTime.Now
                };
                creditlist.Add(entity);
            }
            return creditlist;
        }

        public static Tuple<Message,List<CRD_HD_REPORTEntity>>  SavetoDb(ForeignComBus foreignBus, string json, string batchno, string brno)
        {
            List<CRD_HD_REPORTEntity> List = new List<CRD_HD_REPORTEntity>();
            Dictionary<string,long> dic =new  Dictionary<string,long>();
            var message = JsonConvert.DeserializeObject<Message>(json);

            if (message.CreditDataList != null && message.CreditDataList.Count != 0)
            {
                DealwithNewForeignContainer(message.CreditDataList);
                List = foreignBus.SaveNewForeignCreditInfoToDB(message.CreditDataList, batchno, brno);//入库和修改状态
                if (List.Count != 0)//成功获取
                {

                    List.Where(x => x.Report_Id != 0).ToList().ForEach(x => dic.Add(x.Report_Sn, (long)x.Report_Id));
                    Func<ExtTradeJson> fun = () => { return new ExtTradeJson { ExtJson = json, ReportIdList = dic }; };
                    SengData.SendAcction(fun, batchno);
                }
            }
            return new  Tuple<Message,List<CRD_HD_REPORTEntity>>(message,List);
        }


        public static void DealwithNewForeignContainer(List<NewForeignContainer> foreignList)
        {
            if (foreignList == null || foreignList.Count == 0)
                return;
            foreach (var item in foreignList)
            {
                if (item == null)
                {
                    continue;
                }
                DealwithLn(item.CRD_CD_LN, item.CRD_CD_LN_OVD, item.CRD_CD_LN_SPL);
                DealwithLnd(item.CRD_CD_LND, item.CRD_CD_LND_OVD, item.CRD_CD_LND_SPL);
                DealwithStncard(item.CRD_CD_STNCARD, item.CRD_CD_STN_OVD, item.CRD_CD_STN_SPL);
            }
        }
        private static void DealwithLn(List<CRD_CD_LNEntity> lnList, List<CRD_CD_LN_OVDEntity> lnovdList, List<CRD_CD_LN_SPLEntity> lnsplList)
        {
            if (lnList == null || ((lnovdList == null || lnovdList.Count == 0) && (lnsplList == null || lnsplList.Count == 0)))
                return;
            foreach (var item in lnList)
            {
                if (lnovdList != null && lnovdList.Count != 0)
                {
                    item.LnoverList = lnovdList.Where(x => x.Number == item.Number).ToList();
                }
                if (lnsplList != null && lnsplList.Count != 0)
                {
                    item.LnSPLList = lnsplList.Where(x => x.Number == item.Number).ToList();
                }
            }
        }
        private static void DealwithLnd(List<CRD_CD_LNDEntity> lndist, List<CRD_CD_LND_OVDEntity> lndovdList, List<CRD_CD_LND_SPLEntity> lndsplList)
        {

            if (lndist == null || ((lndovdList == null || lndovdList.Count == 0) && (lndsplList == null || lndsplList.Count == 0)))
                return;
            foreach (var item in lndist)
            {
                if (lndovdList != null && lndovdList.Count != 0)
                {
                    item.LndoverList = lndovdList.Where(x => x.Number == item.Number).ToList();
                }
                if (lndsplList != null && lndsplList.Count != 0)
                {
                    item.LndSPLList = lndsplList.Where(x => x.Number == item.Number).ToList();
                }
                // GetRepair_paymentstate(item,item.LndoverList);
            }
        }
        //private static void GetRepair_paymentstate(CRD_CD_LNDEntity lnd,List<CRD_CD_LND_OVDEntity> ovrList)
        //{
        //    if(string.IsNullOrEmpty(lnd.Payment_State)&&lnd.State!="未激活"&&lnd.GetTime!=null)   
        //    {
        //        if(ovrList==null ||ovrList.Count==0)
        //        {
        //            lnd.Repair_Payment_State = "************************";

        //            return;
        //        }
        //        StringBuilder sb = new StringBuilder();
        //        for(int i=24;i>0;i--)
        //        {
        //            var ovrentity= ovrList.Where(x => x.Month_Dw == lnd.GetTime.Value.AddMonths(-i).ToString("yyyy.MM")).FirstOrDefault();
        //            if(ovrentity!=null)
        //            {
        //                sb.Append((ovrentity.Last_Months ?? 0).ToString());
        //            }
        //            else
        //            {
        //                sb.Append("*");
        //            }
        //        }
        //        lnd.Repair_Payment_State = sb.ToString();
        //    }
        //}
        private static void DealwithStncard(List<CRD_CD_STNCARDEntity> stnList, List<CRD_CD_STN_OVDEntity> stnovdList, List<CRD_CD_STN_SPLEntity> stnsplList)
        {

            if (stnList == null || ((stnovdList == null || stnovdList.Count == 0) && (stnsplList == null || stnsplList.Count == 0)))
                return;
            foreach (var item in stnList)
            {
                if (stnovdList != null && stnovdList.Count != 0)
                {
                    item.StnoverList = stnovdList.Where(x => x.Number == item.Number).ToList();
                }
                if (stnsplList != null && stnsplList.Count != 0)
                {
                    item.StnSPLList = stnsplList.Where(x => x.Number == item.Number).ToList();
                }
            }
        }

    }
}
