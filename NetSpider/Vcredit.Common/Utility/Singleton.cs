using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.Common
{
    /// <summary>
    /// 单例模式
    /// </summary>
    public sealed class Singleton<T> where T : new()
    {
        private Singleton() { }
        public static T Instance
        {
            get { return SingletonCreator.instance; }
        }
        internal class SingletonCreator
        {
            static SingletonCreator() { }
            internal static readonly T instance = new T();
        }
    }
}
