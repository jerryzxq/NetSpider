using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // BasicEntity数据访问对象
    internal class BasicDao : BaseDao<BasicEntity>, IBasicDao 
    {
        public IDaoHelper<BasicEntity> DaoHelper { get; set; }
    }
}
