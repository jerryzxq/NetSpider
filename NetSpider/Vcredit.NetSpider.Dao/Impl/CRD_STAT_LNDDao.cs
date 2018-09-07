using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_STAT_LNDEntity数据访问对象
    internal class CRD_STAT_LNDDao : BaseDao<CRD_STAT_LNDEntity>, ICRD_STAT_LNDDao 
    {
        public IDaoHelper<CRD_STAT_LNDEntity> DaoHelper { get; set; }
    }
}
