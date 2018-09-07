using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_IDENTITY")]
    [Schema("credit")]
    public partial class CRD_PI_IDENTITYEntity : BaseEntity
    {


        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
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


        /// <summary>
        /// 有无个人声明
        /// </summary>
        public string HaveStatement { get; set; }
        /// <summary>
        /// 有无异议标注
        /// </summary>
        public string HaveMark { get; set; }

        /// <summary>
        /// 报告编号
        /// </summary>
        [Ignore]
        public string Report_sn { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [Ignore]
        public string Name { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        ///查询请求时间
        /// </summary>
        [Ignore]
        public DateTime? Query_Time { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 报告时间
        /// </summary>
        [Ignore]
        public DateTime? Report_Create_Time { get; set; }
    }
}