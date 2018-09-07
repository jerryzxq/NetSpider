using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_CD_LNEntity数据访问对象
    internal class CRD_CD_LNDao : BaseDao<CRD_CD_LNEntity>, ICRD_CD_LNDao 
    {
        public IDaoHelper<CRD_CD_LNEntity> DaoHelper { get; set; }
    }
}
