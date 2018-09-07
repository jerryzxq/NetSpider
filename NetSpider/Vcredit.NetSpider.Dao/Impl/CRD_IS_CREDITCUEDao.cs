using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_IS_CREDITCUEEntity数据访问对象
    internal class CRD_IS_CREDITCUEDao : BaseDao<CRD_IS_CREDITCUEEntity>, ICRD_IS_CREDITCUEDao 
    {
        public IDaoHelper<CRD_IS_CREDITCUEEntity> DaoHelper { get; set; }
    }
}
