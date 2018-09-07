using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CallsEntity数据访问对象
    internal class CallsDao : BaseDao<CallsEntity>, ICallsDao 
    {
        public IDaoHelper<CallsEntity> DaoHelper { get; set; }
    }
}
