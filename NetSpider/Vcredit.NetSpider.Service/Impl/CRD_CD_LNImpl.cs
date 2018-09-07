using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_CD_LNEntity服务对象
    internal class CRD_CD_LNImpl : BaseService<CRD_CD_LNEntity>, ICRD_CD_LN 
    {

        public IList<CRD_CD_LNEntity> GetListByReportId(int reportId)
        {
            var ls = base.Find("from CRD_CD_LNEntity where ReportId=?", new object[] { reportId });
            return ls;
        }
    }
}
	
