using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // ProvidentFundDetailEntity服务对象接口
    public interface IProvidentFundDetail : IBaseService<ProvidentFundDetailEntity> 
    {
        IList<ProvidentFundDetailEntity> GetDetailListByProvidentFundId(int id);
    }
}

