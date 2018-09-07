using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Vcredit.ExternalCredit.CommonLayer
{
    public class CachHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMinuts"></param>
        public static void Insert(string key, object value, double expireMinuts = 10)
        {
            HttpRuntime
                .Cache
                .Insert(key,
                        value,
                        null,   //new CacheDependency(UniversalFilePathResolver.ResolvePath("~/bin/Vcredit.ExternalCredit.CrawlerLayer.dll")), 
                        Cache.NoAbsoluteExpiration,
                        TimeSpan.FromMinutes(expireMinuts)   // 缓存有效期时间
                        );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : class
        {
            var obj = HttpRuntime.Cache.Get(key);
            if (obj == null)
                return null;

            return obj as T;
        }
    }
}
