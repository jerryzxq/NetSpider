using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    internal class CRD_PI_FORCEEXCTNImpl : BaseService<CRD_PI_FORCEEXCTNEntity>, ICRD_PI_FORCEEXCTN 
    {
        public IList<CRD_PI_FORCEEXCTNEntity> GetListByReportId(int reportId)
        {
            var ls = base.Find("from CRD_PI_FORCEEXCTNEntity where ReportId=?", new object[] { reportId });
            return ls;
        }
    }
}
