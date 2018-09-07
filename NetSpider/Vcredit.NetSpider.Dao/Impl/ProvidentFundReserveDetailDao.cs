using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Dao
{
    // ProvidentFundReserveDetailEntity数据访问对象
    internal class ProvidentFundReserveDetailDao : BaseDao<ProvidentFundReserveDetailEntity>, IProvidentFundReserveDetailDao
    {
        public IDaoHelper<ProvidentFundReserveDetailEntity> DaoHelper { get; set; }
    }
}
