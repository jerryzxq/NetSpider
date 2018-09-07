using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cn.Vcredit.ThirdParty.Juxinli;
using Quartz;

namespace Vcredit.WeiXin.RestService.Job
{
    class GetJxlMobileReportJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            DataProcess process = new DataProcess();
            process.GetData();
        }
    }
}
