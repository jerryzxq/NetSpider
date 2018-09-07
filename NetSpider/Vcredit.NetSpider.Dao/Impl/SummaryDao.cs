using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // SummaryEntity数据访问对象
    internal class SummaryDao : BaseDao<SummaryEntity>, ISummaryDao 
    {
        public IDaoHelper<SummaryEntity> DaoHelper { get; set; }
    }
}
