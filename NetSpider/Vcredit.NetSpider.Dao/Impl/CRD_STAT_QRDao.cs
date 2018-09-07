using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_STAT_QREntity数据访问对象
    internal class CRD_STAT_QRDao : BaseDao<CRD_STAT_QREntity>, ICRD_STAT_QRDao 
    {
        public IDaoHelper<CRD_STAT_QREntity> DaoHelper { get; set; }
    }
}
