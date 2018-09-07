using System;
using System.IO;
using System.Web;
using Vcredit.Common.Utility;

namespace Vcredit.ExtTrade.CommonLayer
{
    /// <summary>
    /// 文件上传类
    /// </summary>
    public class FileUp
    {
        public FileUp()
        { }

        /// <summary>
        /// 转换为字节数组
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>字节数组</returns>
        public byte[] GetBinaryFile(string filename)
        {
            if (File.Exists(filename))
            {
                FileStream Fsm = null;
                try
                {
                    Fsm = File.OpenRead(filename);
                    return this.ConvertStreamToByteBuffer(Fsm);
                }
                catch
                {
                    return new byte[0];
                }
                finally
                {
                    Fsm.Close();
                }
            }
            else
            {
                return new byte[0];
            }
        }

        /// <summary>
        /// 流转化为字节数组
        /// </summary>
        /// <param name="theStream">流</param>
        /// <returns>字节数组</returns>
        public byte[] ConvertStreamToByteBuffer(System.IO.Stream theStream)
        {
            int bi;
            MemoryStream tempStream = new System.IO.MemoryStream();
            try
            {
                while ((bi = theStream.ReadByte()) != -1)
                {
                    tempStream.WriteByte(((byte)bi));
                }
                return tempStream.ToArray();
            }
            catch
            {
                return new byte[0];
            }
            finally
            {
                tempStream.Close();
            }
        }



        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="binData"></param>
        /// <param name="fileName">文件名（虚拟路径）</param>
        public void SaveFile(byte[] binData, string fileName)
        {
            FileStream fileStream = null;
            MemoryStream m = new MemoryStream(binData);
            try
            {
                //  fileName = HttpContext.Current.Server.MapPath(fileName);
                fileStream = new FileStream(fileName, FileMode.Create);
                m.WriteTo(fileStream);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(fileName + "读取时发生异常", ex);
                throw;
            }
            finally
            {
                m.Close();
                fileStream.Close();
            }
        }
      
    }
}