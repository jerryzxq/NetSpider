using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Processor
{
    public interface IJobExecutor
    {
        decimal GetSalaryByCity(string city);
    }
}
