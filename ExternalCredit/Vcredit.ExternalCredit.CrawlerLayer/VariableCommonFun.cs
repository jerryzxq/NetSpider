using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer
{
     class VariableCommonFun
    {
        public static  byte GetOverdue_Cyc(string payment_state)
        {
            if (string.IsNullOrEmpty(payment_state))
                return 0;
            return (byte)Regex.Matches(payment_state, "[1-7]").Count;

        }
        public static string GetRandomString(int length)
        {

            const string key = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            if (length < 1)
                return string.Empty;

            Random rnd = new Random();
            byte[] buffer = new byte[8];

            ulong bit = 31;
            ulong result = 0;
            int index = 0;
            StringBuilder sb = new StringBuilder((length / 5 + 1) * 5);

            while (sb.Length < length)
            {
                rnd.NextBytes(buffer);

                buffer[5] = buffer[6] = buffer[7] = 0x00;
                result = BitConverter.ToUInt64(buffer, 0);

                while (result > 0 && sb.Length < length)
                {
                    index = (int)(bit & result);
                    sb.Append(key[index]);
                    result = result >> 5;
                }
            }
            return sb.ToString();
        }
        public  static string ReadBase64Str(string fileFullName)
        {
            FileStream stream = new FileInfo(fileFullName).OpenRead();
            Byte[] buffer = new Byte[stream.Length];
            //从流中读取字节块并将该数据写入给定缓冲区buffer中
            stream.Read(buffer, 0, Convert.ToInt32(stream.Length));
            return System.Convert.ToBase64String(buffer);

        }
     
    }
}
