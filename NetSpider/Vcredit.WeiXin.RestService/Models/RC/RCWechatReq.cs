using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models.RC
{
    public class RCWechatReq
    {
        /// <summary>
        /// 身份证号
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string Identitycard { get; set; }
        /// <summary>
        /// 是否本地籍
        /// </summary>
        public string IsLocal { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 社保评分
        /// </summary>
        public int SocialScore { get; set; }
        /// <summary>
        /// 征信评分
        /// </summary>
        public int CreditScore { get; set; }
        /// <summary>
        /// 手机评分
        /// </summary>
        public int MobileScore { get; set; }

        /// <summary>
        /// 是否征信空白
        /// </summary>
        public string IsVcreditNull { get; set; }

        /// <summary>
        /// 手机ID
        /// </summary>
        public string MobileId { get; set; }

        /// <summary>
        /// 征信编号
        /// </summary>
        public string ReportSn { get; set; }

        #region 星星钱袋
        /// <summary>
        /// 是否加贷
        /// </summary>
        public string SocialType { get; set; }
        /// <summary>
        /// 是否加贷
        /// </summary>
        public string CustomerScore { get; set; }
        /// <summary>
        /// 大学名称
        /// </summary>
        public string University { get; set; }
        /// <summary>
        /// 是否全日制
        /// </summary>
        public string University_Isfulltime { get; set; }
        /// <summary>
        /// 大学年级
        /// </summary>
        public string University_Grade { get; set; }
        /// <summary>
        /// 距离毕业月数
        /// </summary>
        public string University_Graduate_Remainmonth { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// 是否新生
        /// </summary>
        public string University_IsNewstudent { get; set; }
        /// <summary>
        /// 是否在校学生
        /// </summary>
        public string University_IsAtSchool { get; set; }
        /// <summary>
        /// 层次
        /// </summary>
        public string University_level { get; set; }
        /// <summary>
        /// 所在高校是否开通服务
        /// </summary>
        public string University_IsOnService { get; set; }
        /// <summary>
        /// 入学时间
        /// </summary>
        public DateTime? Enrollment_Date { get; set; }
        /// <summary>
        /// 离校时间
        /// </summary>
        public DateTime? Leaving_Date { get; set; }
        /// <summary>
        /// 学制
        /// </summary>
        public double Schooling_Length { get; set; }

        /// <summary>
        /// 鹏元评分
        /// </summary>
        public string Pengyuan_Score { get; set; }
        /// <summary>
        /// 认证模式
        /// </summary>
        public string Auth_Mode { get; set; }
        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// 渠道
        /// </summary>
        public string Customer_Channel { get; set; }
        /// <summary>
        /// 是否211院校
        /// </summary>
        public string IS211 { get; set; }
        /// <summary>
        /// 是否985院校
        /// </summary>
        public string IS985 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Front_Education { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Mobile_Is_Checked { get; set; }
        #endregion
    }
}
