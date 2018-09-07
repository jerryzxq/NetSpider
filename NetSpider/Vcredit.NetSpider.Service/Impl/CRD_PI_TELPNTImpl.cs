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
    internal class CRD_PI_TELPNTImpl : BaseService<CRD_PI_TELPNTEntity>, ICRD_PI_TELPNT
    {
        public IList<CRD_PI_TELPNTEntity> GetListByReportId(int reportId)
        {
            var ls = base.Find("from CRD_PI_TELPNTEntity where ReportId=?", new object[] { reportId });
            return ls;
        }
    }

}

