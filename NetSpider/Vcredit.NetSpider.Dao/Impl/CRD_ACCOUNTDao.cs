using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_ACCOUNTEntity数据访问对象
    internal class CRD_ACCOUNTDao : BaseDao<CRD_ACCOUNTEntity>, ICRD_ACCOUNTDao 
    {
        public IDaoHelper<CRD_ACCOUNTEntity> DaoHelper { get; set; }
    }
}
