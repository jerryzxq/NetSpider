using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CreditReportEntity数据访问对象
    internal class CreditReportDao : BaseDao<CreditReportEntity>, ICreditReportDao 
    {
        public IDaoHelper<CreditReportEntity> DaoHelper { get; set; }
    }
}
