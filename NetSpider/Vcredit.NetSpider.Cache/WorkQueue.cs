using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
using ServiceStack.Redis.Support.Queue;
using ServiceStack.Redis.Support.Queue.Implementation;

namespace Vcredit.NetSpider.Cache
{
    public class WorkQueue
    {
        static int maxReadPoolSize = RedisManager.redisConfigInfo.MaxReadPoolSize;
        static int maxWritePoolSize = RedisManager.redisConfigInfo.MaxWritePoolSize;
        static string WriteServerList = RedisManager.redisConfigInfo.WriteServerList;

        private static ISimpleWorkQueue<T> GetQueueClient<T>(string queueName = "vcredit") where T : class
        {
            string[] arrStr = WriteServerList.Split(':');

            return new RedisSimpleWorkQueue<T>(maxReadPoolSize, maxWritePoolSize, arrStr[0], (int)arrStr[1].ToInt(6379), queueName);
        }

        /// <summary>
        /// 进队列
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="workItem">进队列数据</param>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        public static bool Enqueue<T>(T workItem, string queueName = "vcredit") where T : class
        {
            bool isRet = false;
            try
            {
                using (var queue = GetQueueClient<T>(queueName))
                {
                    queue.Enqueue(workItem);
                    isRet = true;
                }
            }
            catch (Exception e)
            {
            }
            return isRet;
        }
        /// <summary>
        /// 出队列（单个）
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        public static T Dequeue<T>(string queueName = "vcredit") where T : class
        {
            T objList = default(T);
           try
            {
                using (var queue = GetQueueClient<T>(queueName))
                {
                    objList = queue.Dequeue(1).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
            
            }
           return objList;
        }
        /// <summary>
        /// 出队列（多个）
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="maxBatchSize">获取个数</param>
        /// <returns></returns>
        public static IList<T> DequeueByList<T>(string queueName = "vcredit", int maxBatchSize = 1) where T : class
        {
            IList<T> objList = default(IList<T>);
            try
            {
                using (var queue = GetQueueClient<T>(queueName))
                {
                    objList = queue.Dequeue(maxBatchSize);
                }
            }
            catch (Exception e)
            {

            }
            return objList;
        }
    }
}
