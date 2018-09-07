using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Vcredit.NetSpider.PluginManager
{
    public class PluginServiceManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dllName"></param>
        /// <returns></returns>
        public static IPluginHtmlParser LoadPlugin(string dllName)
        {
            Assembly asm = AppDomain.CurrentDomain.Load(dllName);
            foreach (Type t in asm.GetTypes())
            {
                foreach (Type iface in t.GetInterfaces())
                {
                    if (iface.Equals(typeof(IPluginHtmlParser)))
                    {
                        return (System.Activator.CreateInstance(t)) as IPluginHtmlParser;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 调用接口IPluginHtmlParser，解析html标签
        /// </summary>
        /// <returns></returns>
        public static IPluginHtmlParser GetHtmlParserPlugin()
        {
            return new Impl.PluginHtmlAgilityPack();
        }
        /// <summary>
        /// 调用接口IPluginJsonParser，解析json字符串
        /// </summary>
        /// <returns></returns>
        public static IPluginJsonParser GetJsonParserPlugin()
        {
            return new Impl.PluginNewtonSoftJson();
        }
        /// <summary>
        /// 调用接口IPluginSecurityCode，解析验证码
        /// </summary>
        /// <returns></returns>
        public static IPluginSecurityCode GetSecurityCodeParserPlugin()
        {
            return new Impl.PluginSecurityCode();
        }
        /// <summary>
        /// 调用接口IPluginEmail，登录email
        /// </summary>
        /// <returns></returns>
        public static IPluginEmail GetEmailPlugin()
        {
            return new Impl.PluginEmail();
        }
    }
}
