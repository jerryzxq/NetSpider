using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_CD_STNCARDEntity数据访问对象
    internal class CRD_CD_STNCARDDao : BaseDao<CRD_CD_STNCARDEntity>, ICRD_CD_STNCARDDao 
    {
        public IDaoHelper<CRD_CD_STNCARDEntity> DaoHelper { get; set; }
    }
}
