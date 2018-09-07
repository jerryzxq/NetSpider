using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    public interface ICRD_CD_ASRREPAY : IBaseService<CRD_CD_ASRREPAYEntity>
    {
        /// <summary>
        /// 根据Bid,保证人代偿信息
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <param name="isAll">是否包含三个统计表</param>
        /// <returns></returns>
        IList<CRD_CD_ASRREPAYEntity> GetListByReportId(int reportId);
    }
}
