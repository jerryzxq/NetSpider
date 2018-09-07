using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;

namespace Vcredit.ExternalCredit.CommonLayer
{
    public   class RSAHelp
    {
        /// <summary>
        /// 使用私钥加密字符串
        /// </summary>
        /// <param name="key">需加密的字符</param>
        /// <param name="keyPath">私钥证书文件地址</param>
        public static string EncryptKey(string str,string keyPath)
        {
           
            string cypher2 = RSAEncrypt(keyPath, str);  // 加密  
            return cypher2;
        }
        public static string GetPublickey(string keyPath)
        {
            System.Security.Cryptography.X509Certificates.X509Certificate2 c2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(keyPath);

            return c2.PublicKey.Key.ToXmlString(false);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="m_strDecryptString"></param>
        /// <returns></returns>
        //static string RSADecrypt(string xmlPrivateKey, string m_strDecryptString)
        //{
        //    RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        //    provider.FromXmlString(xmlPrivateKey);
        //    byte[] rgb = Convert.FromBase64String(m_strDecryptString);
        //    byte[] bytes = provider.Decrypt(rgb, false);
        //    return new UnicodeEncoding().GetString(bytes);
        //}
        /// <summary>   
        /// RSA加密
        /// </summary>   
        /// <param name="xmlPublicKey"></param>   
        /// <param name="m_strEncryptString"></param>   
        /// <returns></returns>   
        public static string RSAEncrypt(string xmlPublicKey, string m_strEncryptString)
        {
            if (string.IsNullOrEmpty(m_strEncryptString))
                return null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                byte[] cipherbytes;
                rsa.FromXmlString(xmlPublicKey);

                cipherbytes = rsa.Encrypt(System.Text.Encoding.UTF8.GetBytes(m_strEncryptString), false);
                return Bytes2String(cipherbytes);
            }
        }
      

  
        public  static string Bytes2String(byte[] data)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (var element in data)
            {
                builder.AppendFormat("{0:X2}", element);
            }
            return builder.ToString();
        }
        public static byte[] String2Bytes(string str)
        {
            const string pattern = @"[^0-9a-fA-F]+";
            str = System.Text.RegularExpressions.Regex.Replace(str, pattern, "");
            if (str == string.Empty)
                return null;
            byte[] data = new byte[str.Length / 2];

            for (int i = 0; i < data.Length; ++i)
                data[i] = byte.Parse(str.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber);
            return data;
        }
    }
}
