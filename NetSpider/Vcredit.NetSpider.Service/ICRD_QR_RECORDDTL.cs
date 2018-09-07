using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_QR_RECORDDTLEntity服务对象接口
    public interface ICRD_QR_RECORDDTL : IBaseService<CRD_QR_RECORDDTLEntity> 
    {
        IList<CRD_QR_RECORDDTLEntity> GetListByReportId(int reportId);
    }
}

