using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Cache
{
    public class CacheClient
    {
        /// <summary>
        /// 根据传入的key-value添加一条记录，当key已存在返回false
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <returns></returns>
        public static bool Add<T>(string key, T value)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Add<T>(key, value);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的key-value添加一条记录，当key已存在返回false
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <param name="expiresAt">到达该时间点，立即失效</param>
        /// <returns></returns>
        public static bool Add<T>(string key, T value, DateTime expiresAt)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Add<T>(key, value, expiresAt);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的key-value添加一条记录，当key已存在返回false
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <param name="expiresIn">经过该时间段（TimeSpan），立即失效</param>
        /// <returns></returns>
        public static bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Add<T>(key, value, expiresIn);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的key获取一条记录的值
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            T objRet = default(T);
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    objRet = redisClient.Get<T>(key);
                }
            }
            catch (Exception e)
            {

            }
            return objRet;
        }
        /// <summary>
        /// 根据传入的多个key获取多条记录的值
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="keys">key值集合</param>
        /// <returns></returns>
        public static IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> objRet = null;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    objRet = redisClient.GetAll<T>(keys);
                }
            }
            catch (Exception e)
            {

            }
            return objRet;
        }
        /// <summary>
        /// 根据传入的key移除一条记录
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Remove(key);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的多个key移除多条记录
        /// </summary>
        /// <param name="keys"></param>
        public static void RemoveAll(IEnumerable<string> keys)
        {
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    redisClient.RemoveAll(keys);
                }
            }
            catch (Exception e)
            {

            }
        }
        /// <summary>
        /// 根据传入的key覆盖一条记录的值，当key不存在不会添加
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <returns></returns>
        public static bool Replace<T>(string key, T value)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Replace<T>(key, value);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的key覆盖一条记录的值，当key不存在不会添加
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <param name="expiresAt">到达该时间点，立即失效</param>
        /// <returns></returns>
        public static bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Replace<T>(key, value, expiresAt);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的key覆盖一条记录的值，当key不存在不会添加
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <param name="expiresIn">经过该时间段（TimeSpan），立即失效</param>
        /// <returns></returns>
        public static bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Replace<T>(key, value, expiresIn);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 据传入的key修改一条记录的值，当key不存在则添加
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T value)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Set<T>(key, value);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 据传入的key修改一条记录的值，当key不存在则添加
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <param name="expiresAt">到达该时间点，立即失效</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T value, DateTime expiresAt)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Set<T>(key, value, expiresAt);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 据传入的key修改一条记录的值，当key不存在则添加
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">value值</param>
        /// <param name="expiresIn">经过该时间段（TimeSpan），立即失效</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            bool isRet = false;
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    isRet = redisClient.Set<T>(key, value, expiresIn);
                }
            }
            catch (Exception e)
            {

            }
            return isRet;
        }
        /// <summary>
        /// 根据传入的多个key覆盖多条记录
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="values">Dictionary集合</param>
        public static void SetAll<T>(IDictionary<string, T> values)
        {
            try
            {
                using (var redisClient = RedisManager.GetClient())
                {
                    redisClient.SetAll<T>(values);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
