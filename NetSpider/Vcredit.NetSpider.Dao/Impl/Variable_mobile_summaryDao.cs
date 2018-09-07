using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_HD_REPORT_HTMLEntity数据访问对象
    internal class Variable_mobile_summaryDao : BaseDao<Variable_mobile_summaryEntity>, IVariable_mobile_summaryDao 
    {
        public IDaoHelper<Variable_mobile_summaryEntity> DaoHelper { get; set; }
    }
}

