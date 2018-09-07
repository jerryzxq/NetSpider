using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_STAT_LNEntity数据访问对象
    internal class CRD_STAT_LNDao : BaseDao<CRD_STAT_LNEntity>, ICRD_STAT_LNDao 
    {
        public IDaoHelper<CRD_STAT_LNEntity> DaoHelper { get; set; }
    }
}
