using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CommonLayer.Extension;

namespace Vcredit.ExternalCredit.CommonLayer
{

    /// <summary>
    /// ASE加解密
    /// </summary>
    public class AESHelper
    {
        //http://www.oschina.net/code/snippet_242957_9931 解决方法
        public static String getAESKey()
        {
            int count = 16; //随机数位数
            char start = '0';
            char end = '9';

            Random rnd = new Random();
            char[] result = new char[count];
            int len = end - start + 1;

            while (count-- > 0)
            {
                result[count] = (char)(rnd.Next(len) + start);
            }
            return new String(result);
        }
        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AesEncrypt(string str, string key)
        {


            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return RSAHelp.Bytes2String(resultArray);
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            string EnStr = Commonfunction.DecodeFromBase64(str);

            var toEncryptArray = RSAHelp.String2Bytes(EnStr);
            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

    }

}
