using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.ExternalCredit.Attributes;

namespace Vcredit.ExternalCredit.Dto
{
    /// <summary>
    /// 征信查询实体
    /// </summary>
    public class AssureQueryUserInfoParamDto
    {
        ///// <summary>
        ///// 身份证过期日期
        ///// </summary>
        //[Cert_ExpDateCheck]
        //[Required(ErrorMessage = "请输入身份证过期日期")]
      
        //public DateTime Cert_ExpDate { get; set; }
        /// <summary>
        /// 证件类型
        /// {"0","身份证"},{"1","户口本"},{"2","护照"},{"3","军官证"},{"4","士兵证"},{"5","港澳居民来往内地通行证"},{"6","台湾同胞来往内地通行证"},{"7","临时身份证"},{"8","外国人居留证"},{"9","警官证"},{"A","香港身份证"},{"B","澳门身份证"},{"C","台湾身份证"},{"X","其他证件"}
        /// </summary>
        [Required(ErrorMessage = "请输入证件类型")]
        [CardTypeCheck]
        public string CertType { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        [Required(ErrorMessage = "请输入证件号码")]
        [IdentityCheck]
        [ReTryCheck]
        public string CertNo { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = ("姓名不能为空"))]
		[NameCheck]
        public string Name { get; set; }

        /// <summary>
        /// 查询原因
        /// {"01","贷后管理"},{"02","贷款审批"},{"03","担保资格查询"}
        /// </summary>
        [Required]
        [QueryReasonCheck]
        public string QueryReason { get; set; }

        /// <summary>
        /// 业务方
        /// </summary>
        [Required(ErrorMessage = "业务方不能为空")]
        public string BusType { get; set; }

        /// <summary>
        /// VBS ApplyID
        /// </summary>
        [Required(ErrorMessage = "ApplyID不能为空")]
        public int ApplyID { get; set; }
    }
}