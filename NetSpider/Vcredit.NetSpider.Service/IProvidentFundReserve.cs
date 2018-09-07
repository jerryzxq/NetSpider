using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    // ProvidentFundReserveEntity服务对象接口
    public interface IProvidentFundReserve : IBaseService<ProvidentFundReserveEntity>
    {
        ProvidentFundReserveEntity GetByProvidentFundId(long id);
    }
}
