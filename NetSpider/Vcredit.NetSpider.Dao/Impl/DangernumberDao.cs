using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Dao
{
    internal class DangernumberDao : BaseDao<DangernumberEntity>, IDangernumberDao
    {
        public IDaoHelper<DangernumberEntity> DaoHelper { get; set; }
    }
}
