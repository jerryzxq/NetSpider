using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Vcredit.ExtTrade.ModelLayer.Common
{
    [Alias("CRD_CD_CreditUserInfo")]
    [Schema("credit")]
    public partial class CRD_CD_CreditUserInfoEntity
    {
        public CRD_CD_CreditUserInfoEntity()
        {
            Time_Stamp = DateTime.Now;
        }

         [AutoIncrement]
         [PrimaryKey]
        public decimal? CreditUserInfo_Id { get; set; }

        /// <summary>
        /// 过期天数
        /// </summary>
        public int ExpiryDate_Num { get; set; }
        /// <summary>
        /// 授权文件日期
        /// </summary>
        public string Authorization_Date { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///证件类型
        /// </summary>
        public string Cert_Type { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        public string Cert_No { get; set; }
        /// <summary>
        /// 机构代码
        /// </summary>
        public string Query_Org { get; set; }
        /// <summary>
        /// 用户代码
        /// </summary>
        public string User_Code { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error_Reason { get; set; }
        /// <summary>
        /// 状态 
        /// </summary>
        public byte? State { get; set; }

        public string UpLoadDirectoryName { get; set; }
        public DateTime? Time_Stamp { get; set; }
        /// <summary>
        /// 授权文件图片base64
        /// </summary>
        [Ignore]
        public string AuthorizationFileBase64String { get; set; }
        /// <summary>
        /// 身份证文件图片base64
        /// </summary>
        [Ignore]
        public string CertFileBase64String { get; set; }

        /// <summary>
        /// 姓名首字母
        /// </summary>
        public string NameFirstLetter { get; set; }

        
        /// <summary>
        /// 本地保存的日期文件夹
        /// </summary>
        public string LocalDirectoryName { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusType { get; set; }

        /// <summary>
        /// 查询原因
        /// </summary>
        public string QueryReason { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public int SourceType { get; set; }
        /// <summary>
        /// 报告编号
        /// </summary>
        public string Report_sn { get; set; }
        public override string ToString()
        {
            return string.Format(@"Query_Org:{0},User_Code:{1},Name:{2},Cert_Type:{3},Cert_NO:{4},Authorization_Date:{5},
            ExpiryDate_Num:{6},NameFirstLetter:{7},BusType{8}", Query_Org, User_Code, Name, Cert_Type, Cert_No, Authorization_Date, ExpiryDate_Num, NameFirstLetter,BusType);
        }

        /// <summary>
        /// 批次号
        /// </summary>
        public string BatNo { get; set; }
        /// <summary>
        /// 查询征信流水号
        /// </summary>
        public string PactNo { get; set; }
        /// <summary>
        /// 是否已经授权
        /// </summary>
        public bool? czAuth { get; set; }
        /// <summary>
        /// 是否有身份证信息
        /// </summary>
        public bool? czId { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 授权文件状态
        /// </summary>
        public int? FileState  { get; set; }
        /// <summary>
        /// 上报时间
        /// </summary>
        public DateTime? ReportTime { get; set; }
        /// <summary>
        /// 上报UKEY
        /// </summary>
        public string Ukey { get; set; }
        /// <summary>
        /// VBS ApplyID
        /// </summary>
        public int? ApplyID { get; set; }
        /// <summary>
        /// 授权方式01：线上，02：线下
        /// </summary>
        public string grantType { get; set; }
        /// <summary>
        /// 身份证过期日期
        /// </summary>
        [Ignore]
        public string Cert_ExpDate { get; set; }
    }
}