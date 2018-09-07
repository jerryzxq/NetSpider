using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Vcredit.Common.Utility;

namespace Vcredit.ExternalCredit.AssureApi.Job.Scheduler
{
    /// <summary>
    /// Scheduler管理器
    /// </summary>
    public class SchedulerManager : ISchedulerManager
    {
        private static readonly SchedulerManager _instance = new SchedulerManager();

        public static SchedulerManager Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Scheduler工厂
        /// </summary>
        private ISchedulerFactory _schedulerFactory;

        /// <summary>
        /// Scheduler实例
        /// </summary>
        private IScheduler _scheduler;

        #region ISchedulerManager Members

        /// <summary>
        /// 初始化Scheduler
        /// <remarks>该接口只能在服务启动时被调用一次</remarks>
        /// </summary>
        public virtual void Initialize()
        {
            try
            {
                _schedulerFactory = new StdSchedulerFactory();
                _scheduler = _schedulerFactory.GetScheduler();

                Log4netAdapter.WriteInfo("【SchedulerManager】【初始化SchedulerManager成功】");
            }
            catch (Exception exception)
            {
                Log4netAdapter.WriteInfo(String.Format("【SchedulerManager】【初始化SchedulerManager失败】【异常：{0}】", exception));
            }
        }

        /// <summary>
        /// 启动Scheduler
        /// </summary>
        public void Start()
        {
            Log4netAdapter.WriteInfo("【SchedulerManager】【开始启动Scheduler】");
            try
            {
                _scheduler.Start();
                while (!_scheduler.IsStarted)
                {
                    Log4netAdapter.WriteInfo("【SchedulerManager】【Scheduler启动中】");
                    Thread.Sleep(1000);
                }
                Log4netAdapter.WriteInfo("【SchedulerManager】【Scheduler启动完成】");
            }
            catch (Exception exception)
            {
                Log4netAdapter.WriteInfo(String.Format("【SchedulerManager】【Scheduler启动失败】【异常：{0}】", exception));
            }
        }

        /// <summary>
        /// 停止Scheduler
        /// </summary>
        public void Stop()
        {
            Log4netAdapter.WriteInfo("【SchedulerManager】【开始停止Scheduler】");
            try
            {
                _scheduler.Shutdown(true);
                while (!_scheduler.IsShutdown)
                {
                    Log4netAdapter.WriteInfo("【SchedulerManager】【Scheduler停止中...】");
                    Thread.Sleep(1000);
                }
                Log4netAdapter.WriteInfo("【SchedulerManager】【Scheduler停止完成】");
            }
            catch (Exception exception)
            {
                Log4netAdapter.WriteInfo(String.Format("【SchedulerManager】【Scheduler停止失败】【异常：{0}】", exception));
            }
        }

        /// <summary>
        /// 暂停所有Job
        /// </summary>
        public void Pause()
        {
            Log4netAdapter.WriteInfo("【SchedulerManager】【开始暂停所有Job】");
            try
            {
                _scheduler.PauseAll();
                Log4netAdapter.WriteInfo("【SchedulerManager】【暂停所有Job完成】");
            }
            catch (Exception exception)
            {
                Log4netAdapter.WriteInfo(String.Format("【SchedulerManager】【暂停所有Job失败】【异常：{0}】", exception));
            }
        }

        /// <summary>
        /// 恢复所有Job
        /// </summary>
        public void Resume()
        {
            Log4netAdapter.WriteInfo("【SchedulerManager】【开始恢复所有Job】");
            try
            {
                _scheduler.ResumeAll();
                Log4netAdapter.WriteInfo("【SchedulerManager】【恢复所有Job完成】");
            }
            catch (Exception exception)
            {
                Log4netAdapter.WriteInfo(String.Format("【SchedulerManager】【恢复所有Job失败】【异常：{0}】", exception));
            }
        }

        #endregion
    }
}