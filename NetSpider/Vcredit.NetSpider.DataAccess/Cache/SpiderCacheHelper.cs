using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Helper;

namespace Vcredit.NetSpider.DataAccess.Cache
{
    public class SpiderCacheHelper
    {
        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <param name="CacheKey">键</param>
        public static object GetCache(string CacheKey)
        {
            if (!String.IsNullOrEmpty(AppSettings.memcachedServer))
            {
                return MemcachedHelper.Get(CacheKey);
            }
            else
            {
                return CacheHelper.GetCache(CacheKey);
            }
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string CacheKey, object objObject)
        {
            if (!String.IsNullOrEmpty(AppSettings.memcachedServer))
            {
                if (AppSettings.cacheMinutes > 0)
                {
                    MemcachedHelper.Set(CacheKey, objObject, DateTime.Now.AddMinutes(AppSettings.cacheMinutes));
                }
                else
                {
                    MemcachedHelper.Set(CacheKey, objObject);
                }
            }
            else
            {
                if (AppSettings.cacheMinutes > 0)
                {
                    TimeSpan tspan = TimeSpan.FromMinutes(AppSettings.cacheMinutes);
                    CacheHelper.SetCache(CacheKey, objObject,tspan);
                }
                else
                {
                    CacheHelper.SetCache(CacheKey, objObject);
                }
            }
        }

        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        public static void RemoveCache(string CacheKey)
        {
            if (!String.IsNullOrEmpty(AppSettings.memcachedServer))
            {
                MemcachedHelper.Delete(CacheKey);
            }
            else
            {
                CacheHelper.RemoveCache(CacheKey);
            }
        }
    }
}
