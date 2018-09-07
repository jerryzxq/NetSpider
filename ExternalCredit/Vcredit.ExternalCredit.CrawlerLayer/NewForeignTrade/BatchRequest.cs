using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    /// <summary>
    /// 批量请求数据
    /// </summary>
    public class BatchRequest
    {
        readonly  CRD_CD_CreditUserInfoBusiness credit = new CRD_CD_CreditUserInfoBusiness();
        readonly  NewForeignCommon common = new NewForeignCommon();
        readonly ForeignComBus fcb = new ForeignComBus();
        const string interfaceCode = "4102";
        public void SubmitBatchRequest()
        {
            var cuinfoList = credit.GetAllList();
            GetBatchCredit(cuinfoList);
     
        }

        private void GetBatchCredit(List<CRD_CD_CreditUserInfoEntity> cuinfoList)
        {
            string batchNo = string.Empty;
            string json = string.Empty;
            try
            {
                ManyCreditReq creditReq = new ManyCreditReq();
                if (cuinfoList.Count == 0)
                {
                    Log4netAdapter.WriteInfo("没有需要上传的数据");
                    return;
                }
                credit.UpdateState(cuinfoList, (byte)RequestState.UpLoading);//正在上传......
                batchNo = InitialCreditreq(cuinfoList, creditReq);//获取批量请求数据
             
                json = DealingRequest(creditReq, cuinfoList);
                if (json == string.Empty)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                credit.UpdateState(cuinfoList, (byte)RequestState.UpLoadFail);//修改上传失败
                Log4netAdapter.WriteError("上传时出现异常", ex);
                return;
            }
            DealingResponResult(cuinfoList, batchNo, json);//处理返回的结果
        }
        private string DealingRequest(ManyCreditReq manyCreditReq, List<CRD_CD_CreditUserInfoEntity> creditList)
        {
            var jSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var resp = common.GetRequestResult(interfaceCode, JsonConvert.SerializeObject(manyCreditReq, jSetting));
             
            string json = string.Empty;
            string log = string.Empty;
            if (resp == null)
            {
                log = "批量上传接口无法连接";
                creditList.ForEach(x => x.Error_Reason = log);
                credit.UpdateState(creditList, (byte)RequestState.UpLoadFail);
            }
            else
            {
                if (resp.respCode != common.NewExtCreditErrorDic.Keys.First() && resp.respCode != common.NewExtCreditErrorDic.Keys.ElementAt(1))
                {
                    log = "批次号：" + manyCreditReq.batchNo + "，请求出现错误信息错误信息：" + resp.respDesc + ",错误代码：" + resp.respCode;
                    creditList.ForEach(x => x.Error_Reason = log);
                    credit.UpdateState(creditList, (byte)RequestState.UpLoadFail);
                }
                if(!string.IsNullOrEmpty(resp.content))
                {
                    log = resp.content;
                    json = resp.content;
                }
            }

            Log4netAdapter.WriteInfo(log);
            return json;
        }

        private void DealingResponResult(List<CRD_CD_CreditUserInfoEntity> cuinfoList, string batchNo, string json)
        {

            var batEntity = JsonConvert.DeserializeObject<BatchResEntity>(json);
            if (batEntity.batchCount != cuinfoList.Count.ToString() ||batEntity.batchNo != batchNo)
            {
                Log4netAdapter.WriteInfo(string.Format("批次号获取提交的数量不一致提交的批次号：{0},获取到的批次号：{1}，提交的数量：{2}，获取到的数量{3}", batchNo, batEntity.batchNo, cuinfoList.Count.ToString(), batEntity.batchCount));
            }
            if (batEntity.errorList == null || batEntity.errorList.Count == 0)
            {
                cuinfoList.ForEach(x => { x.BatNo = batchNo; x.Error_Reason = null;});//给批次号。
                credit.UpdateState(cuinfoList, (byte)RequestState.UpLoadSuccess);//上传成功
            }
            else//有错误信息的情况
            {
                DealIngHaveFail(cuinfoList, batEntity);

            }
        }

        private void DealIngHaveFail(List<CRD_CD_CreditUserInfoEntity> cuinfoList, BatchResEntity batEntity)
        {
            List<CRD_CD_CreditUserInfoEntity> failList = new List<CRD_CD_CreditUserInfoEntity>();
            List<CRD_CD_CreditUserInfoEntity> sucessList = new List<CRD_CD_CreditUserInfoEntity>();
            foreach (var item in cuinfoList)
            {
                if (batEntity.errorList.Exists(x=>x.idNo==item.Cert_No))
                {
                    item.Error_Reason =batEntity.errorList.Find(x=>x.idNo==item.Cert_No).dealDesc;
                    failList.Add(item);
                }
                else
                {
                    item.BatNo = batEntity.batchNo;
                    sucessList.Add(item);
                }
            }
            if (failList.Count != 0)
                credit.UpdateState(failList, (byte)RequestState.UpLoadFail);//上传失败
            if (sucessList.Count != 0)
                credit.UpdateState(sucessList, (byte)RequestState.UpLoadSuccess);//上传成功
        }

        private  string InitialCreditreq(List<CRD_CD_CreditUserInfoEntity> cuinfoList, ManyCreditReq creditReq)
        {
            string batchNo = ConfigData.orgCode+CommonFun.GetTimeStamp();
            creditReq.dataCnt = cuinfoList.Count;
            creditReq.batchNo = batchNo;
            creditReq.brNo = ConfigData.orgCode;
            foreach (var item in cuinfoList)
            {
                SingleCreditReq singleReq = new SingleCreditReq()
                {
                    cifName = item.Name,
                    idType = item.Cert_Type,
                    idNo = item.Cert_No,
                    czPactNo = item.PactNo,
                    appDate = item.Authorization_Date,
                    czAuth = item.czAuth ?? true ? NewForeignCommon.czTrue : NewForeignCommon.czFalse,
                    czId = item.czAuth ?? true ? NewForeignCommon.czTrue : NewForeignCommon.czFalse,
                    crpReason = item.QueryReason,
                    grantType = item.grantType

                };
                creditReq.singleReqList.Add(singleReq);
            }
            return batchNo;
        }
      
    }
}
