using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Dao
{
    // Spd_LoginEntity数据访问对象
    internal class Spd_applyDao : BaseDao<Spd_applyEntity>, ISpd_applyDao
    {
        public IDaoHelper<Spd_applyEntity> DaoHelper { get; set; }
    }
}
