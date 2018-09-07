/*********************************************  
 * * 功能描述： Configuration处理工具类
 * * 创 建 人:  张志博
 * * 日    期:  2014/9/19
 * * 修 改 人:  
 * * 修改日期: 
 * * 修改描述:  
 * * 版    本:  1.0
 * *******************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Vcredit.Common.Helper
{
    public static class ConfigurationHelper
    {
        public static void UpdateSetting(string SettingKey, string SettingValue)
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //修改
            cfa.AppSettings.Settings[SettingKey].Value = SettingValue;
            cfa.Save();
            ConfigurationManager.RefreshSection("appSettings");// 刷新命名节
            return;
        }
        /// <summary>
        /// 获取AppSettings
        /// </summary>
        /// <param name="SettingKey"></param>
        /// <returns></returns>
        public static string GetAppSetting(string SettingKey)
        {
            string Value = null;
           
            //Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //if (cfa.AppSettings.Settings.Count > 0)
            //{
            //    var query = cfa.AppSettings.Settings[SettingKey];
            //    if (query != null)
            //    {
            //        Value = cfa.AppSettings.Settings[SettingKey].Value;
            //    }
            //}

            var Configs = ConfigurationManager.AppSettings;
            if (Configs.Count > 0)
            {
                var query = Configs[SettingKey];
                if (query != null)
                {
                    Value = Configs[SettingKey];
                }
            }
            return Value;
        }
        /// <summary>
        /// 获取ConnectionStrings
        /// </summary>
        /// <param name="SettingKey"></param>
        /// <returns></returns>
        public static string GetConnectionString(string ConnectionName)
        {
            string Value = null;
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (cfa.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                var query = cfa.ConnectionStrings.ConnectionStrings[ConnectionName];
                if (query != null)
                {
                    Value = cfa.ConnectionStrings.ConnectionStrings[ConnectionName].ConnectionString;
                }
            }
            return Value;
        }
    }
}
