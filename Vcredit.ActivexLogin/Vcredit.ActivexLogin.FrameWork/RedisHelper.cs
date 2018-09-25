using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis.Support.Queue;
using ServiceStack.Redis.Support.Queue.Implementation;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Cache;

namespace Vcredit.ActivexLogin.FrameWork
{
    public class RedisHelper
    {
        private const string _rootPrefix = "ActivexLogin:";

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

        /// <summary>
        /// 塞入队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="package"></param>
        public static void Enqueue<T>(T data, string package) where T : class
        {
            WorkQueue.Enqueue(data, _rootPrefix + package);
        }
        /// <summary>
        /// 从队列获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="package"></param>
        /// <returns></returns>
        public static T Dequeue<T>(string package) where T : class
        {
            return WorkQueue.Dequeue<T>(_rootPrefix + package);
        }

    }
}
