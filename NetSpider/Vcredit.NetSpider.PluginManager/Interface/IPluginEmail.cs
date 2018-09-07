using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.PluginManager
{
    public interface IPluginEmail
    {
        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="tryApop"></param>
        /// <returns></returns>
        bool AuthenticateByPop3(string host, string userName, string password, int port = 995, bool ssl = true);
    }
}
