using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vcredit.Common.Utility;

namespace Vcredit.ExternalCredit.AssureApi.Job
{
    /// <summary>
    /// 下载用户征信 Job
    /// </summary>
    public class DownloadUserCreditJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Log4netAdapter.WriteInfo("该Job方法没有实现");
        }
    }
}