using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Chsi
{
    public class ChsiRegisterReq : BaseReq
    {
        /// <summary>
        ///  姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 是否忽略已注册的手机号
        /// </summary>
        public string ignoremphone { get; set; }

        //private string _ignoremphone = "false";
        ///// <summary>
        ///// 是否忽略已注册的手机号
        ///// </summary>
        //public string ignoremphone
        //{
        //    get { return _ignoremphone; }
        //    set { _ignoremphone = value; }
        //}
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 密码确认
        /// </summary>
        public string Password1 { get; set; }
        /// <summary>
        /// 图片验证码
        /// </summary>
        public string Vercode { get; set; }
        /// <summary>
        /// 手机校验码
        /// </summary>
        public string Smscode { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string Identitycard { get; set; }
        /// <summary>
        ///  证件类型，SFZ-居民身份证;SFZ_GAT-港澳台身份证;SFZ_HQ-华侨身份证(无身份证者可填护照号);QIT-其他
        /// </summary>
        public string Credentialtype { get; set; }
        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 密保问题1
        /// </summary>
        public string Pwdreq1 { get; set; }
        /// <summary>
        /// 密保问题1——答案
        /// </summary>
        public string Pwdanswer1 { get; set; }
        /// <summary>
        /// 密保问题2
        /// </summary>
        public string Pwdreq2 { get; set; }
        /// <summary>
        /// 密保问题2——答案
        /// </summary>
        public string Pwdanswer2 { get; set; }
        /// <summary>
        /// 密保问题3
        /// </summary>
        public string Pwdreq3 { get; set; }
        /// <summary>
        /// 密保问题3——答案
        /// </summary>
        public string Pwdanswer3 { get; set; }
    }
}
