/*********************************************  
 * * 功能描述： log应用工具类，调用log4net接口
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

namespace Vcredit.Common.Utility
{
    public class Log4netAdapter 
    {
        /// <summary>
        /// 静态只读实体对象info信息
        /// </summary>
        private static readonly log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");
        /// <summary>
        ///  静态只读实体对象error信息
        /// </summary>
        private static readonly log4net.ILog Logerror = log4net.LogManager.GetLogger("logerror");
        /// <summary>
        ///  添加info信息
        /// </summary>
        /// <param name="info"></param>
        public static void WriteInfo(string info)
        {
            if (Loginfo.IsInfoEnabled)
            {
                Loginfo.Info(info);
            }
        }
        /// <summary>
        /// 添加异常信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void WriteError(string info, Exception se)
        {
            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(info, se);
            }
        }
    }
}
