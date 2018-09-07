using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.Attributes;

namespace Vcredit.ExternalCredit.Dto.Assure
{
    /// <summary>
    /// 担保征信上报提交参数
    /// </summary>
    public class AssureReportedParamDto
    {
        /// <summary>
        /// 担保业务编号（唯一）
        /// </summary>
        [Required(ErrorMessage = "担保业务编号-不能为空")]
        [GuaranteeLetterCodeUnique]
        public string GuaranteeLetterCode { get; set; }

        /// <summary>
        /// 担保合同号码
        /// </summary>
        [Required(ErrorMessage = "担保合同号码-不能为空")]
        public string GuaranteeContractCode { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = "姓名-不能为空")]
        public string WarranteeName { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        [Required(ErrorMessage = "身份证号码-不能为空")]
        [IdentityCheck]
        public string WarranteeCertNo { get; set; }

        /// <summary>
        /// 担保起始日期
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime GuaranteeStartDate { get; set; }

        /// <summary>
        /// 担保到期日期
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime GuaranteeStopDate { get; set; }

        /// <summary>
        /// 担保金额
        /// </summary>
        [DataType(DataType.Currency, ErrorMessage = "必须是数字类型")]
        public decimal GuaranteeSum { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        [DataType(DataType.Currency, ErrorMessage = "必须是数字类型")]
        public decimal Rate { get; set; }

        /// <summary>
        /// 在保余额
        /// </summary>
        [DataType(DataType.Currency, ErrorMessage = "必须是数字类型")]
        public decimal InkeepBalance { get; set; }
    }
}
