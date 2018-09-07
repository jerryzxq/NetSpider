using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CommonLayer;
namespace Vcredit.ExternalCredit.Dto.Assure
{
    /// <summary>
    /// 担保查询返回结果实体
    /// </summary>
    public class AddQueryInfoResultDto
    {
        /// <summary>
        /// 失败原因
        /// </summary>
        public SysEnums.AssureFailReasonState FailReason { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AddQueryInfoResultDto()
        {
            this.FailReason = SysEnums.AssureFailReasonState.默认;
        }
    }
}
