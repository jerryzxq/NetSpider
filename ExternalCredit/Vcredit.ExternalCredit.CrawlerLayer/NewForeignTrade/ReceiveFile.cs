using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public enum AuthorizationFileState
    {
        /// <summary>
        /// 默认状态
        /// </summary>
        Default = 0,

        /// <summary>
        /// 接收失败
        /// </summary>
        ReceiveFail = 1,

        /// <summary>
        /// 接收成功
        /// </summary>
        ReceiveSuccess = 2,

        /// <summary>
        /// 上传失败
        /// </summary>
        UploadFail = 3,

        /// <summary>
        /// 上传成功
        /// </summary>
        UpLoadSuccess= 4,


    }
    public class ReceiveFile
    {
        readonly  CRD_CD_CreditUserInfoBusiness creditBus = new CRD_CD_CreditUserInfoBusiness();
        readonly  FileUp fileUp = new FileUp();
        readonly  FileOperator fileOpe = new FileOperator();
        public string idType { get; set; }
        public string cifName { get; set; }
        public string idNo { get; set; }

        public string AuthorizationBase64Str { get; set; }
        public string CertBase64Str { get; set; }

        public void Receive(int creditUserInfo_Id,string pactNo)
        {

            string date= fileOpe.GetDirectoryName();
            string saveDirectory = ConfigData.requestFileSavePath;
            fileOpe.CreateNotExistDirectory(saveDirectory);
            try
            {
                fileUp.SaveFile(Convert.FromBase64String(AuthorizationBase64Str),
                saveDirectory + "\\SQ-" + creditUserInfo_Id.ToString() + "-" + pactNo + "-" + idNo+ ".JPG");
                fileUp.SaveFile(Convert.FromBase64String(CertBase64Str),
                saveDirectory + "\\ZJ-" + creditUserInfo_Id.ToString() + "-" + pactNo + "-" + idNo + ".JPG");
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("cert_no:"+idNo+"接收失败",ex);
                UpdateFileStae((int)AuthorizationFileState.ReceiveFail,creditUserInfo_Id);
                return;
            }

            UpdateFileStae((int)AuthorizationFileState.ReceiveSuccess,creditUserInfo_Id);
        }
       
        private  void UpdateFileStae(int fileState,int  creditUserInfo_id )
        {
            creditBus.UpdateFileState(fileState,creditUserInfo_id );
            
        }


        public void Receive()
        {

            string date = fileOpe.GetDirectoryName();
            string saveDirectory = ConfigData.requestFileSavePath;
            fileOpe.CreateNotExistDirectory(saveDirectory);
            try
            {
                cifName = System.Text.UTF8Encoding.UTF8.GetString(Convert.FromBase64String(cifName));
                fileUp.SaveFile(Convert.FromBase64String(AuthorizationBase64Str),
                saveDirectory + "\\SQ_" + idNo + "_" + date + ".JPG");
                fileUp.SaveFile(Convert.FromBase64String(CertBase64Str),
                saveDirectory + "\\ZJ_" + idNo + "_" + date + ".JPG");
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("cert_no:" + idNo + "接收失败", ex);
                UpdateFileStae((int)AuthorizationFileState.ReceiveFail);
                return;
            }

            UpdateFileStae((int)AuthorizationFileState.ReceiveSuccess);
        }
        private void UpdateFileStae(int fileState)
        {
            var creditentity = creditBus.GetByCertNo(idNo, cifName, idType).OrderByDescending(x => x.CreditUserInfo_Id).FirstOrDefault();
            if (creditentity != null)
            {
                creditentity.FileState = fileState;
                creditBus.Save(creditentity);
            }

        }

    }
}
