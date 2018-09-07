using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    /// <summary>
    /// 单个用户征信请求
    /// </summary>
    public class SingleRequest
    {
        readonly static  NewForeignCommon comon = new NewForeignCommon();
        readonly static CRD_CD_CreditUserInfoBusiness credit = new CRD_CD_CreditUserInfoBusiness();
        const string  interfaceCode = "4101";
        public void SubmitReqest(SingleCreditReq sreq)
        {
            sreq.czPactNo = GetczPactNo();
            sreq.appDate = DateTime.Now.ToString("yyyyMMdd");
            sreq.brNo = ConfigData.orgCode;
            ForeignComBus foreignComBus = new ForeignComBus();
            CRD_CD_CreditUserInfoEntity creditEntity = new CRD_CD_CreditUserInfoEntity()
            {
                Query_Org = ConfigData.orgCode,
                Name = sreq.cifName,
                Cert_Type = sreq.idType,
                Cert_No = sreq.idNo,
                PactNo = sreq.czPactNo,
                Authorization_Date = sreq.appDate,
                czAuth = sreq.czAuth==NewForeignCommon.czTrue,
                czId = sreq.czId==NewForeignCommon.czTrue,
                SourceType = (byte)SysEnums.SourceType.Trade,
                State = (byte)RequestState.UpLoading,
                BusType =sreq.BusType,
                UpdateTime=DateTime.Now,
                FileState= comon.GetFileState(sreq.idNo),
                QueryReason = sreq.crpReason,
                ApplyID = sreq.ApplyID
            };
            credit.Save(creditEntity);
            Task.Factory.StartNew(() =>
            {
                DealingRequest(sreq, foreignComBus, creditEntity);
            }).ContinueWith(e=>e.Dispose());
        }
        public static void  ReceiveCreditInfo(SingleCreditReq sreq)
        {
            sreq.czPactNo = GetczPactNo();
            sreq.appDate = DateTime.Now.ToString("yyyyMMdd");
            CRD_CD_CreditUserInfoEntity creditEntity = new CRD_CD_CreditUserInfoEntity()
            {
                Query_Org = ConfigData.orgCode,
                Name = sreq.cifName,
                Cert_Type = sreq.idType,
                Cert_No = sreq.idNo,
                PactNo =sreq.czPactNo,
                Authorization_Date = sreq.appDate,
                czAuth = sreq.czAuth == NewForeignCommon.czTrue,
                czId = sreq.czId == NewForeignCommon.czTrue,
                SourceType = (byte)SysEnums.SourceType.Trade,
                State = (byte)RequestState.Default,
                BusType=sreq.BusType,
                UpdateTime = DateTime.Now,
                FileState = (int)AuthorizationFileState.Default,
                QueryReason = sreq.crpReason,
                ApplyID=sreq.ApplyID,
                grantType =sreq.grantType

            };
            credit.Save(creditEntity);
        }

        private void DealingRequest(SingleCreditReq sreq, ForeignComBus foreignComBus, CRD_CD_CreditUserInfoEntity creditEntity)
        {
            var resp = comon.GetRequestResult(interfaceCode, JsonConvert.SerializeObject(sreq));
            string log = string.Empty;
            if (resp == null)
            {
                log = "单笔上传接口无法连接";
                SaveCredit(creditEntity, (byte)RequestState.UpLoadFail, "单笔上传接口无法连接");
            }
            else
            {
                if (resp.respCode != comon.NewExtCreditErrorDic.First().Key)
                {
                    string error = sreq.idNo + "请求出现错误信息错误信息：" + resp.respDesc + ",错误代码：" + resp.respCode;
                    log = error;
                    SaveCredit(creditEntity, (byte)RequestState.QueryFail, error);
                }
                if(!string.IsNullOrEmpty(resp.content))
                {
                    log = "成功获取,描述：" + resp.respDesc + ",代码:" + resp.respCode;
                    Tuple<Message, List<CRD_HD_REPORTEntity>> message = null;
                    try
                    {
                        message = NewForeignTrade.SavetoDb(foreignComBus, resp.content, creditEntity.BatNo, creditEntity.Query_Org);
                    }
                    catch (Exception ex)
                    {
                        Log4netAdapter.WriteError(log, ex);
                        SaveCredit(creditEntity, (byte)RequestState.AnalysisFail, ex.Message);
                    }
                    if (message.Item1.ErrorDataList != null && message.Item1.ErrorDataList.Count != 0)
                    {
                        SaveCredit(creditEntity, (byte)RequestState.QueryFail, message.Item1.ErrorDataList[0].ErrorDes + message.Item1.ErrorDataList[0].ErrorCode);
                    }
                }
            }

            Log4netAdapter.WriteInfo(log);
        }

        private  void SaveCredit( CRD_CD_CreditUserInfoEntity creditEntity,byte state,string error)
        {
            creditEntity.State = state;
            creditEntity.Error_Reason = error;
            creditEntity.UpdateTime = DateTime.Now;
            credit.Save(creditEntity);
        }
        private static string GetczPactNo()
        {

            return CommonFun.GetGuidID();
        }
    }
}
