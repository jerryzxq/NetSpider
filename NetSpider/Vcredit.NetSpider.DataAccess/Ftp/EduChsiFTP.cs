using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.DataAccess.Ftp
{
    public class EduChsiFTP
    {
        #region
        FTPHelper ftpClient = null;
        string ftpHost = string.Empty;
        string ftpUser = string.Empty;
        string ftpPassword = string.Empty;
        string reportLocalPath = string.Empty;
        string ftpDirectory = string.Empty;
        string FileDir = string.Empty;
        #endregion
        public EduChsiFTP()
        {
            ftpHost = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpAddress"));
            ftpUser = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpUserName"));
            ftpPassword = Chk.IsNull(ConfigurationHelper.GetAppSetting("FtpPassword"));
            FileDir = Chk.IsNull(ConfigurationHelper.GetAppSetting("FileDir"));
            //ftpHost += "/Edu";
        }

        public void UploadChsiPhoto(byte[] photo, string filename)
        {
            ftpDirectory = "ChsiPhoto/";
            ftpClient = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
            ftpClient.UploadFile(photo, filename);
        }
        public void DownloadChsiPhoto(string filename)
        {
            ftpDirectory = "ChsiPhoto/";
            ftpClient = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
            ftpClient.DownloadFile(FileDir + ftpDirectory + filename, filename);
        }
        public byte[] DownloadChsiPhotoToByte(string filename)
        {
            ftpDirectory = "ChsiPhoto/";
            ftpClient = new FTPHelper(ftpHost + ftpDirectory, ftpUser, ftpPassword);
            var l = ftpClient.ListDirectoryDetails();
            Log4netAdapter.WriteInfo(l.Count.ToString());
            return ftpClient.DownloadFile(filename);
        }
    }
}
