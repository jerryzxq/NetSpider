using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    internal class CRD_CD_ASRREPAYImpl : BaseService<CRD_CD_ASRREPAYEntity>, ICRD_CD_ASRREPAY
    {
        public IList<CRD_CD_ASRREPAYEntity> GetListByReportId(int reportId)
        {
            var ls = base.Find("from CRD_CD_ASRREPAYEntity where ReportId=?", new object[] { reportId });
            return ls;
        }
    }
}
