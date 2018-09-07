using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Entity.WorkFlow;
using Vcredit.NetSpider.WorkFlow;
using Vcredit.NetSpider.Fetcher;
using System.Net;
using Vcredit.NetSpider.DataManager;
using Vcredit.Common;
using Vcredit.NetSpider.Entity.JobManager;
using System.Data;
using Vcredit.NetSpider.JobManager;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.Processor
{
    public class BaseExecutor : IExecutor
    {
        IJobService JobService = JobServiceManager.GetJobService();//调用Job处理接口
        IWorkflowDataService wfService = DataServiceManager.GetWorkflowDataService();//调用流程数据处理接口
        IRuntimeService runService = ProcessEngine.GetRuntimeService();//调用流程引擎接口
        IFetcherService fetcherService = FetcherServiceManager.GetWebFetcherService();//调用数据采集接口
        IDataSeletorService seletorService = DataServiceManager.GetDataSeletorService();//调用数据筛选接口
        IParameterConvertService paramService = DataServiceManager.GetParameterConvertService();//调用参数处理接口

        Dictionary<string, List<string>> dics = new Dictionary<string, List<string>>();

        CookieCollection cookies = new CookieCollection();
        HttpResult httpresult = null;
        //public void Execute()
        //{
        //    ProcessInstance proc = runService.StartProcess("yidong");
        //    do
        //    {
        //        GetResult(proc);
        //    }
        //    while (!proc.isEnd);

        //    var result = runService.EndProcess(proc);

        //    Console.ReadLine();
        //}

        private void GetResult(ProcessInstance proc)
        {
            foreach (Task taskItem in proc.currenttasks)
            {
                httpresult = (HttpResult)fetcherService.Fetch(taskItem, cookies);
                cookies = httpresult.CookieCollection;
                //cookies = GetCookieCollection(cookies, httpresult.CookieCollection);
                Dictionary<string, List<string>> result = seletorService.GetSeletorResult(taskItem, httpresult.Html);

                //更新抓取流程返回值
                //wfService.SaveOrUpdateProcessData(proc, result);
                //dics = result.Union(dics).ToDictionary(o => o.Key, o => o.Value);
            }

            string GatherData = string.Empty;
            foreach (KeyValuePair<string, List<string>> item in proc.GatherData)
            {
                string strTemp = string.Empty;
                foreach (string strItem in item.Value)
                {
                    strTemp += strItem + ",";
                }
                GatherData += "返回参数:" + strTemp + "\r\n";
            }
            FileOperateHelper.WriteFile("D:\\参数_" + proc.currentsort + ".txt", GatherData);

            GatherData = string.Empty;
            foreach (KeyValuePair<string, string> item in proc.Variable)
            {
                GatherData += item.Value + ",";
            }
            GatherData += "变量:" + GatherData + "\r\n";
            FileOperateHelper.WriteFile("D:\\变量_" + proc.currentsort + ".txt", GatherData);

            runService.CompleteTasks(proc.currenttasks);
        }

        private CookieCollection GetCookieCollection(CookieCollection OldCookies, CookieCollection NewCookies)
        {
            for (int i = 0; i < OldCookies.Count; i++)
            {
                if (NewCookies.Count > 0)
                {
                    for (int j = 0; j < NewCookies.Count; j++)
                    {
                        if (NewCookies[j].Name != OldCookies[i].Name)
                        {
                            NewCookies.Add(OldCookies[i]);
                        }
                    }
                }
                else
                {
                    NewCookies.Add(OldCookies[i]);
                }
            }

            return NewCookies;
        }

        public void Execute(string JobName, IDictionary<string, string> Variable)
        {
            ProcessInstance proc = runService.StartProcess(JobName);
            do
            {
                GetResult(proc);
            }
            while (!proc.isEnd);

            var result = runService.EndProcess(proc);
        }
        /// <summary>
        /// 给流程实例添加内部变量
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="Variable"></param>
        /// <returns></returns>
        private ProcessInstance AddVariable(ProcessInstance proc, Dictionary<string, string> Variable)
        {
            foreach (KeyValuePair<string, string> item in Variable)
            {
                proc.Variable.Add(item.Key, item.Value);
            }

            return proc;
        }

        public void ExecuteById(string id, IDictionary<string, string> Variable, int start, int end)
        {
            try
            {
                if (start == 0)
                {
                    return;
                }
                //启动流程
                ProcessInstance proc = runService.StartProcessById(id, start);

                //如果无结束节点，获取任务配置的结束节点
                if (end == 0)
                {
                    Job Job = JobService.GetJobByName(proc.processDefKey);
                    Step maxStep = Job.Steps.OrderByDescending(o => o.sort).FirstOrDefault();
                    end = maxStep.sort;
                }

                //判断缓存是否有此流程实例的cookie，如果有，赋值
                if (CacheHelper.GetCache(id) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(id);
                }
                //开始执行采集任务
                do
                {
                    foreach (Task taskItem in proc.currenttasks)
                    {
                        HttpResult httpresult = (HttpResult)fetcherService.Fetch(taskItem, cookies);

                        cookies = GetCookieCollection(cookies, httpresult.CookieCollection);
                        CacheHelper.SetCache(id, cookies);
                        //获取返回值，返回类型为字符串list
                        Dictionary<string, List<string>> result = seletorService.GetSeletorResult(taskItem, httpresult.Html);
                        //获取返回值，返回类型为datatable
                        Dictionary<string, DataTable> resultDT = seletorService.GetSeletorGatherDataTable(taskItem, httpresult.Html);
                    }
                    runService.CompleteTasks(proc.currenttasks);
                }
                while (!proc.isEnd && proc.currentsort <= end);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public object ExecuteByIdToObject(string id, IDictionary<string, string> Variable, int start)
        {
            object outputdata = null;
            try
            {
                if (start == 0)
                {
                    return false;
                }
                //启动流程
                ProcessInstance proc = runService.StartProcessById(id, start);

                //判断缓存是否有此流程实例的cookie，如果有，赋值
                if (CacheHelper.GetCache(id) != null)
                {
                    cookies = (CookieCollection)CacheHelper.GetCache(id);
                }

                bool isOutPut = false;
                //开始执行采集任务
                do
                {
                    foreach (Task taskItem in proc.currenttasks)
                    {
                        HttpResult httpresult = (HttpResult)fetcherService.Fetch(taskItem, cookies);

                        cookies = GetCookieCollection(cookies, httpresult.CookieCollection);
                        CacheHelper.SetCache(id, cookies);
                        //判断是否是返回值byte数组，如果是返回
                        if (taskItem.resultType == "byte")
                        {
                            outputdata = httpresult.ResultByte;
                            isOutPut = true;
                        }
                        else
                        {
                            //获取返回值，返回类型为字符串list
                            Dictionary<string, List<string>> result = seletorService.GetSeletorResult(taskItem, httpresult.Html);
                            //获取返回值，返回类型为datatable
                            Dictionary<string, DataTable> resultDT = seletorService.GetSeletorGatherDataTable(taskItem, httpresult.Html);
                        }
                    }
                    runService.CompleteTasks(proc.currenttasks);
                }
                while (!proc.isEnd && !isOutPut);

                return outputdata;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return outputdata;
        }

        /// <summary>
        /// 链式流程执行
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Variable"></param>
        public void ExecuteToEnd(string id, IDictionary<string, string> Variable = null)
        {
            try
            {
                //根据id获取流程实例
                ProcessInstance proc = wfService.GetProcessInstanceById(id);
                //获取此流程实例的配置模板
                Job Job = JobService.GetJobByName(proc.processDefKey);
                //获取此流程的第一步配置
                Step currentStep = Job.Steps.Where(o => o.sort == 1).FirstOrDefault();

                Task firstTask = new Task();
                PluginEmitMapper.Map<Step, Task>(currentStep, firstTask);

                firstTask.id = CommonFun.GetGuidID();
                firstTask.processInstanceId = id;
                if(Variable!=null)
                {
                    firstTask.Variable = (Dictionary<string, string>)Variable;
                }
                paramService.InitTaskVariable(firstTask, null);//重新加载内参

                ExecutorTask(firstTask);
                proc.isEnd = true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 递归执行链式流程
        /// </summary>
        /// <param name="task"></param>
        private void ExecutorTask(Task task)
        {
            try
            {
                ProcessInstance proc = wfService.GetProcessInstanceById(task.processInstanceId);
                Job Job = JobService.GetJobByName(proc.processDefKey);
                Step currentStep = Job.Steps.Where(o => o.sort == task.sort).FirstOrDefault();

                if (!task.url.Contains("http"))
                {
                    return;
                }
                HttpResult httpresult = (HttpResult)fetcherService.Fetch(task, cookies);
                if (httpresult==null||httpresult.StatusCode != HttpStatusCode.OK)
                {
                    return;
                }
                if (httpresult.CookieCollection != null)
                {
                    cookies = GetCookieCollection(cookies, httpresult.CookieCollection);
                }
                CacheHelper.SetCache(task.processInstanceId, cookies);
                Dictionary<string, List<string>> result = seletorService.GetSeletorResult(task, httpresult.Html);
                Dictionary<string, DataTable> resultDT = seletorService.GetSeletorGatherDataTable(task, httpresult.Html);


                Dictionary<string, string> tempVariableDics = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> dicItem in task.Variable)
                {
                    tempVariableDics.Add(dicItem.Key, dicItem.Value);
                }
                if (task.Variable.Count == 0)
                {
                    paramService.InitTaskVariable(task, result);//重新加载内参
                }

                bool isfinish = true;//当前步骤是否结束
                bool isSkip = false;//步骤是否跳过
                bool isEnter = true;//步骤是否进入
                bool isFlowEnd = false;//当前子流程是否结束

                //当前子流程是否结束
                if (!String.IsNullOrEmpty(currentStep.isFlowEndExpr))
                {
                    isFlowEnd = wfService.AnalyticExpressionFromTask(result, task.Variable, currentStep.isFlowEndExpr);
                    if (isFlowEnd)
                    {
                        return;
                    }
                }

                //判断是否结束此步骤
                if (!String.IsNullOrEmpty(currentStep.isFinishExpr))
                {
                    isfinish = wfService.AnalyticExpressionFromTask(result, task.Variable, currentStep.isFinishExpr);
                }

                if (isfinish)
                {
                    task.nexttasks = wfService.GetNextTasks(task, task.GatherData);
                    if (task.nexttasks.Count > 0)
                    {
                        int i = 0;
                        foreach (Task taskItem in task.nexttasks)
                        {
                            if (task.sort == 1 && i == 0)
                            {

                            }
                            if (task.sort == 1)
                            {

                            }
                            i++;
                            ExecutorTask(taskItem);
                        }
                    }
                }
                else
                {
                    string procId = task.processInstanceId;
                    PluginEmitMapper.Map<Step, Task>(currentStep, task);

                    task.id = CommonFun.GetGuidID();
                    task.processInstanceId = procId;
                    task.Variable = tempVariableDics;
                    paramService.InitTaskVariable(task, result);//重新加载内参
                    ExecutorTask(task);
                }

                return;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
