using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // NetsEntity数据访问对象
    internal class NetsDao : BaseDao<NetsEntity>, INetsDao 
    {
        public IDaoHelper<NetsEntity> DaoHelper { get; set; }
    }
}
