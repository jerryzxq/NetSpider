using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Dto.Assure
{
    /// <summary>
    /// 担保上报数据查询DTO
    /// </summary>
    public class AssureReportedQueryDto : BaseAssureQueryDto
    {
        /// <summary>
        /// 放款方
        /// </summary>
        public string CreditorName { get; set; }
    }
}
