using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;
using Vcredit.Common.Utility;


namespace Vcredit.NetSpider.Service
{
    internal class CRD_PI_ADMINPNSHMImpl : BaseService<CRD_PI_ADMINPNSHMEntity>, ICRD_PI_ADMINPNSHM
    {
        public IList<CRD_PI_ADMINPNSHMEntity> GetListByReportId(int reportId)
        {
            var ls = base.Find("from CRD_PI_ADMINPNSHMEntity where ReportId=?", new object[] { reportId });
            return ls;
        }
    }

}

