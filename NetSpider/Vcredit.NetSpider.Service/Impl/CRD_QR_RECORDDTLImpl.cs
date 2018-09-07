using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_QR_RECORDDTLEntity服务对象
    internal class CRD_QR_RECORDDTLImpl : BaseService<CRD_QR_RECORDDTLEntity>, ICRD_QR_RECORDDTL 
    {
        public IList<CRD_QR_RECORDDTLEntity> GetListByReportId(int reportId)
        {
            var ls = base.Find("from CRD_QR_RECORDDTLEntity where ReportId=?", new object[] { reportId });
            return ls;
        }
    }
}
	
