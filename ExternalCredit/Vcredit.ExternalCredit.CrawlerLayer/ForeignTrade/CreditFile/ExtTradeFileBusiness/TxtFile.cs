using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public class TxtFile : AbastractFileBase
    {

        OperatorLog opeLog = new OperatorLog(FileType.TxtFile);
        Dictionary<string, List<string>> ReadTxtFile()
        {
            Dictionary<string, List<string>> listDic = new Dictionary<string, List<string>>();
            var fileInfos = GetLocalFileInfo(".txt");
            foreach (var item in fileInfos)
            {
                if(!listDic.ContainsKey(item.FullName))
                {
                    listDic.Add(item.FullName,System.IO.File.ReadLines(item.FullName).ToList());
                }
            }
            opeLog.SetOperateTotalNum(listDic.Count);//设置总条数
            return listDic;     
        }



        Dictionary<string, List<CRD_CD_QueryResultEntity>> ConvertToEntityList(Dictionary<string, List<string>> listdic)
        {
            Dictionary<string, List<CRD_CD_QueryResultEntity>> batchDic = new Dictionary<string, List<CRD_CD_QueryResultEntity>>();
     
            StringBuilder error = new StringBuilder();


            foreach (var item in listdic)
            {
                List<CRD_CD_QueryResultEntity> list = new List<CRD_CD_QueryResultEntity>();
                foreach (var line in item.Value)
                {
                    CRD_CD_QueryResultEntity queryEnity = ConvertToEntity(item.Key, line, error);
                    if (queryEnity.FailReason !=null)
                        list.Add(queryEnity);
                }
                if (error.Length != 0)
                {
                    opeLog.AddFailNum(1);
                    Log4netAdapter.WriteInfo(error.ToString());

                }
                else
                {
                    batchDic.Add(item.Key, list);
                }

            }
            return batchDic;
        }
        CRD_CD_QueryResultEntity ConvertToEntity(string fileName, string fileLine, StringBuilder error)
        {
           var arr= fileName.Split('\\');
           fileName = arr[arr.Length - 1];
            CRD_CD_QueryResultEntity queryResult = new CRD_CD_QueryResultEntity();
            string[] fields = fileLine.Replace("|+|","|").Split('|');
     
            if (fields == null || fields.Length != 4)
            {
                error.AppendLine("fileName:" + fileName + ",fileLine：" + fileLine + "字段数目不是。");
                opeLog.AddfailReson(new FailReason(fileName, fileLine, "", "字段数目不是4"));
        
            }
            else
            {
                try
                {
                    queryResult.FileName = fileName;
                    queryResult.QueryNum = int.Parse(fields[0]);
                    queryResult.SuccessNum = int.Parse(fields[1]);
                    queryResult.FailNum = int.Parse(fields[2]);
                    queryResult.FailReason = fields[3];
                }
                catch (Exception ex)
                {
                    error.AppendLine("fileName:" + fileName + ",fileLine：" + fileLine + ex.Message + ex.StackTrace);
                    opeLog.AddfailReson(new FailReason(fileName, fileLine, "", "数字转换出现异常"));
     
                }
            }
            return queryResult;
        }


        public override bool SaveData()
        {
            string error = string.Empty;
            try
            {
                Dictionary<string,List<CRD_CD_QueryResultEntity>> queryList = ConvertToEntityList(ReadTxtFile());
                error = new CRD_CD_QueryResultBusiness(opeLog).InsertDicList(queryList);//保存查询结果

                List<CRD_CD_QueryResultEntity> list = new List<CRD_CD_QueryResultEntity>();
                foreach(var item in queryList)
                {
                    list.AddRange(item.Value);
                }
                UpdateQueryFailCreditInfo(list);//更新查询失败状态

                new  CRD_CD_OperatorLogBusiness().InsertEntity(opeLog.GetoperatorLogEntity());//插入日志
               
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("txt解析发生异常", ex);
            }
            if (error == string.Empty)
                return true;
            else
            {
                Log4netAdapter.WriteInfo(error.ToString());
                return false;
            }

        }
        void UpdateQueryFailCreditInfo(List<CRD_CD_QueryResultEntity> list)
        {
            string[] queryCreditFailArray = null;
            string[] queryItemArray = null;
            List<CRD_CD_CreditUserInfoEntity> creditList = new List<CRD_CD_CreditUserInfoEntity>();
            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item.FailReason.Trim()))
                {
                    queryCreditFailArray = item.FailReason.Split(',');
                    foreach (var queryitem in queryCreditFailArray)
                    {
                        queryItemArray = queryitem.Split('_');
                        if(queryItemArray.Length!=3)
                        {
                            Log4netAdapter.WriteInfo(item.FailReason+"查询结果信息错误，无法跟新数据状态");
                            continue;
                        }
                        else
                        {
                            creditList.Add(new CRD_CD_CreditUserInfoEntity()
                            {
                                Cert_No = queryItemArray[1],
                                Error_Reason=GetQueryFailReason(queryItemArray[2]),
                                State=(byte)RequestState.QueryFail

                            });
                        }
                    }
                }
            }
            if(creditList.Count!=0)
            {
                new CRD_CD_CreditUserInfoBusiness().UpdateListStateByCert_No(creditList);
            }
        }
        string GetQueryFailReason(string reason)
        {
            string error = string.Empty;
            switch (reason)
            {
                case "1":
                    error = "影像资料错误或者未检索到影像资料";
                    break;
                case "2":
                    error = "查询征信报告失败";
                    break;
                default:
                    error = "原因不明";
                    break;

            }
            return error;
        }
    }
}
