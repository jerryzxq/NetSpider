using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.AssureApi.Job.Scheduler
{
    /// <summary>
    /// SchedulerManager接口
    /// </summary>
    [Description("SchedulerManager接口")]
    public interface ISchedulerManager
    {
        /// <summary>
        /// 初始化Scheduler
        /// <remarks>该接口只能在服务启动时被调用一次</remarks>
        /// </summary>
        void Initialize();

        /// <summary>
        /// 启动Scheduler
        /// </summary>
        void Start();

        /// <summary>
        /// 停止Scheduler
        /// </summary>
        void Stop();

        /// <summary>
        /// 暂停所有Job
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢复所有Job
        /// </summary>
        void Resume();
    }
}
