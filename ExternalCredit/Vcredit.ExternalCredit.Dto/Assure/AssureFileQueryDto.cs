using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.Dto.Assure
{
    /// <summary>
    /// 担保文件查询参数DTO
    /// </summary>
    public class AssureFileQueryDto : DataRequestDto
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int? State { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime? CreateTimeBegin { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime? CreateTimeEnd { get; set; }
    }
}
