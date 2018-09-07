using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.Common.Utility
{
    public class Quene<T>
    {
        private int maxsize;
        private T[] data;
        private int front;
        private int rear;
        private static Quene<T> crawlerQuene;
        
        /// <summary>
        /// 初始化队列
        /// </summary>
        /// <param name="size"></param>
        public static Quene<T> GetCrawlerQuene(int size, T[] list = null)
        {
            if (crawlerQuene == null)
            {
                crawlerQuene = new Quene<T>(size);
            }
            if (list != null && list.Count() > 0)
            {
                foreach (T t in list)
                    crawlerQuene.Enquene(t);
            }
            return crawlerQuene;
        }

        /// <summary>
        /// 初始化队列
        /// </summary>
        /// <param name="size"></param>
        private Quene(int size)
        {
            data = new T[size];
            maxsize = size;
            front = rear = -1;
        }

        /// <summary>
        /// 取得队列实际元素的个数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            if (rear > front)
            {
                return rear - front;
            }
            else
            {
                return (rear - front + maxsize) % maxsize;
            }
        }

        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return front == rear;
        }

        /// <summary>
        /// 判断队列是否满载
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            if (front != -1) //如果已经有元素出队过
            {
                return (rear + 1) % maxsize == front;//为了区分与IsEmpty的区别，有元素出队过以后，就只有浪费一个位置来保持逻辑正确性.
            }
            else
            {
                return rear == maxsize - 1;
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            front = rear = -1;
        }

        /// <summary>
        /// 入队（即向队列尾部添加一个元素）
        /// </summary>
        /// <param name="item"></param>
        public void Enquene(T item)
        {
            if (IsFull())
            {
                Console.WriteLine("Queue is full");
                return;
            }
            if (rear == maxsize - 1) //如果rear到头了，则循环重来（即解决伪满问题）
            {
                rear = 0;
            }
            else
            {
                rear++;
            }
            data[rear] = item;
        }

        /// <summary>
        /// 出队(即从队列头部删除一个元素)
        /// </summary>
        /// <returns></returns>
        public T Dequene()
        {
            if (IsEmpty())
            {
                Console.WriteLine("Queue is empty");
                return default(T);
            }
            if (front == maxsize - 1) //如果front到头了，则重新置0
            {
                front = 0;
            }
            else
            {
                front++;
            }
            return data[front];
        }

        /// <summary>
        /// 插队
        /// </summary>
        /// <returns></returns>
        public void JumpQueue(T item)
        {
            if (IsEmpty())
            {
                Console.WriteLine("Queue is empty");
                return;
            }

        }

        /// <summary>
        /// 取得队列头部第一元素
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (IsEmpty())
            {
                Console.WriteLine("Queue is empty!");
                return default(T);
            }
            return data[(front + 1) % maxsize];
        }

        public override string ToString()
        {
            if (IsEmpty()) { return "queue is empty."; }

            StringBuilder sb = new StringBuilder();

            if (rear > front)
            {
                for (int i = front + 1; i <= rear; i++)
                {
                    sb.Append(this.data[i].ToString() + ",");
                }
            }
            else
            {
                for (int i = front + 1; i < maxsize; i++)
                {
                    sb.Append(this.data[i].ToString() + ",");
                }

                for (int i = 0; i <= rear; i++)
                {
                    sb.Append(this.data[i].ToString() + ",");
                }
            }
            return "front = " + this.front + " \t rear = " + this.rear + "\t count = " + this.Count() + "\t data = " + sb.ToString().Trim(',');
        }

    }
}
