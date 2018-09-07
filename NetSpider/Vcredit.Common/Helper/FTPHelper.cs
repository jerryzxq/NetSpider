using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Vcredit.Common.Helper
{
    /// <summary>
    /// FTP帮助类
    /// </summary>
    public class FTPHelper
    {
        string _server;
        string _userName;
        string _password;
        bool _usePassive;

        string _ftpURI;

        public FTPHelper(string server, string userName, string password, bool usePassive = false)
        {
            _server = server;
            _userName = userName;
            _password = password;
            _usePassive = usePassive;

            if (!_server.Contains("ftp://"))
            {
                _ftpURI = string.Format("ftp://{0}", _server);
            }
            else
            {
                _ftpURI = _server;
            }
        }

        #region CreateFtpRequest
        FtpWebRequest CreateFtpRequest(string requestUriString)
        {
            return CreateFtpRequest(new Uri(requestUriString));
        }

        FtpWebRequest CreateFtpRequest(Uri requestUri)
        {
            FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create(requestUri);
            ftp.UseBinary = true;
            ftp.UsePassive = _usePassive;
            ftp.KeepAlive = false;
            ftp.Credentials = new NetworkCredential(_userName, _password);
            return ftp;
        }
        #endregion

        #region ListDirectoryDetails
        /// <summary>
        /// 获取FTP服务器上的文件的详细列表
        /// </summary>
        public IList<string> ListDirectoryDetails(string workingDirectory = null, string strMask = "*.*")
        {
            string requestUriString = string.IsNullOrEmpty(workingDirectory) ? _ftpURI : _ftpURI + "/" + workingDirectory;
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", requestUriString, strMask));
            ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            IList<string> result = new List<string>();
            using (WebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        result.Add(reader.ReadLine());
                    }
                    reader.Close();
                }
                response.Close();
            }
            return result;
        }
        #endregion

        #region ListDirectory
        /// <summary>
        /// 获取FTP服务器上的文件的简短列表
        /// </summary>
        public IList<string> ListDirectory(string workingDirectory = null, string strMask = null)
        {
            string requestUriString = string.IsNullOrEmpty(workingDirectory) ? _ftpURI : _ftpURI + "/" + workingDirectory;
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", requestUriString, strMask));
            ftp.Method = WebRequestMethods.Ftp.ListDirectory;
            IList<string> result = new List<string>();
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default))
                {
                    while (!reader.EndOfStream)
                    {
                        result.Add(reader.ReadLine());
                    }
                    reader.Close();
                }
                response.Close();
            }
            return result;
        }
        #endregion

        #region FileExists
        /// <summary>
        /// 判断文件是否存在于FTP服务器上
        /// </summary>
        public bool FileExists(string fileName, string workingDirectory = null)
        {
            foreach (string s in ListDirectory(workingDirectory, null))
            {
                if (s.ToLower().Equals(fileName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region GetDateTimestamp
        /// <summary>
        /// 获取FTP服务器上的文件的最后修改时间
        /// </summary>
        public DateTime GetDateTimestamp(string remoteFile)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            ftp.Method = WebRequestMethods.Ftp.GetDateTimestamp;

            DateTime result;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                result = response.LastModified;
                response.Close();
            }
            return result;
        }
        #endregion

        #region GetFileSize
        /// <summary>
        /// 获取FTP服务器上的文件的大小
        /// </summary>
        public long GetFileSize(string remoteFile)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            ftp.Method = WebRequestMethods.Ftp.GetFileSize;

            long result;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                result = response.ContentLength;
                response.Close();
            }
            return result;
        }
        #endregion

        #region DeleteFile
        public void DeleteFile(string remoteFile)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            ftp.Method = WebRequestMethods.Ftp.DeleteFile;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                response.Close();
            }
        }
        #endregion

        #region DownloadFile
        /// <summary>
        /// 从FTP服务器下载文件
        /// </summary>
        public void DownloadFile(string localFile, string remoteFile)
        {
            //FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            //ftp.Method = WebRequestMethods.Ftp.DownloadFile;
            //using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        byte[] buffer = new byte[2048 * 8];
            //        using (FileStream fs = new FileStream(localFile, FileMode.Create, FileAccess.Write))
            //        {
            //            int count = stream.Read(buffer, 0, buffer.Length);
            //            while (count > 0)
            //            {
            //                fs.Write(buffer, 0, count);
            //                count = stream.Read(buffer, 0, buffer.Length);
            //            }
            //            fs.Close();
            //        }
            //        stream.Close();
            //    }
            //    response.Close();
            //}
            byte[] result = null;
            try
            {
                FtpWebRequest reqFTP = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));//连接
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                //FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                //Stream ftpStream = response.GetResponseStream();
                //int bufferSize = 2048;
                //int readCount = 0;
                //int length = 0;
                //List<byte> tmpResult = new List<byte>();
                //byte[] buffer = new byte[bufferSize];
                //MemoryStream ms = new MemoryStream();
                //readCount = ftpStream.Read(buffer, 0, bufferSize);
                //tmpResult.AddRange(buffer);
                //length += readCount;
                //while (readCount > 0)
                //{
                //    ms.Write(buffer, 0, readCount);
                //    readCount = ftpStream.Read(buffer, 0, bufferSize);
                //    tmpResult.AddRange(buffer);
                //}
                //ms.Flush();
                //ms.Close();
                //ftpStream.Close();
                //response.Close();
                //reqFTP.Abort();
                //result = ms.ToArray();

                //using (FileStream fs = new FileStream(localFile, FileMode.Create, FileAccess.Write))
                //{
                //    fs.Write(buffer, 0, buffer.Length);
                //    fs.Close();
                //}

                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                FileStream outputStream = new FileStream(localFile, FileMode.Create);
                Stream ftpStream = response.GetResponseStream();

                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }

        }
        public byte[] DownloadFile(string remoteFile)
        {
            byte[] result = null;
            try
            {
                FtpWebRequest reqFTP = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));//连接
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                int bufferSize = 2048;
                int readCount = 0;
                int length = 0;
                List<byte> tmpResult = new List<byte>();
                byte[] buffer = new byte[bufferSize];
                MemoryStream ms = new MemoryStream();
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                tmpResult.AddRange(buffer);
                length += readCount;
                while (readCount > 0)
                {
                    ms.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    tmpResult.AddRange(buffer);
                }
                ms.Flush();
                ms.Close();
                ftpStream.Close();
                response.Close();
                reqFTP.Abort();
                result = ms.ToArray();
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }
            return result;
            //byte[] buffer = new byte[2048 * 8];
            //FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            //ftp.Method = WebRequestMethods.Ftp.DownloadFile;
            //using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        int count = stream.Read(buffer, 0, buffer.Length);
            //        while (count > 0)
            //        {
            //            count = stream.Read(buffer, 0, buffer.Length);
            //        }
            //    }
            //    response.Close();
            //}
            //return buffer;
        }
        public Stream DownloadFileToStream(string remoteFile)
        {
            Stream stream = null;
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            ftp.Method = WebRequestMethods.Ftp.DownloadFile;
            FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();
            stream = response.GetResponseStream();
            return stream;
        }
        #endregion

        #region UploadFile
        /// <summary>
        /// 将文件上载到FTP服务器
        /// </summary>
        public void UploadFile(string localFile, string remoteFile)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            ftp.Method = WebRequestMethods.Ftp.UploadFile;
            using (FileStream fs = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                ftp.ContentLength = fs.Length;
                using (Stream stream = ftp.GetRequestStream())
                {
                    byte[] buffer = new byte[2048 * 8];
                    int count = fs.Read(buffer, 0, buffer.Length);
                    while (count > 0)
                    {
                        stream.Write(buffer, 0, count);
                        count = fs.Read(buffer, 0, buffer.Length);
                    }
                    stream.Close();
                }
                fs.Close();
            }
        }

        public void UploadFile(byte[] bytes, string remoteFile)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, remoteFile));
            ftp.Method = WebRequestMethods.Ftp.UploadFile;
            ftp.ContentLength = bytes.Length;
            using (Stream stream = ftp.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }

        #endregion

        #region MakeDirectory
        public void MakeDirectory(string directoryName)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, directoryName));
            ftp.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                response.Close();
            }
        }
        #endregion

        #region RemoveDirectory
        public void RemoveDirectory(string directoryName)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, directoryName));
            ftp.Method = WebRequestMethods.Ftp.RemoveDirectory;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                response.Close();
            }
        }
        #endregion

        #region Rename
        /// <summary>
        /// 重命名FTP服务器上的文件或文件夹,也可用于移动文件或目录
        /// </summary>
        public void Rename(string oldName, string newName)
        {
            FtpWebRequest ftp = CreateFtpRequest(string.Format("{0}/{1}", _ftpURI, oldName));
            ftp.Method = WebRequestMethods.Ftp.Rename;
            ftp.RenameTo = newName;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                response.Close();
            }
        }
        #endregion
    }
}