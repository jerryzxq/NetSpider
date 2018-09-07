///*********************************************  
// * * 功能描述： 有用的公共函数
// * * 创 建 人:  张志博
// * * 日    期:  2014/9/19
// * * 修 改 人:  
// * * 修改日期: 
// * * 修改描述:  
// * * 版    本:  1.0
// * *******************************************/
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Net;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Web;

//namespace Vcredit.ExtTrade.CommonLayer
//{
//    public class CommonFun
//    {
//        /// <summary>
//        /// 获取访问服务器的客户端IP
//        /// </summary>
//        /// <returns></returns>
//        public static string GetClientIP()
//        {
//           string ip=  HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
//           if (ip == null)
//               return string.Empty;
//           else
//               return ip;
//        }
//        /// <summary>
//        /// base64编码的字符串转化成转换之前的字符串
//        /// </summary>
//        /// <param name="base64str"></param>
//        /// <returns></returns>
//        public static  string ConvertFromBase64(string base64str)
//        {
//            byte[] c = Convert.FromBase64String(base64str);
//            return System.Text.Encoding.UTF8.GetString(c);
//        }


//    }
//}
