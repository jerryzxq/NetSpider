using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;

namespace Vcredit.ActivexLogin.Common
{
    public class Constants
    {
        public static readonly string REQEUST_QUEUE_PREFIX = ":BlockQueue";
        public static readonly string REQEUST_ENCRYPT_PREFIX = ":Encrypt";
        public static readonly string REQEUST_COOKIE_PREFIX = ":Cookies";

        private static readonly string showSitesStr = ConfigurationManager.AppSettings["ShowSites"];
        private static List<ProjectEnums.WebSiteType> _showSites;
        /// <summary>
        /// 获取配置要显示的站点
        /// </summary>
        /// <returns></returns>
        public static List<ProjectEnums.WebSiteType> GetShowSites()
        {
            if (_showSites == null)
            {
                _showSites = new List<ProjectEnums.WebSiteType>();
                foreach (var item in showSitesStr.Split(new char[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
                {
                    if (!item.IsEmpty())
                    {
                        try
                        {
                            var type = (ProjectEnums.WebSiteType)item.ToInt(0);
                            _showSites.Add(type);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException("ShowSites 配置错误，系统找不到配置的站点", ex);
                        }
                    }
                }
            }

            return _showSites;
        }

        /// <summary>
        /// 当前app是否置顶
        /// </summary>
        public static readonly bool DoTopMost = ConfigurationManager.AppSettings["DoTopMost"].ToBool(false);

        /// <summary>
        /// 是否弹窗模式
        /// </summary>
        public static readonly bool IsDialog = ConfigurationManager.AppSettings["IsDialog"].ToBool(false);

        public static readonly string WebAppUrl = ConfigurationManager.AppSettings["WebAppUrl"];
    }
}
