using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using Vcredit.Common.Ext;
namespace Vcredit.Common.Utility
{
    public class RSAUtil
    {
        /// <summary>
        /// 创建RSAky
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string CreateRSAString(string key)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters keys = rsa.ExportParameters(true);
            String pkxml = "<root>\n<Modulus>"+ key + "</Modulus>";
            pkxml += "\n<Exponent>10001</Exponent>\n</root>";
            return pkxml;

        }

        private static RSACryptoServiceProvider CreateRSAEncryptProviderByRSAString(string rsas)
        {
            string srakey = CreateRSAString(rsas);
            RSAParameters parameters1;
            parameters1 = new RSAParameters();
            XmlDocument document1 = new XmlDocument();
            document1.LoadXml(srakey);
            XmlElement element1 = (XmlElement)document1.SelectSingleNode("root");
            parameters1.Modulus = ReadChild(element1, "Modulus");
            parameters1.Exponent = ReadChild(element1, "Exponent");
            CspParameters parameters2 = new CspParameters();
            parameters2.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider(parameters2);
            provider1.ImportParameters(parameters1);
            return provider1;

        }
        
        private static RSACryptoServiceProvider CreateRSADecryptProvider(String privateKey)
        {
            RSAParameters parameters1;
            parameters1 = new RSAParameters();
            //StreamReader reader1 = new StreamReader(privateKeyFile);
            XmlDocument document1 = new XmlDocument();
            document1.LoadXml(privateKey);
            XmlElement element1 = (XmlElement)document1.SelectSingleNode("root");
            parameters1.Modulus = ReadChild(element1, "Modulus");
            parameters1.Exponent = ReadChild(element1, "Exponent");
            parameters1.D = ReadChild(element1, "D");
            parameters1.DP = ReadChild(element1, "DP");
            parameters1.DQ = ReadChild(element1, "DQ");
            parameters1.P = ReadChild(element1, "P");
            parameters1.Q = ReadChild(element1, "Q");
            parameters1.InverseQ = ReadChild(element1, "InverseQ");
            CspParameters parameters2 = new CspParameters();
            parameters2.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider(parameters2);
            provider1.ImportParameters(parameters1);
            return provider1;
        }
        private static RSACryptoServiceProvider CreateRSAEncryptProvider(String publicKey)
        {
            RSAParameters parameters1;
            parameters1 = new RSAParameters();
            //StreamReader reader1 = new StreamReader(publicKeyFile);
            XmlDocument document1 = new XmlDocument();
            document1.LoadXml(publicKey);
            XmlElement element1 = (XmlElement)document1.SelectSingleNode("root");
            parameters1.Modulus = ReadChild(element1, "Modulus");
            parameters1.Exponent = ReadChild(element1, "Exponent");
            CspParameters parameters2 = new CspParameters();
            parameters2.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider(parameters2);
            provider1.ImportParameters(parameters1);
            return provider1;
        }

        private static byte[] ReadChild(XmlElement parent, string name)
        {
            XmlElement element1 = (XmlElement)parent.SelectSingleNode(name);
            return hexToBytes(element1.InnerText);
        }

        private static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        private static byte[] hexToBytes(String src)
        {
            int l = src.Length / 2;
            String str;
            byte[] ret = new byte[l];

            for (int i = 0; i < l; i++)
            {
                str = src.Substring(i * 2, 2);
                ret[i] = Convert.ToByte(str, 16);
            }
            return ret;
        }

        private static void SaveToFile(String filename, String data)
        {
            System.IO.StreamWriter sw = System.IO.File.CreateText(filename);
            sw.WriteLine(data);
            sw.Close();
        }

        #region 公开方法
        public static void CreateRSAKey()
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            RSAParameters keys = rsa.ExportParameters(true);
            String pkxml = "<root>\n<Modulus>" + ToHexString(keys.Modulus) + "</Modulus>";
            pkxml += "\n<Exponent>" + ToHexString(keys.Exponent) + "</Exponent>\n</root>";
            String psxml = "<root>\n<Modulus>" + ToHexString(keys.Modulus) + "</Modulus>";
            psxml += "\n<Exponent>" + ToHexString(keys.Exponent) + "</Exponent>";
            psxml += "\n<D>" + ToHexString(keys.D) + "</D>";
            psxml += "\n<DP>" + ToHexString(keys.DP) + "</DP>";
            psxml += "\n<P>" + ToHexString(keys.P) + "</P>";
            psxml += "\n<Q>" + ToHexString(keys.Q) + "</Q>";
            psxml += "\n<DQ>" + ToHexString(keys.DQ) + "</DQ>";
            psxml += "\n<InverseQ>" + ToHexString(keys.InverseQ) + "</InverseQ>\n</root>";

            SaveToFile("publickey.xml", pkxml);
            SaveToFile("privatekey.xml", psxml);

            string str = "";
            str = rsa.ToXmlString(false);
            TextWriter writer = new StreamWriter("public_1.xml");
            writer.Write(str);
            writer.Close();
        }
        public static string EncryptByFile(string str, string publicKeyFile)
        {
            StreamReader reader = new StreamReader(publicKeyFile);

            RSACryptoServiceProvider rsaencrype = CreateRSAEncryptProvider(reader.ReadToEnd());

            String text = str;

            byte[] data = new UnicodeEncoding().GetBytes(text);

            byte[] endata = rsaencrype.Encrypt(data, true);

            return ToHexString(endata);
        }

        public static string DecryptByFile(string hexstr, string privateKeyFile)
        {
            StreamReader reader = new StreamReader(privateKeyFile);
            RSACryptoServiceProvider rsadeencrypt = CreateRSADecryptProvider(reader.ReadToEnd());

            byte[] miwen = hexToBytes(hexstr);

            byte[] dedata = rsadeencrypt.Decrypt(miwen, true);

            return System.Text.UnicodeEncoding.Unicode.GetString(dedata);
        }
        public static string EncryptByXml(string str, string publicKey)
        {
            RSACryptoServiceProvider rsaencrype = CreateRSAEncryptProvider(publicKey);

            String text = str;

            byte[] data = new UnicodeEncoding().GetBytes(text);

            byte[] endata = rsaencrype.Encrypt(data, true);

            return ToHexString(endata);
        }

        public static string DecryptByXml(string hexstr, string privateKey)
        {
            RSACryptoServiceProvider rsadeencrypt = CreateRSADecryptProvider(privateKey);

            byte[] miwen = hexToBytes(hexstr);

            byte[] dedata = rsadeencrypt.Decrypt(miwen, true);

            return System.Text.UnicodeEncoding.Unicode.GetString(dedata);
        }
        public static string EncryptByKey(string password, string key)
        {
            RSACryptoServiceProvider rsaencrype = CreateRSAEncryptProviderByRSAString(key);


            byte[] data = new UnicodeEncoding().GetBytes(password);

            byte[] endata = rsaencrype.Encrypt(data, true);

            return ToHexString(endata);
        }
        #endregion
    }
}
