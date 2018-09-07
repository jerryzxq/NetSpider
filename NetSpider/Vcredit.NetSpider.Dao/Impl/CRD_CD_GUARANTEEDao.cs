using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_CD_GUARANTEEEntity数据访问对象
    internal class CRD_CD_GUARANTEEDao : BaseDao<CRD_CD_GUARANTEEEntity>, ICRD_CD_GUARANTEEDao 
    {
        public IDaoHelper<CRD_CD_GUARANTEEEntity> DaoHelper { get; set; }
    }
}
