using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Variable_mobile_summaryEntity服务对象接口
    public interface IVariable_mobile_summary : IBaseService<Variable_mobile_summaryEntity>
    {
        void DeleteBySourceIdAndSourceType(string sourceId, string sourceType);

        Variable_mobile_summaryEntity GetByBusIdentityNoAndMobile(string busIdentityNo, string Mobile, string BusType=null);

        Variable_mobile_summaryEntity GetBySourceIdAndSourceType(string sourceId, string sourceType);

    }
}

