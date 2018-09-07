using ServiceStack.Messaging;
using ServiceStack.Messaging.Redis;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Cache;

namespace Vcredit.Framework.Queue.Redis
{
    public class RedisQueue
    {
        /// <summary>
        /// redisPrefix
        /// </summary>
        private const string queuePrefix = "Queue:";

        /// <summary>
        /// redis配置文件信息
        /// </summary>
        private static RedisConfigInfo redisConfigInfo = RedisConfigInfo.GetConfig();

        #region 发布到队列

        /// <summary>
        /// 发布到队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">发布到队列实体</param>
        /// <param name="queueName">消息队列名称</param>
        /// <returns></returns>
        public static void Send<T>(T t, string queueName)
        {
            if (t == null)
                throw new ArgumentException("请求参数不能为空！");

            var list = new List<T>();
            list.Add(t);
            Publish(list, queueName);
        }

        /// <summary>
        /// 发布集合到队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="queueName"></param>
        public static void Send<T>(IEnumerable<T> list, string queueName)
        {
            if (list == null || !list.Any())
                throw new ArgumentException("请求参数不能为空！");

            Publish(list, queueName);
        }
        private static void Publish<T>(IEnumerable<T> list, string queueName)
        {
            if (list == null || !list.Any())
                throw new ArgumentException("请求参数不能为空！");

            queueName = queuePrefix + queueName;

            using (var redisManager = new RedisManagerPool(redisConfigInfo.WriteServerList.Split(new[] { ',' })
                                                            , new RedisPoolConfig
                                                            {
                                                                MaxPoolSize = redisConfigInfo.MaxWritePoolSize
                                                            }))
            {
                using (IMessageFactory redisMqFactory = new RedisMessageFactory(redisManager))
                {
                    using (var mqClient = redisMqFactory.CreateMessageQueueClient())
                    {
                        foreach (var t in list)
                        {
                            mqClient.Publish(queueName, new Message<T>
                            {
                                Body = t
                            });
                        }
                    }
                }
            }
        }
        #endregion

        #region 从队列中获取

        /// <summary>
        /// 从队列中获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="act">消息获取后回掉</param>
        /// <param name="queueName">消息队列名称</param>
        public static void Receive<T>(Action<T> act, string queueName)
        {
            queueName = queuePrefix + queueName;
            using (var redisManager = new RedisManagerPool(redisConfigInfo.ReadServerList.Split(new[] { ',' })
                                                            , new RedisPoolConfig
                                                            {
                                                                MaxPoolSize = redisConfigInfo.MaxReadPoolSize
                                                            }))
            {



                using (IMessageFactory redisMqFactory = new RedisMessageFactory(redisManager))
                {

                    using (var mqClient = redisMqFactory.CreateMessageQueueClient())
                    {
                        while (true)
                        {
                            try
                            {
                                //Blocks thread on client until reply message is received
                                var t = mqClient.Get<T>(queueName).GetBody();
                                act(t);
                            }
                            catch (Exception ex)
                            {
                                Log4netAdapter.WriteError("获取队列异常", ex);
                            }
                        }

                    }
                }
            }

        }


        #endregion
    }
}
