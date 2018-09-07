using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.WorkFlow;

namespace Vcredit.NetSpider.Processor
{
    public interface IExecutor
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="JobName">Job名称</param>
        /// <param name="Variable">内参</param>
        void Execute(string JobName, IDictionary<string, string> Variable=null);
        /// <summary>
        /// 根据流程实例Id,执行
        /// </summary>
        /// <param name="Id">流程实例Id</param>
        /// <param name="Variable">内参</param>
        /// <param name="start">开始节点</param>
        /// <param name="end">结束节点</param>
        void ExecuteById(string Id, IDictionary<string, string> Variable = null, int start=0, int end=0);
        /// <summary>
        /// 根据流程实例Id,执行,并返回输出数据
        /// </summary>
        /// <param name="Id">流程实例Id</param>
        /// <param name="Variable">内参</param>
        /// <param name="start">开始节点</param>
        /// <returns></returns>
        object ExecuteByIdToObject(string Id, IDictionary<string, string> Variable = null, int start = 0);

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="JobName">Job名称</param>
        /// <param name="Variable">内参</param>
        void ExecuteToEnd(string JobName, IDictionary<string, string> Variable = null);
    }
}
