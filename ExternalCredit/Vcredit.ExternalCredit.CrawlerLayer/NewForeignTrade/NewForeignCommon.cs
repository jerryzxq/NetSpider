using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public class NewForeignCommon
    {
        public const string czTrue = "1";

        public const string czFalse= "0";
        /// <summary>
        /// 新外贸接口错误代码
        /// </summary>
        public readonly  Dictionary<string, string> NewExtCreditErrorDic = new Dictionary<string, string>(){
            {"0000", "成功"},
            {"0001", "成功，但存在错误"},
            {"9999", "此服务异常"},
            {"9000", "服务不存在"},
            {"9001", "暂停服务"},
            {"9002", "数据格式错误"},
            {"9003", "无使用权限"},
            {"9004", "输入参数非法"},
            {"1001", "合同号不存在"},
            {"1002", "合同号已存在"},
            {"1003", "合作机构号错误"},
            {"1004", "查询记录不存在"},
            {"2999", "插入批次号重复"},
            {"2990", "查询批次号不存在"},
            {"2991", "实收信息不存在"},
            {"2992", "该时间段的信息不存在"},
            {"6101", "盈高登录失败"},
            {"6102", "盈高登录发生异常"},
            {"6103", "盈高登录网络异常"},
            {"6201", "人行登录失败"},
            {"6202", "人行登录失败，请检查链接"},
            {"6203", "人行登录超时，请检查链接"},
            {"6204", "人行无法链接，请检查远程链接"},
            {"6205", "人行登录失败其他问题"},
            {"6301", "登录失败,找不到人行对应的操作员或者操作员处于失效状态"},
            {"6302", "登录失败,请检查人行系统是否可以使用!"},
            {"6401", "报文文件上传失败"},
            {"6501", "更新查征信息异常"},
            {"6602", "解析存储异常"},
            {"6701", "json组装异常"},
        };
        public  int GetFileState(string certno)
        {
            int state = (int)AuthorizationFileState.Default;
            if(!Directory.Exists(ConfigData.requestFileSavePath))
            {
                return state;
            }
            var sqfile = Directory.GetFiles(ConfigData.requestFileSavePath, "SQ_" + certno + "*");
            var zjfile = Directory.GetFiles(ConfigData.requestFileSavePath, "ZJ_" + certno + "*");
      
            if (sqfile.Count()!=0&&zjfile.Count()!=0)
            {
                state = (int)AuthorizationFileState.ReceiveSuccess;
            }
            return  state ;

        }
        
        public  ResponeEntity GetRequestResult(string  txCode,string json)
        {
            ResponeEntity resp = null;
            try
            {
                var req = CreateReqEntity(txCode, json);
                var returnobj = new WebServiceProxy(ConfigData.ForeignWebserviceUrl, "InfWsSearch").ExecuteQuery("search", req, AssignmentForReqest);
                resp = GetRepEntity(returnobj);

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(json + "请求时出现异常", ex);
                return null;
            }
            return resp;
        }
        public  RequestEntity CreateReqEntity(string txCode, string content)
        {
            string[] dateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss").Split(' ');
            
            RequestEntity req = new RequestEntity() {
                txCode = txCode,
                reqDate =dateTime[0],
                reqTime=dateTime[1],
                token=CommonFun.GetGuidID(),
                reqSerial=CommonFun.GetTimeStamp(),
                content=content,
                brNo=ConfigData.orgCode,

            };
            return req;
        }
        public  void AssignmentForReqest(RequestEntity req, object apireqestentity)
        {
            if (apireqestentity == null)
                return;
            foreach (var item in apireqestentity.GetType().GetProperties())
            {
                switch (item.Name)
                {

                    case "txCode":
                        item.SetValue(apireqestentity, req.txCode);
                        break;
                    case "reqDate":
                        item.SetValue(apireqestentity, req.reqDate);
                        break;
                    case "reqTime":
                        item.SetValue(apireqestentity, req.reqTime);
                        break;
                    case "token":
                        item.SetValue(apireqestentity, req.token);
                        break;
                    case "reqSerial":
                        item.SetValue(apireqestentity, req.reqSerial);
                        break;
                    case "content":
                        item.SetValue(apireqestentity, req.content);
                        break;
                    case "brNo":
                        item.SetValue(apireqestentity, req.brNo);
                        break;
                }
            }
        }
        public  ResponeEntity GetRepEntity(object apiReturnObj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(apiReturnObj);
                return JsonConvert.DeserializeObject<ResponeEntity>(json);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("解析查询接口返回时出现异常", ex);
                return null;
            }
        }
        
    }
}
