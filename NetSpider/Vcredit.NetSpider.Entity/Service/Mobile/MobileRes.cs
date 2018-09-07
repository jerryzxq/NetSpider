using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Mobile
{
    public class MobileRes : BaseRes
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string PhoneNum { get; set; }

        /// <summary>
        /// 套餐品牌
        /// </summary>
        public string PackageBrand { get; set; }

        /// <summary>
        /// 当前手机套餐
        /// </summary>
        public string PhonePackage { get; set; }

        /// <summary>
        /// 当前手机星级
        /// </summary>
        public string StarLevel { get; set; }

        /// <summary>
        /// 当前积分
        /// </summary>
        public string PhoneIntegral { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string Postcode { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public string Idtype { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        public string Idcard { get; set; }

        /// <summary>
        /// PUK码
        /// </summary>
        public string PUK { get; set; }

        /// <summary>
        /// 入网时间
        /// </summary>
        public string Regdate { get; set; }

        private List<MobileCall> _PhoneCallList = new List<MobileCall>();
        /// <summary>
        /// 通话详单集合
        /// </summary>
        public List<MobileCall> PhoneCallList
        {
            get { return _PhoneCallList; }
            set { _PhoneCallList = value; }
        }

        private List<MobileSMS> _PhoneSMSList = new List<MobileSMS>();
        /// <summary>
        /// 短信详单集合
        /// </summary>
        public List<MobileSMS> PhoneSMSList
        {
            get { return _PhoneSMSList; }
            set { _PhoneSMSList = value; }
        }

        private List<MobileGPRS> _PhoneGPRSList = new List<MobileGPRS>();
        /// <summary>
        /// GPRS详单集合
        /// </summary>
        public List<MobileGPRS> PhoneGPRSList
        {
            get { return _PhoneGPRSList; }
            set { _PhoneGPRSList = value; }
        }

    }
}
