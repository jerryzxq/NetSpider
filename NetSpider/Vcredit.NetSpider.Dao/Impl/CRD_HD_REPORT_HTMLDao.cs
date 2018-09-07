using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_HD_REPORT_HTMLEntity数据访问对象
    internal class CRD_HD_REPORT_HTMLDao : BaseDao<CRD_HD_REPORT_HTMLEntity>, ICRD_HD_REPORT_HTMLDao 
    {
        public IDaoHelper<CRD_HD_REPORT_HTMLEntity> DaoHelper { get; set; }
    }
}
