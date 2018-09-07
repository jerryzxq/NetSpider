using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;


namespace  Vcredit.NetSpider.Service
{
    // CreditReportEntity服务对象
    internal class CreditReportImpl : BaseService<CreditReportEntity>, ICreditReport 
    {
        ICreditReportQueryHisDao queryhisDao;
        ICreditReportSummaryDao summaryDao;
        ICreditCardDao cardDao;
        ICreditReportHtmlDao htmlDao;
        public override object Save(CreditReportEntity entity)
        {
            base.Save(entity);
            foreach (var detail in entity.CreditReportSummaryList)
            {
                detail.ReportId = entity.ReportId;
                summaryDao.Save(detail);
            }
            foreach (var detail in entity.CreditReportQueryHisList)
            {
                detail.ReportId = entity.ReportId;
                queryhisDao.Save(detail);
            }
            foreach (var detail in entity.CreditCardList)
            {
                detail.ReportId = entity.ReportId;
                cardDao.Save(detail);
            }
            htmlDao.Save(entity.CreditReportHtml);

            return entity.ReportId;
        }
    }
}
	
