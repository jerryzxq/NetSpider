using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Vcredit.ExtTrade.CommonLayer
{
    public class SharpZip
    {
        public const int BUFFER_SIZE = 2048;

        #region CompressLevel

        // 0 - store only to 9 - means best compression
        public enum CompressLevel
        {
            Store = 0,
            Level1,
            Level2,
            Level3,
            Level4,
            Level5,
            Level6,
            Level7,
            Level8,
            Best
        }

        #endregion

        #region Unzip

        public static int Unzip(string destFolder, string srcZipFile, string password)
        {
            ZipInputStream zipStream = null;
            ZipEntry zipEntry = null;
            FileStream streamWriter = null;
            int count = 0;

            try
            {
                zipStream = new ZipInputStream(File.OpenRead(srcZipFile));
                zipStream.Password = password;

                while ((zipEntry = zipStream.GetNextEntry()) != null)
                {
                    string zipFileDirectory = Path.GetDirectoryName(zipEntry.Name);
                    string destFileDirectory = Path.Combine(destFolder, zipFileDirectory);
                    if (!Directory.Exists(destFileDirectory))
                    {
                        Directory.CreateDirectory(destFileDirectory);
                    }

                    string fileName = Path.GetFileName(zipEntry.Name);
                    if (fileName.Length > 0)
                    {
                        string destFilePath = Path.Combine(destFileDirectory, fileName);

                        streamWriter = File.Create(destFilePath);
                        int size = BUFFER_SIZE;
                        byte[] data = new byte[BUFFER_SIZE];
                        int extractCount = 0;
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                            extractCount += size;
                        }

                        streamWriter.Flush();
                        streamWriter.Close();
                        count++;
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                }

                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }

            return count;
        }

        #endregion

        #region Zip folder

        public static int Zip(string destFolder, string srcFolder, string password, CompressLevel level)
        {
            ZipOutputStream zipStream = null;
            FileStream streamWriter = null;
            int count = 0;

            try
            {
                //Use Crc32
                Crc32 crc32 = new Crc32();

                //Create Zip File
                zipStream = new ZipOutputStream(File.Create(destFolder));

                //Specify Level
                zipStream.SetLevel(Convert.ToInt32(level));

                //Specify Password
                if (password != null && password.Trim().Length > 0)
                {
                    zipStream.Password = password;
                }

                count = PutDirectoryToZipStream(srcFolder, null, zipStream, crc32, streamWriter);
            }
            catch
            {
                throw;
            }
            finally
            {
                //Clear Resource
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
            }

            return count;
        }

        #endregion

        #region Zip multi files

        public static int Zip(string destFolder, string[] srcFiles, string folderName, string password, CompressLevel level)
        {
            ZipOutputStream zipStream = null;
            FileStream streamWriter = null;
            int count = 0;

            try
            {
                //Use Crc32
                Crc32 crc32 = new Crc32();

                //Create Zip File
                zipStream = new ZipOutputStream(File.Create(destFolder));

                //Specify Level
                zipStream.SetLevel(Convert.ToInt32(level));

                //Specify Password
                if (password != null && password.Trim().Length > 0)
                {
                    zipStream.Password = password;
                }

                //Foreach File
                foreach (string file in srcFiles)
                {
                    //Check Whether the file exists
                    if (!File.Exists(file))
                    {
                        throw new FileNotFoundException(file);
                    }

                    //Read the file to stream
                    streamWriter = File.OpenRead(file);
                    byte[] buffer = new byte[streamWriter.Length];
                    streamWriter.Read(buffer, 0, buffer.Length);
                    streamWriter.Close();

                    //Specify ZipEntry
                    crc32.Reset();
                    crc32.Update(buffer);
                    ZipEntry zipEntry = new ZipEntry(Path.Combine(folderName, Path.GetFileName(file)));
                    zipEntry.DateTime = DateTime.Now;
                    zipEntry.Size = buffer.Length;
                    zipEntry.Crc = crc32.Value;

                    //Put file info into zip stream
                    zipStream.PutNextEntry(zipEntry);

                    //Put file data into zip stream
                    zipStream.Write(buffer, 0, buffer.Length);

                    count++;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                //Clear Resource
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
            }

            return count;
        }

        #endregion

        #region Common Function

        /// <summary>
        /// Example : Make "C:TestZipFile" To "ZipFile"
        /// </summary>
        /// <param name="directory">Directory</param>
        /// <returns></returns>
        private static string GetDirectoryName(string directory)
        {
            directory = directory.Replace("/", @"").Replace(@"\", @"");
            if (directory[directory.Length - 1].ToString() == @"")
            {
                directory = directory.Substring(0, directory.Length - 1);
            }

            int lastRoot = directory.LastIndexOfAny(@"".ToCharArray());

            return directory.Substring(lastRoot + 1, directory.Length - lastRoot - 1);
        }

        private static int PutDirectoryToZipStream(string directory, string logicBaseDir,
                                                   ZipOutputStream zipStream, Crc32 crc32, FileStream streamWriter)
        {
            int count = 0;

            //Get the logic directory
            string logicDir = null;
            if (logicBaseDir == null || logicBaseDir.Length == 0)
            {
                logicDir = GetDirectoryName(directory);
            }
            else
            {
                logicDir = logicBaseDir + @"" + GetDirectoryName(directory);
            }
            logicDir = logicDir.Replace("/", @"").Replace(@"\", @"");

            //Get Directories Name
            string[] dirs = Directory.GetDirectories(directory);

            //Get Files Name
            string[] files = Directory.GetFiles(directory);

            //Foreach Directories
            foreach (string dir in dirs)
            {
                count = count + PutDirectoryToZipStream(dir, logicDir, zipStream, crc32, streamWriter);
            }

            //Foreach Files
            foreach (string file in files)
            {
                //Read the file to stream
                streamWriter = File.OpenRead(file);
                byte[] buffer = new byte[streamWriter.Length];
                streamWriter.Read(buffer, 0, buffer.Length);
                streamWriter.Close();

                //Specify ZipEntry
                crc32.Reset();
                crc32.Update(buffer);
                ZipEntry zipEntry = new ZipEntry(Path.Combine(logicDir, Path.GetFileName(file)));
                zipEntry.DateTime = DateTime.Now;
                zipEntry.Size = buffer.Length;
                zipEntry.Crc = crc32.Value;

                //Put file info into zip stream
                zipStream.PutNextEntry(zipEntry);

                //Put file data into zip stream
                zipStream.Write(buffer, 0, buffer.Length);

                count++;
            }

            return count;
        }

        #endregion

        public static string Decompress(string zipText)
        {
            byte[] gzipBuff = Convert.FromBase64String(zipText);
            return Encoding.UTF8.GetString(DecompressBytes(gzipBuff));
           
        }

        public static byte[] DecompressBytes(byte[] compressed)
        {
            byte[] buffer = new byte[4096];
            using (MemoryStream ms = new MemoryStream(compressed))
            using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
            using (MemoryStream uncompressed = new MemoryStream())
            {
                for (int r = -1; r != 0; r = gzs.Read(buffer, 0, buffer.Length))
                    if (r > 0) uncompressed.Write(buffer, 0, r);
                return uncompressed.ToArray();
            }
        }
    }
}
