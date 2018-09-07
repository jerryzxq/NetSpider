using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Dto.Assure
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseAssureQueryDto : DataRequestDto
    {
        /// <summary>
        /// 担保业务编号
        /// </summary>
        public string GuaranteeLetterCode { get; set; }

        /// <summary>
        /// 状态参数
        /// AssureReportState.Default=0; AssureReportState.UpLoadFail=1; AssureReportState.UpLoadSuccess=2; AssureReportState.UpLoading=6
        /// </summary>
        public int? State { get; set; }

        /// <summary>
        /// 数据更新时间Begin
        /// </summary>
        public DateTime? UpdateTimeBegin { get; set; }

        /// <summary>
        /// 数据更新时间End
        /// </summary>
        public DateTime? UpdateTimeEnd { get; set; }
    }
}
