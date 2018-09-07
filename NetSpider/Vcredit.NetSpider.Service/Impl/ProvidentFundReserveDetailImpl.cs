using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    // ProvidentFundReserveDetailEntity服务对象
    internal class ProvidentFundReserveDetailImpl : BaseService<ProvidentFundReserveDetailEntity>, IProvidentFundReserveDetail
    {
        public IList<ProvidentFundReserveDetailEntity> GetDetailListByProvidentFundId(long id)
        {
            IList<ProvidentFundReserveDetailEntity> entityList = new List<ProvidentFundReserveDetailEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from provident.ProvidentFundReserveDetail WITH(NOLOCK) where ProvidentFundId=" + id).AddEntity("ProvidentFundReserveDetailEntity", typeof(ProvidentFundReserveDetailEntity)).List<ProvidentFundReserveDetailEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}
