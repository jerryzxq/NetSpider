using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.CommonLayer;
using System.IO;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    class MyFileInfo
    {
        public MyFileInfo(bool IsExistTodayFile,FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            this.IsExistTodayFile = IsExistTodayFile;
        }
        public FileInfo fileInfo { set; get; }
        public bool  IsExistTodayFile{ set; get; }
    }
    public class TxtFileRequest
    {
      
        FileOperator fileOp = new FileOperator();

        List<CRD_CD_CreditUserInfoEntity> upLoadFailEntitys = new List<CRD_CD_CreditUserInfoEntity>();
        Dictionary<string, CRD_CD_CreditUserInfoEntity> creditDic = new Dictionary<string, CRD_CD_CreditUserInfoEntity>();
        CRD_CD_CreditUserInfoBusiness creditUserInfo = new CRD_CD_CreditUserInfoBusiness();
        List<MyFileInfo> fileInfolist = new List<MyFileInfo>();//存放搜寻到的文件信息的文件
        string directoryName;
        public TxtFileRequest()
        {
            directoryName =fileOp.GetDirectoryName(isLimitedByTime:true);
        }
        public void UpLoadRequestFile()
        {
            List<CRD_CD_CreditUserInfoEntity> cuinfoList = creditUserInfo.GetAllList();
            string txtLineStrs = string.Empty;
            if (cuinfoList.Count == 0)
            {
                Log4netAdapter.WriteInfo("没有查询到数据");
                return;
            }
            creditUserInfo.UpdateState(cuinfoList, (byte)RequestState.UpLoading);//正在上传......
            //实体转化为txt的文本
            txtLineStrs = ConvertToStr(cuinfoList);
            UpdateStateFailFile();//回滚错误的状态

            if (string.IsNullOrWhiteSpace(txtLineStrs))
            {
                StringBuilder sb = new StringBuilder();
                cuinfoList.ForEach(x => sb.Append(x.Cert_No + ","));
                sb.Append("信息上传失败");
                Log4netAdapter.WriteInfo(sb.ToString());
            }
            else
            {
                //创建txt文件
                FileOperateHelper.WriteFile(fileOp.GetTxtFileName(GetTxtBatchNo(), ConfigData.requestFileSavePath, directoryName), txtLineStrs);
                //上传文件;
                UploadFile(cuinfoList);
            }
        }
        #region 私有方法
        int GetTxtBatchNo()
        {
            string filePath = ConfigData.RequestedFileSavePath + directoryName;
            fileOp.CreateNotExistDirectory(filePath);
            DirectoryInfo di = new DirectoryInfo(filePath);
            var filearray = di.GetFiles();
            if(filearray!=null &&filearray.Length!=0)
            {
               var containNames= filearray.Where(item => item.Name.Contains(directoryName));
               if (containNames != null && containNames.Count() != 0)
                   return containNames.Count() + 1;
               else
                   return 1;
            }
            return 1;
        }
        string ConvertToStr(List<CRD_CD_CreditUserInfoEntity> cuinfoList)
        {
            StringBuilder fileContent = new StringBuilder();
            string zipFileName = string.Empty;
            string dateDirectory = ConfigData.requestFileSavePath + directoryName;
            fileOp.CreateNotExistDirectory(dateDirectory);//如果不存在目录就创建。
            var sucStae= (byte)RequestState.UpLoadSuccess;
            foreach (var item in cuinfoList)
            {
                zipFileName = item.NameFirstLetter + "_" + item.Cert_No + ".zip";
                if (item.LocalDirectoryName != directoryName)//如果是已经上传过但没有成功获取征信的客户重新上传的
                {
                    var tupeResultNomal = fileOp.CheckIsExistFile(ConfigData.requestFileSavePath + item.LocalDirectoryName, zipFileName);
                    if (tupeResultNomal.Item2)
                    {
                        fileInfolist.Add(new MyFileInfo(false ,new FileInfo(tupeResultNomal.Item1)));
                    }
                    else//迁移到今天的文件夹
                    {
                        if (!DealingzipFile(zipFileName, dateDirectory, item))
                            continue;
                    }
                }
                else
                {
                    var tupeResultNomal = fileOp.CheckIsExistFile(dateDirectory, zipFileName);
                    if (tupeResultNomal.Item2)
                    {
                        fileInfolist.Add(new MyFileInfo(true, new FileInfo(tupeResultNomal.Item1)));
                    }
                    else
                    {
                        if (!DealingzipFile(zipFileName, dateDirectory, item))
                            continue;
                    }
                }
             
                if (!creditDic.ContainsKey(zipFileName))
                {
                    fileContent.AppendLine(string.Format("{0}|+|{1}|+|{2}|+|{3}|+|{4}|+|{5}|+|{6}|+|{7}",
                 item.Query_Org, item.User_Code, item.Name, item.NameFirstLetter,
                 item.Cert_Type, item.Cert_No, item.Authorization_Date, item.ExpiryDate_Num.ToString()));

                    item.State = sucStae;//先把状态修改为上传成功
                    item.Error_Reason = null;
                    creditDic.Add(zipFileName, item);
                }
                 
            }
            return fileContent.ToString();
        }

        private bool DealingzipFile(string zipFileName, string dateDirectory, CRD_CD_CreditUserInfoEntity item)
        {
            var tupeResultUp = fileOp.CheckIsExistFile(ConfigData.RequestedFileSavePath + item.LocalDirectoryName, zipFileName);
            if (tupeResultUp.Item2)//如果Zip文件存在，复制到今天的文件夹
            {
                fileInfolist.Add(new MyFileInfo(false, new FileInfo(tupeResultUp.Item1)));
                return true;
            }
            else
            {
                AddCannotFindzipFile(zipFileName, item);
                return false;
            }
        }

        void AddCannotFindzipFile(string zipFileName, CRD_CD_CreditUserInfoEntity item)
        {
            item.State = (byte)RequestState.UpLoadFail;
            item.Error_Reason = "找不到解压文件" + zipFileName;
            upLoadFailEntitys.Add(item);
        }
        void UploadFile(List<CRD_CD_CreditUserInfoEntity> cuinfoList)
        {
            FTPHelper ftp = null;
            string upLoadDirectoryFullName = ConfigData.uploadFTPPath + directoryName + "\\";
            string requestedMovePath = ConfigData.RequestedFileSavePath + directoryName + "\\";//上传后文件移动的路径
            try
            {
                ftp = new FTPHelper(ConfigData.ftpHost, ConfigData.ftpUser, ConfigData.ftpPassword);
                if (!Directory.Exists(requestedMovePath))
                {
                    Directory.CreateDirectory(requestedMovePath);
                }
                IList<string> ftpDetials = ftp.ListDirectory(ConfigData.uploadFTPPath);
                DirectoryInfo di = new DirectoryInfo(fileOp.GetDirectoryName(ConfigData.requestFileSavePath, true));
                if (!ftpDetials.Contains(directoryName))//如何不存在文件夹
                {
                    ftp.MakeDirectory(upLoadDirectoryFullName);//创建文件夹
                }
                UploadAndRemovetxt(di.GetFiles("*.txt").FirstOrDefault(), ftp, upLoadDirectoryFullName, requestedMovePath);//先上传txt文件;
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("ftp出现异常", ex);
                creditUserInfo.UpdateState(cuinfoList, (byte)RequestState.UpLoadFail);//回滚
                return;
            }
       
            UploadFile(ftp, upLoadDirectoryFullName, requestedMovePath);

           
        }

        private void UploadFile(FTPHelper ftp, string upLoadDirectoryFullName, string requestedMovePath)
        {
            foreach (var item in fileInfolist)
            {
                if (!creditDic.ContainsKey(item.fileInfo.Name))
                    continue;
                try
                {
                    var remotefile = upLoadDirectoryFullName + item.fileInfo.Name;
                    ftp.UploadFile(item.fileInfo.FullName, remotefile);
                    var credit = creditDic[item.fileInfo.Name];
                    credit.UpLoadDirectoryName = directoryName;
                    if (!creditUserInfo.Update(credit))
                    {
                        ftp.DeleteFile(remotefile);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError(item.fileInfo.FullName + "上传文件出现异常", ex);
                    DealingUploadFail(item.fileInfo, ex.Message);

                }
                if(item.IsExistTodayFile)
                    FileOperateHelper.FileMove(item.fileInfo.FullName, requestedMovePath + item.fileInfo.Name);//上传成功移动指定的位置。
            }
        }

        void UploadAndRemovetxt(FileInfo txt, FTPHelper ftp, string remoteDic,string requestedMovePath)
        {
    
            if (txt == null)
                throw new Exception(txt.FullName + "txt文件获取失败");
            try
            {
                ftp.UploadFile(txt.FullName, remoteDic+txt.Name);
                FileOperateHelper.FileMove(txt.FullName, requestedMovePath + txt.Name);
            }
            catch (Exception ex )
            {
                throw new Exception(txt.FullName + "txt文件上传失败"+ex.Message);
            }
        }

        private void DealingUploadFail(FileInfo file,string error)
        {
           
            var credit = creditDic[file.Name];
            credit.Error_Reason = error;
            credit.State = (byte)RequestState.UpLoadFail;
            creditUserInfo.Update(credit);
        }
        void UpdateStateFailFile()
        {
            if (upLoadFailEntitys.Count != 0)
            {
                creditUserInfo.UpdateListStateByID(upLoadFailEntitys);
                upLoadFailEntitys.Clear();
            }
          
        }
        #endregion

    }
}
