using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_QR_RECORDDTLEntity数据访问对象
    internal class CRD_QR_RECORDDTLDao : BaseDao<CRD_QR_RECORDDTLEntity>, ICRD_QR_RECORDDTLDao 
    {
        public IDaoHelper<CRD_QR_RECORDDTLEntity> DaoHelper { get; set; }
    }
}
