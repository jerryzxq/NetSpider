using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.NetSpider.Cache.Common;
using Vcredit.Common.Ext;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Vcredit.NetSpider.Cache
{
    public class CacheCookie
    {
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveCache(string key)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet=redisClient.Remove(CacheKeys.CookieBaseKey + key);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 设置cookie缓存
        /// </summary>
        /// <param name="key">缓存key</param>
        /// <param name="cookie">对应cookie的object对象</param>
        /// <returns></returns>
        public static bool SetCache(string key, object cookie,int minute=20)
        {
            bool isRet = false;
            try
            {
                
                TimeSpan expiresIn = new TimeSpan(0, 0, minute, 0, 0);//缓存有效时间
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Set<byte[]>(CacheKeys.CookieBaseKey + key, cookie.ToBytes(), expiresIn);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }

        /// <summary>
        /// 获取cookie缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetCache(string key)
        {
            object objRet = null;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    objRet = BytesToObject(redisClient.Get<byte[]>(CacheKeys.CookieBaseKey + key));
                }
            }
            catch (Exception e)
            {

            }
            return objRet;
        }

        /// <summary>
        /// 将一个序列化后的byte[]数组还原
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        private static object BytesToObject(byte[] Bytes)
        {
            if (Bytes == null)
            {
                return null;
            }
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                IFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
            
        }
    }
}
