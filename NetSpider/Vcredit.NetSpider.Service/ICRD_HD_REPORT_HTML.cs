using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_HD_REPORT_HTMLEntity服务对象接口
    public interface ICRD_HD_REPORT_HTML : IBaseService<CRD_HD_REPORT_HTMLEntity> 
    {
        /// <summary>
        /// 根据报告编号，查询报告原始数据
        /// </summary>
        /// <param name="ReportSn"></param>
        /// <returns></returns>
        CRD_HD_REPORT_HTMLEntity GetByReportSn(string ReportSn);
        /// <summary>
        /// 根据报告ID，查询报告原始数据
        /// </summary>
        /// <param name="ReportSn"></param>
        /// <returns></returns>
        CRD_HD_REPORT_HTMLEntity GetByReportId(int ReportId);
    }
}

