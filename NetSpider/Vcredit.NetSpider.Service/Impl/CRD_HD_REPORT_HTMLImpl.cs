using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_HD_REPORT_HTMLEntity服务对象
    internal class CRD_HD_REPORT_HTMLImpl : BaseService<CRD_HD_REPORT_HTMLEntity>, ICRD_HD_REPORT_HTML 
    {

        public CRD_HD_REPORT_HTMLEntity GetByReportSn(string ReportSn)
        {
            CRD_HD_REPORT_HTMLEntity entity = base.Find("from CRD_HD_REPORT_HTMLEntity where ReportSn=?", new object[] { ReportSn }).FirstOrDefault();

            return entity;
        }


        public CRD_HD_REPORT_HTMLEntity GetByReportId(int ReportId)
        {
            CRD_HD_REPORT_HTMLEntity entity = base.Find("from CRD_HD_REPORT_HTMLEntity where ReportSn in (select ReportSn from CRD_HD_REPORTEntity where Id=?)", new object[] { ReportId }).FirstOrDefault();

            return entity;
        }
    }
}
	
