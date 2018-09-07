using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Vcredit.Common.Constants
{
    public class AppSettings
    {
        public static string localUrl = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["localUrl"]);
        public static int cacheMinutes = Vcredit.Common.Utility.Chk.IsNullInt(ConfigurationManager.AppSettings["cacheMinutes"]);
        public static string kkdWeChatService = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["kkdWeChatService"]);
        public static string xxqdWeChatService = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["xxqdWeChatService"]);
        public static string memcachedServer = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["memcachedServer"]);
        public static string collectionWebChatService = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["collectionWebChatService"]);
        public static string grayNumberService = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["grayNumberService"]);
    }
}
