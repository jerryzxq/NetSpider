using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CreditReportHtmlEntity数据访问对象
    internal class CreditReportHtmlDao : BaseDao<CreditReportHtmlEntity>, ICreditReportHtmlDao 
    {
        public IDaoHelper<CreditReportHtmlEntity> DaoHelper { get; set; }
    }
}
