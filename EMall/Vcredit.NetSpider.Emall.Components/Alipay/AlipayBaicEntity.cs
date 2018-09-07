using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_Baic")]
  
    public class AlipayBaicEntity
    {

        public AlipayBaicEntity() { }


        #region Attributes

        private long iD;

        [AutoIncrement]
        /// <summary>
        /// 编号
        /// </summary>
        public long ID
        {
            get { return iD; }
            set { iD = value; }
        }

        private string accountName;

        /// <summary>
        /// 账户名
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        private string realName;


        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName
        {
            get { return realName; }
            set { realName = value; }
        }

        private string isRealName;


        /// <summary>
        /// 实名认证
        /// </summary>
        public string IsRealName
        {
            get { return isRealName; }
            set { isRealName = value; }
        }

        private string documentType;


        /// <summary>
        /// 证件类型
        /// </summary>
        public string DocumentType
        {
            get { return documentType; }
            set { documentType = value; }
        }

        private string documentNo;


        /// <summary>
        /// 证件号
        /// </summary>
        public string DocumentNo
        {
            get { return documentNo; }
            set { documentNo = value; }
        }

        private string documentTime;


        /// <summary>
        /// 证件有效期
        /// </summary>
        public string DocumentTime
        {
            get { return documentTime; }
            set { documentTime = value; }
        }

        private string validityTime;


        /// <summary>
        /// 证件有效期
        /// </summary>
        public string ValidityTime
        {
            get { return validityTime; }
            set { validityTime = value; }
        }

        private string email;


        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        private string mobile;


        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile
        {
            get { return mobile; }
            set { mobile = value; }
        }

        private string taoBao;


        /// <summary>
        /// 淘宝会员名
        /// </summary>
        public string TaoBao
        {
            get { return taoBao; }
            set { taoBao = value; }
        }

        private DateTime? regTime;


        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegTime
        {
            get { return regTime; }
            set { regTime = value; }
        }

        private int? bankCount;


        /// <summary>
        /// 银行卡绑定数
        /// </summary>
        public int? BankCount
        {
            get { return bankCount; }
            set { bankCount = value; }
        }

        private int? addressCount;


        /// <summary>
        /// 收货地址绑定数
        /// </summary>
        public int? AddressCount
        {
            get { return addressCount; }
            set { addressCount = value; }
        }

        private string memberGuarantee;


        /// <summary>
        /// 会员保障
        /// </summary>
        public string MemberGuarantee
        {
            get { return memberGuarantee; }
            set { memberGuarantee = value; }
        }

        private decimal? accountBalance;


        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal? AccountBalance
        {
            get { return accountBalance; }
            set { accountBalance = value; }
        }

        private decimal? yuEbao;


        /// <summary>
        /// 余额宝
        /// </summary>
        public decimal? YuEbao
        {
            get { return yuEbao; }
            set { yuEbao = value; }
        }

        private decimal? yuEbaoProfit;


        /// <summary>
        /// 余额宝历史累计收益
        /// </summary>
        public decimal? YuEbaoProfit
        {
            get { return yuEbaoProfit; }
            set { yuEbaoProfit = value; }
        }

        private decimal? jiFenBao;


        /// <summary>
        /// 集分宝
        /// </summary>
        public decimal? JiFenBao
        {
            get { return jiFenBao; }
            set { jiFenBao = value; }
        }

        private decimal? zhaoCaiBao;
        /// <summary>
        /// 招财宝
        /// </summary>
        public decimal? ZhaoCaiBao
        {
            get { return zhaoCaiBao; }
            set { zhaoCaiBao = value; }
        }

        private decimal? cunJinBao;
        /// <summary>
        /// 存金宝
        /// </summary>
        public decimal? CunJinBao
        {
            get { return cunJinBao; }
            set { cunJinBao = value; }
        }
        private decimal? jiJin;
        /// <summary>
        /// 基金
        /// </summary>
        public decimal? JiJin
        {
            get { return jiJin; }
            set { jiJin = value; }
        }
        private decimal? taoLiCai;
        /// <summary>
        /// 淘宝理财
        /// </summary>
        public decimal? TaoLiCai
        {
            get { return taoLiCai; }
            set { taoLiCai = value; }
        }



        private decimal? flowersBalance;


        /// <summary>
        /// 蚂蚁花呗消费额度
        /// </summary>
        public decimal? FlowersBalance
        {
            get { return flowersBalance; }
            set { flowersBalance = value; }
        }

        private decimal? flowerAvailable;


        /// <summary>
        /// 蚂蚁花呗可用额度
        /// </summary>
        public decimal? FlowerAvailable
        {
            get { return flowerAvailable; }
            set { flowerAvailable = value; }
        }

        private DateTime? createTime;


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime
        {
            get
            {

                if (createTime == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return createTime;
                }
            }
            set { createTime = value; }
        }

        private string busIdentityCard;


        /// <summary>
        /// 业务身份证号
        /// </summary>
        public string BusIdentityCard
        {
            get { return busIdentityCard; }
            set { busIdentityCard = value; }
        }

        private string busName;


        /// <summary>
        /// 业务姓名
        /// </summary>
        public string BusName
        {
            get { return busName; }
            set { busName = value; }
        }

        private string busId;


        /// <summary>
        /// 业务编号
        /// </summary>
        public string BusId
        {
            get { return busId; }
            set { busId = value; }
        }

        private string busType;


        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusType
        {
            get { return busType; }
            set { busType = value; }
        }

        private string token;


        /// <summary>
        /// 标识
        /// </summary>
        public string Token
        {
            get { return token; }
            set { token = value; }
        }
        private string userid;

        public string UserID
        {
            get { return userid; }
            set { userid = value; }
        }

        private List<AlipayAddressEntity> _Address = new List<AlipayAddressEntity>();
        [Ignore]
        public List<AlipayAddressEntity> Address
        {
            get { return _Address; }
            set { _Address = value; }

        }
        private List<AlipayBankEntity> _Bank = new List<AlipayBankEntity>();
        [Ignore]
        public List<AlipayBankEntity> Bank
        {
            get { return _Bank; }
            set { _Bank = value; }

        }

        private List<AlipayBillEntity> _Bill = new List<AlipayBillEntity>();
        [Ignore]
        public List<AlipayBillEntity> Bill
        {
            get { return _Bill; }
            set { _Bill = value; }

        }
        private List<AlipayElectronicBillEntity> _ElectronicBill = new List<AlipayElectronicBillEntity>();
        [Ignore]
        public List<AlipayElectronicBillEntity> ElectronicBill
        {
            get { return _ElectronicBill; }
            set { _ElectronicBill = value; }

        }

        private List<AlipayBillFlowerEntity> _FlowerBill = new List<AlipayBillFlowerEntity>();
        [Ignore]
        public List<AlipayBillFlowerEntity> FlowerBill
        {
            get { return _FlowerBill; }
            set { _FlowerBill = value; }

        }

        private List<AlipayBillFlowerOrderDetailEntity> _FlowerBillDetail = new List<AlipayBillFlowerOrderDetailEntity>();
        [Ignore]
        public List<AlipayBillFlowerOrderDetailEntity> FlowerBillDetail
        {
            get { return _FlowerBillDetail; }
            set { _FlowerBillDetail = value; }

        }
        #endregion

    }
}
