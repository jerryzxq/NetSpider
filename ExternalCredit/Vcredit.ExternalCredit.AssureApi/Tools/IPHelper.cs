using Microsoft.Owin;
using System;
using System.Text.RegularExpressions;
using Vcredit.Common.Utility;

namespace Vcredit.ExternalCredit.AssureApi.Tools
{
    /// <summary>
    /// IPAddress 的摘要说明
    /// </summary>
    public class IPHelper
    {
        IOwinContext _context = null;
        public IPHelper(IOwinContext context)
        {
            _context = context;
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IP
        {
            get
            {
                string userIP = string.Empty;
                try
                {
                    if (_context == null ||
                        _context.Request == null ||
                        _context.Request.Headers == null)
                        return userIP;

                    //CDN加速后取到的IP   
                    userIP = _context.Request.Headers["Cdn-Src-Ip"];
                    if (!string.IsNullOrEmpty(userIP))
                        return userIP;

                    userIP = _context.Request.Headers["HTTP_X_FORWARDED_FOR"];
                    if (!string.IsNullOrEmpty(userIP))
                        return userIP;

                    if (_context.Request.Headers["HTTP_VIA"] != null)
                    {
                        userIP = _context.Request.Headers["HTTP_X_FORWARDED_FOR"];
                        if (userIP == null)
                            userIP = _context.Request.Headers["REMOTE_ADDR"];
                    }
                    else
                        userIP = _context.Request.Headers["REMOTE_ADDR"];

                    if (string.IsNullOrEmpty(userIP) || string.Compare(userIP, "unknown", true) == 0)
                        userIP = _context.Request.RemoteIpAddress;

                    return userIP;
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError(string.Format("获取ip方法出现异常"), ex);
                }
                return userIP;
            }
        }
    }
}