using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExternalCredit.Services;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace GetAuthFIles
{
    class Program
    {
        readonly static FileUp fileUp = new FileUp();
        readonly static CRD_CD_CreditUserInfoBusiness creditUserInfo = new CRD_CD_CreditUserInfoBusiness();
        readonly static IComplianAdditionalService comAddService = new CompainGetAuthFiles();
        private static void SaveFiles(UserFileRespne req, CRD_CD_CreditUserInfoEntity credit)
        {
            var content = JsonConvert.DeserializeObject<UserFilesContent>(req.Content);

            string saveDirectory = ConfigData.requestFileSavePath;
            FileOperateHelper.FolderCreate(saveDirectory);
            fileUp.SaveFile(Convert.FromBase64String(content.InfoPowerSignatured),
            saveDirectory + "\\SQ_" + credit.CreditUserInfo_Id.ToString() + "_" + credit.Cert_No + "." + content.InfoPowerSignaturedFileType);
            fileUp.SaveFile(Convert.FromBase64String(content.IdCardImg),
            saveDirectory + "\\ZJ_" + credit.CreditUserInfo_Id.ToString() + "_" + credit.Cert_No + ".jpg");
        }
        private static  void GetApplyFile()
        {
            var creditList = creditUserInfo.GetNotUploadFileInfo((int)SysEnums.SourceType.ShangHai);
            foreach (var item in creditList)
            {
                try
                {
                    if (item.ApplyID == null || item.ApplyID == 0)
                    {
                        Log4netAdapter.WriteInfo(item.Cert_No + "ApplyID值为null");
                        continue;
                    }
                    var resp = comAddService.GetAuthenticationFile(new UserFilesRequest
                    {
                        IdentityNo = item.Cert_No,
                        Name = item.Name,
                        ApplyID = item.ApplyID.Value

                    });
                    if (resp != null && resp.IsSucceed)
                    {
                        SaveFiles(resp, item);
                        item.FileState = (int)AuthorizationFileState.ReceiveSuccess;
                    }
                    else
                    {
                        item.FileState = (int)AuthorizationFileState.ReceiveFail;
                        if (resp != null)
                            Log4netAdapter.WriteInfo(item.Cert_No + "获取失败：" + resp.Message);
                    }
                }
                catch (Exception ex)
                {
                    item.FileState = (int)AuthorizationFileState.ReceiveFail;
                    Log4netAdapter.WriteError(item.Cert_No + "出现异常", ex);
                }
                creditUserInfo.UpdateFileState(item);

            }
        }
        static void Main(string[] args)
        {
            try
            {
                Log4netAdapter.WriteInfo("开始获取授权文件");
                GetApplyFile();
                Log4netAdapter.WriteInfo("获取授权文件结束");
            }
            catch (Exception ex )
            {
               Log4netAdapter.WriteError( "上海小贷获取授权文件出现问题", ex); 
            }
        }
    }
}
