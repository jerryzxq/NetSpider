using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Dao
{
    // ProvidentFundReserveEntity数据访问对象
    internal class ProvidentFundReserveDao : BaseDao<ProvidentFundReserveEntity>, IProvidentFundReserveDao
    {
        public IDaoHelper<ProvidentFundReserveEntity> DaoHelper { get; set; }
    }
}
