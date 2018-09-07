using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_CD_LNDEntity数据访问对象
    internal class CRD_CD_LNDDao : BaseDao<CRD_CD_LNDEntity>, ICRD_CD_LNDDao 
    {
        public IDaoHelper<CRD_CD_LNDEntity> DaoHelper { get; set; }
    }
}
