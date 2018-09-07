using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    public interface ICRD_PI_TAXARREAR : IBaseService<CRD_PI_TAXARREAREntity>
    {
        /// <summary>
        /// 根据Bid,查询征信报告数据
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <param name="isAll">是否包含三个统计表</param>
        /// <returns></returns>
        IList<CRD_PI_TAXARREAREntity> GetListByReportId(int reportId);
    }
}
