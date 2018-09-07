using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.Monitor
{
    public class Common
    {
        /// <summary>
        /// 获取采集网站名
        /// </summary>
        /// <param name="website">采集网站</param>
        /// <returns></returns>
        public static string GetWebsiteName(string website)
        {
            if (website.IsEmpty()) return "";
            string region = string.Empty;
            string[] mobileStr = website.Split('_');
            region = CommonFun.GetProvinceName(mobileStr[1]);
            switch (mobileStr[0])
            {
                case "ChinaUnicom": region += "联通"; break;
                case "ChinaMobile": region += "移动"; break;
                case "ChinaNet": region += "电信"; break;
                default: break;
            }
            return region;
        }
    }
}
