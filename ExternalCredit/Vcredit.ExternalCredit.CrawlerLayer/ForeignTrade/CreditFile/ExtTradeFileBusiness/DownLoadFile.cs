using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public class DownLoadFile
    {
        FileOperator ope = new FileOperator();
        public  void DownLoad()
        {   
            List<string> directoryNames = new List<string>();
            string[] directoryArray=  GetFTPDownLoadDateDirectory();//获取配置文件指定的下载日期文件夹
            if( directoryArray==null)//如果没有，获取默认今天的日期文件夹
            {
                directoryNames.Add(ope.GetDirectoryName());
            }
            else
            {
                directoryNames.AddRange(directoryArray);
            }
            //2.下载代码。
            try
            {
                foreach(var directoryitem in  directoryNames)
                {
                    DownLoadFileFromFTP(directoryitem);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("下载文件失败", ex);
            }

        }

        void DownLoadFileFromFTP(string directoryitem)
        {
            FTPHelper ftp = new FTPHelper(ConfigData.ftpHost, ConfigData.ftpUser, ConfigData.ftpPassword);
            IList<string> ftpDetialsList = ftp.ListDirectory(ConfigData.downLoadFTPPath);
            if (ftpDetialsList.Contains(directoryitem))//下载的目录如果不存在，就不下载
            {
                string downLoadDirectory = ConfigData.downLoadFTPPath + directoryitem;

                string localSaveDirectory = ConfigData.localFileSaveDir + directoryitem;
                IList<string> fileNameList = ftp.ListDirectory(downLoadDirectory + "/");
                ope.CreateNotExistDirectory(localSaveDirectory);
                Parallel.ForEach(fileNameList, item =>
                {
                    try
                    {
                        ftp.DownloadFile(localSaveDirectory + "\\" + item, downLoadDirectory + "\\" + item);
                    }
                    catch (Exception ex)
                    {
                        Log4netAdapter.WriteError("文件：" + localSaveDirectory + "\\" + item + "下载文件失败", ex);
                    }
                });
            }
            else
            {
                Log4netAdapter.WriteInfo("ftp没有文件夹" + directoryitem);
            }
        }
        string[] GetFTPDownLoadDateDirectory()
        {
            if(string.IsNullOrEmpty(ConfigData.FTPDownLoadDateDirectroy))
                return null;
            return ConfigData.FTPDownLoadDateDirectroy.Split(',');

        }
    }
}
