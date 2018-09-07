using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CreditReportSummaryEntity数据访问对象
    internal class CreditReportSummaryDao : BaseDao<CreditReportSummaryEntity>, ICreditReportSummaryDao 
    {
        public IDaoHelper<CreditReportSummaryEntity> DaoHelper { get; set; }
    }
}
