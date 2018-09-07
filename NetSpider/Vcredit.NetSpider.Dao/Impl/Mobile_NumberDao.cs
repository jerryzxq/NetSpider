using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Dao
{
    internal class Mobile_NumberDao : BaseDao<Mobile_NumberEntity>, IMobile_NumberDao
    {
        public IDaoHelper<Mobile_NumberEntity> DaoHelper { get; set; }
    }
}
