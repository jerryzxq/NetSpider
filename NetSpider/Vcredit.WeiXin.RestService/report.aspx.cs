using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vcredit.NetSpider.Service;
using Vcredit.Common.Ext;

namespace Vcredit.WeiXin.RestService
{
    /// <summary>
    /// 查询征信报告
    /// </summary>
    public partial class report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["ReportSn"] != null)
            {
                string ReportSn = Request["ReportSn"];
                ICRD_HD_REPORT_HTML service = NetSpiderFactoryManager.GetCRDHDREPORTHTMLService();
                var entity = service.GetByReportSn(ReportSn);
                if (entity != null)
                {
                    Response.Write(entity.Html);
                }
            }
            if (Request["ReportId"] != null)
            {
                string ReportId = Request["ReportId"];
                ICRD_HD_REPORT_HTML service = NetSpiderFactoryManager.GetCRDHDREPORTHTMLService();
                var entity = service.GetByReportId(ReportId.ToInt(0));
                if (entity != null)
                {
                    Response.Write(entity.Html);
                }
            }

        }
    }
}