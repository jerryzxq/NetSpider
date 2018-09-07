using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.DataAccess.Mongo;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class JobExecutor : IJobExecutor
    {
        public decimal GetSalaryByCity(string city)
        {
            JobuiMongo jb = new JobuiMongo();
            var joBui = jb.Load(city);
            if (joBui != null)
            {
                return joBui.salary;
            }
            return 0;
        }
    }
}
