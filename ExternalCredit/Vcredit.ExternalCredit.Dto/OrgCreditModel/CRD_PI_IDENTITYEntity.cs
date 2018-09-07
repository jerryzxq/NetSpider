using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExternalCredit.Dto.OrgCreditModel
{

    public partial class CRD_PI_IdentityDto : BaseDto
    {


        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }
      
        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 婚姻状况
        /// </summary>
        public string Marital_State { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 单位电话
        /// </summary>
        public string Office_Telephone_No { get; set; }
        /// <summary>
        /// 住宅电话
        /// </summary>
        public string Home_Telephone_No { get; set; }
        /// <summary>
        /// 学历
        /// </summary>
        public string Edu_Level { get; set; }
        /// <summary>
        /// 学位
        /// </summary>
        public string Edu_Degree { get; set; }
        /// <summary>
        /// 通讯地址
        /// </summary>
        public string Post_Address { get; set; }
        /// <summary>
        /// 户籍地址
        /// </summary>
        public string Registered_Address { get; set; }
        /// <summary>
        /// 配偶姓名
        /// </summary>
        public string Mate_Name { get; set; }
        /// <summary>
        /// 配偶证件类型
        /// </summary>
        public string Mate_Cert_Type { get; set; }
        /// <summary>
        /// 配偶证件号码
        /// </summary>
        public string Mate_Cert_No { get; set; }
        /// <summary>
        /// 配偶工作单位
        /// </summary>
        public string Mate_Employer { get; set; }
        /// <summary>
        /// 配偶联系电话
        /// </summary>
        public string Mate_Telephone_No { get; set; }




      

    }
}