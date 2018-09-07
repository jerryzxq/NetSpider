using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace  Vcredit.NetSpider.Service
{
    // ProvidentFundLoanEntity服务对象接口
    public interface IProvidentFundLoan : IBaseService<ProvidentFundLoanEntity> 
    {
        ProvidentFundLoanEntity GetByProvidentFundId(long id);
    }
}


