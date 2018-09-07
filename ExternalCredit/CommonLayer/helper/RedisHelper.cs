using System;
using Vcredit.NetSpider.Cache;

namespace Vcredit.ExternalCredit.CommonLayer.helper
{
    public class RedisHelper
    {
        private const string _rootPrefix = "OrgCredit:";

        public static void SetCache(string key, object value, string prefix, int minute = 20)
        {
            Set(key, value, prefix, minute);
        }

        private static void Set(string key, object value, string prefix, int minute = 20)
        {
            if (!string.IsNullOrEmpty(prefix)) { prefix += ":"; }
            if (minute <= -1)
                CacheClient.Set(_rootPrefix + prefix + key, value);
            else
                CacheClient.Set(_rootPrefix + prefix + key, value, DateTime.Now.AddMinutes(minute));
        }

        public static T GetCache<T>(string key, string prefix) where T : class
        {
            return Get<T>(key, prefix);
        }
        public static object GetCache(string key, string prefix)
        {
            return Get<object>(key, prefix);
        }

        private static T Get<T>(string key, string prefix) where T : class
        {
            if (!string.IsNullOrEmpty(prefix)) { prefix += ":"; }
            return CacheClient.Get<T>(_rootPrefix + prefix + key);
        }

        public static bool RemoveCache(string key, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix)) { prefix += ":"; }
            return CacheClient.Remove(_rootPrefix + prefix + key);
        }

    }
}
