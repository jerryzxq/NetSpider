using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_HD_REPORTEntity数据访问对象
    internal class CRD_HD_REPORTDao : BaseDao<CRD_HD_REPORTEntity>, ICRD_HD_REPORTDao 
    {
        public IDaoHelper<CRD_HD_REPORTEntity> DaoHelper { get; set; }
    }
}
