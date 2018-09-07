using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.Processor
{
    public interface IVcreditCertifyExecutor
    {

        BaseRes GetVcreditCertify(string sort, string username, string password, params string[] prams);
    }
}
