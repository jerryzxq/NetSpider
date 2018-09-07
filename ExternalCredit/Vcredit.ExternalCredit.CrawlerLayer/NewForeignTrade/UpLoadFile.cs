
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExternalCredit.Services;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.Services.Requests;
using Vcredit.ExternalCredit.Services.Responses;
namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    class UploadFile
    {
        public FileInfo _FileInfo { get; set; }
        public string  _PactNo { get; set; }
    }
    public  class UpLoadFile
    {
        IComplianAdditionalService comAddService = new ComplianAdditionalServiceImpl();
        CRD_CD_CreditUserInfoBusiness creditUserInfo = new CRD_CD_CreditUserInfoBusiness();
        FileOperator fileOp = new FileOperator();
        FileUp fileUp = new FileUp();
        IList<string> ftpDirDetials = new List<string>();
        readonly string dataDirectory = DateTime.Now.ToString("yyyyMMdd");
          FTPHelper ftp = null;
        public UpLoadFile()
        {
            initialize();
        }
        private void initialize()
        {
            try
            {
                ftp = new FTPHelper(ConfigData.ftpHost, ConfigData.ftpUser, ConfigData.ftpPassword);//初始化上传到外贸的ftp服务器
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("ftp出现异常", ex);
                return;
            }
            
        }
       
        public void UpLoad()
        {
            GetApplyFile();//获取授权文件
            var fileInofs = new DirectoryInfo(ConfigData.requestFileSavePath).GetFiles();
            if(fileInofs.Length ==0)
            {
                Log4netAdapter.WriteInfo("没有要上传的文件");
                return;
            }
            UploadFile(fileInofs);
        }
        #region 私有方法
        private void SaveFiles(UserFileRespne req, CRD_CD_CreditUserInfoEntity credit)
        {
            var content = JsonConvert.DeserializeObject<UserFilesContent>(req.Content);
         
            string saveDirectory = ConfigData.requestFileSavePath;
            fileOp.CreateNotExistDirectory(saveDirectory);
            fileUp.SaveFile(Convert.FromBase64String(content.InfoPowerSignatured),
            saveDirectory + "\\SQ-" + credit.CreditUserInfo_Id.ToString() + "-"+credit.PactNo+"-" + credit.Cert_No + "." + content.InfoPowerSignaturedFileType);
            fileUp.SaveFile(Convert.FromBase64String(content.IdCardImg),
            saveDirectory + "\\ZJ-" + credit.CreditUserInfo_Id.ToString() + "-"+credit.PactNo+"-" + credit.Cert_No + ".JPG");
        }
        private void GetApplyFile()
        {
   
            var creditList = creditUserInfo.GetNotUploadFileInfo();
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
                creditUserInfo.UpdateFileState(item.FileState??0,(int)item.CreditUserInfo_Id);

            }
        }
    
        private void UploadFile(FileInfo[] fileInfos)
        {
            List<UploadFile> haveUploadFiles = new List<UploadFile>();
            Dictionary<int ,bool> haveUploadCert_Nos = new  Dictionary<int ,bool>();//存储已经上传的且已经更新状态的身份证号，防止重复更新
            int creditUserInfo_id = 0;
            foreach (var item in fileInfos)
            {
          
                try
                {
                    var array = item.Name.Split('-');
                     creditUserInfo_id = int.Parse(array[1]);
                    string pactno = array[2];
                    if(string.IsNullOrEmpty(pactno))
                    {
                        Log4netAdapter.WriteInfo(array[1]+"pactno 为空");
                        continue;
                    }

                    UpFtpFile(item);
                    haveUploadFiles.Add(new UploadFile() { _FileInfo = item, _PactNo = pactno });
                    ChangeState(haveUploadCert_Nos, creditUserInfo_id, AuthorizationFileState.UpLoadSuccess);
                
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError(item.Name + "上传文件出现异常", ex);
                    ChangeState(haveUploadCert_Nos, creditUserInfo_id, AuthorizationFileState.UploadFail);
                }
            }
            if (haveUploadFiles.Count != 0)//上传xml描述文件
                UploadXMlFile( haveUploadFiles);
        }

        private  void ChangeState( Dictionary<int, bool> haveUploadCert_Nos,  int creditUserInfo_Id,AuthorizationFileState state)
        {
            bool iscontian = false;
            if (haveUploadCert_Nos.ContainsKey(creditUserInfo_Id))
            {
                iscontian = true;
                if (!haveUploadCert_Nos[creditUserInfo_Id])
                    return;
                else if (state == AuthorizationFileState.UpLoadSuccess)
                    return;
                
            }
  
            if (creditUserInfo.UpdateFileState((int)state,creditUserInfo_Id))
            {
                if (iscontian)
                {
                    haveUploadCert_Nos[creditUserInfo_Id] = (state == AuthorizationFileState.UpLoadSuccess);
                }
                else
                {
                    haveUploadCert_Nos.Add(creditUserInfo_Id, state == AuthorizationFileState.UpLoadSuccess);
                }
            }
            else
            {
                Log4netAdapter.WriteInfo(creditUserInfo_Id + "上传ftp成功，但是更新状态失败");
            }
        }
        private void UploadXMlFile(List<UploadFile> haveUploadFiles)
        {
            string requestedMovePath = ConfigData.RequestedFileSavePath + "\\" + dataDirectory + "\\";

            fileOp.CreateNotExistDirectory(requestedMovePath);
            string sequenceid = DateTime.Now.ToString("yyyyMMdd-") + CommonFun.GetTimeStamp();
            UpLoadXmlFile Root = new UpLoadXmlFile();
            Root.Task = new Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.Work();
            Root.Task.SequenceId = sequenceid;
            Root.Task.BranchCode = "000100060001";
            Root.Task.Source = ConfigData.Source;
            Root.Task.OtherParam = string.Empty;
            List<Document> documents = new List<Document>();
            foreach (var item in haveUploadFiles)
            {
                if (item._FileInfo.Name.StartsWith("SQ"))
                    AddDoc(documents, item, "GRCZSQS");
                else
                    AddDoc(documents, item, "SFZ");
            }
            Root.Task.Documents = documents;
            TextWriter writer = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(Root.GetType());
                writer = new StreamWriter(requestedMovePath + sequenceid + ".xml");
                serializer.Serialize(writer, Root);

            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("", ex);
                return;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
            ftp.UploadFile(requestedMovePath + sequenceid + ".xml", ConfigData.uploadFTPPath + sequenceid + ".xml");

        }

        private static void AddDoc(List<Document> documents, CrawlerLayer.NewForeignTrade.UploadFile item,string subtype)
        {
            documents.Add(new Document()
            {

                Busstype =ConfigData.BussType,
                Subtype = subtype,
                FilesCount = 1,
                BussNo = item._PactNo??string.Empty,
                Files = new List<Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.File>() { 
                   new  Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.File(){FileOrder=1,Name=item._FileInfo.Name}
                   }
            });
        }
        private  void UpFtpFile(FileInfo item)
        {
            var remotefile = ConfigData.uploadFTPPath + item.Name;
           
            ftp.UploadFile(item.FullName, remotefile);
      
            FileOperateHelper.Delete(item.FullName);
        }
        #endregion
    }
}
