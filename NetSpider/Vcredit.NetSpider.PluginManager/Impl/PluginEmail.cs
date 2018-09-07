using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumiSoft.Net.POP3.Client;

namespace Vcredit.NetSpider.PluginManager.Impl
{
    public class PluginEmail : IPluginEmail
    {
        public bool AuthenticateByPop3(string host, string userName, string password, int port, bool ssl)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return false;
            try
            {
                using (POP3_Client pop3 = new POP3_Client())
                {
                    //与Pop3服务器建立连接  
                    pop3.Connect(host, port, ssl);
                    pop3.Login(userName,password);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
