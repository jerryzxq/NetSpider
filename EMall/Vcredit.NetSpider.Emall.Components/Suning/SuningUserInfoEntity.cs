using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Suning_UserInfo")]
    //[Schema("dbo")]
	public class SuningUserInfoEntity
	{
	
		public SuningUserInfoEntity() { }


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
      
		private string snCustomerId;
       

        /// <summary>
        /// 苏宁客户编号
        /// </summary>
		public string SnCustomerId
		{
			get { return snCustomerId; }
			set { snCustomerId = value; }
		}
      
		private string token;
       

        /// <summary>
        /// TOKEN
        /// </summary>
		public string Token
		{
			get { return token; }
			set { token = value; }
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

        private string customeType;

        /// <summary>
        /// 客户类型
        /// </summary>
        public string CustomeType
        {
            get { return customeType; }
            set { customeType = value; }
        }

        private int currentLevelPoints;

        /// <summary>
        /// 当前积分
        /// </summary>
        public int CurrentLevelPoints
        {
            get { return currentLevelPoints; }
            set { currentLevelPoints  = value; }
        }
         
        //LevelDays
        private int levelDays;

        /// <summary>
        /// 当前积分
        /// </summary>
        public int LevelDays
        {
            get { return levelDays; }
            set { levelDays = value; }
        }

		private string memberGrade;
       

        /// <summary>
        /// 会员级别
        /// </summary>
        public string MemberGrade
		{
			get { return memberGrade; }
			set { memberGrade = value; }
		} 

      
		private int? cloudDrill;
       

        /// <summary>
        /// 云钻
        /// </summary>
		public int? CloudDrill
		{
			get { return cloudDrill; }
			set { cloudDrill = value; }
		}
       
		private decimal? creditAmount;
       

        /// <summary>
        /// 信用额度
        /// </summary>
		public decimal? CreditAmount
		{
			get { return creditAmount; }
			set { creditAmount = value; }
		}
      
		private string userName;
       

        /// <summary>
        /// 用户名
        /// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
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
      
		private string nickName;
       

        /// <summary>
        /// 昵称
        /// </summary>
		public string NickName
		{
			get { return nickName; }
			set { nickName = value; }
		}
      
		private string gender;
       

        /// <summary>
        /// 性别
        /// </summary>
		public string Gender
		{
			get { return gender; }
			set { gender = value; }
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
        /// 电话
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string birthday;
       

        /// <summary>
        /// 生日
        /// </summary>
        public string Birthday
		{
			get { return birthday; }
			set { birthday = value; }
		}
      
		private string constellation;
       

        /// <summary>
        /// 星座
        /// </summary>
		public string Constellation
		{
			get { return constellation; }
			set { constellation = value; }
		}
      
		private string liveAdress;
       

        /// <summary>
        /// 居住地
        /// </summary>
		public string LiveAdress
		{
			get { return liveAdress; }
			set { liveAdress = value; }
		}
      
		private string streetAdress;
       

        /// <summary>
        /// 街道
        /// </summary>
		public string StreetAdress
		{
			get { return streetAdress; }
			set { streetAdress = value; }
		}
      
		private string memberNo;
       

        /// <summary>
        /// 会员编号
        /// </summary>
        public string MemberNo
		{
			get { return memberNo; }
			set { memberNo = value; }
		}

        private string accountId;

        public string AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }


		private string provinceCode;
       

        /// <summary>
        /// 省份代码
        /// </summary>
		public string ProvinceCode
		{
			get { return provinceCode; }
			set { provinceCode = value; }
		} 

        private string cityCode;


        /// <summary>
        /// 城市代码
        /// </summary>
        public string CityCode
        {
            get { return cityCode; }
            set { cityCode = value; }
        }

        private string twonCode;


        /// <summary>
        /// 城镇编码
        /// </summary>
        public string TwonCode
        {
            get { return twonCode; }
            set { twonCode = value; }
        }

        private string streetCode;


        /// <summary>
        /// 街道编码
        /// </summary>
        public string StreetCode
        {
            get { return streetCode; }
            set { streetCode = value; }
        }
        private decimal? giftCart;

        /// <summary>
        /// 卡账户余额 
        /// </summary>
        public decimal? GiftCart
        {
            get { return giftCart; }
            set { giftCart = value; }
        }
        private int? accountPoint;

        /// <summary>
        /// 账户积分
        /// </summary>
        public int? AccountPoint
        {
            get { return accountPoint; }
            set { accountPoint = value; }
        }
        private int? cartQty;

        //AccountPoint
        /// <summary>
        /// 卡数量
        /// </summary>
		public int? CartQty
		{
			get { return cartQty; }
			set { cartQty = value; }
		}

        private string isRealName;


        /// <summary>
        /// 是否认证
        /// </summary>
        public string IsRealName
        {
            get { return isRealName; }
            set { isRealName = value; }
        }
        
		private DateTime? createTime = DateTime.Now;
       

        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
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

        private decimal? changeBao;


        /// <summary>
        /// 零钱宝
        /// </summary>
        public decimal? ChangeBao
        {
            get { return changeBao; }
            set { changeBao = value; }
        }

        private decimal? yiFbaoProfit;


        /// <summary>
        /// 易付宝累计收益
        /// </summary>
        public decimal? YiFbaoProfit
        {
            get { return yiFbaoProfit; }
            set { yiFbaoProfit = value; }
        }

        private decimal? renxingfuTotalAmount;
        /// <summary>
        /// 任性付总额度
        /// </summary>
        public decimal? RenxingfuTotalAmount
        {
            get { return renxingfuTotalAmount; }
            set { renxingfuTotalAmount = value; }
        }
        private decimal? renxingfuUsedAmount;
        /// <summary>
        /// 任性付已用额度
        /// </summary>
        public decimal? RenxingfuUsedAmount
        {
            get { return renxingfuUsedAmount; }
            set { renxingfuUsedAmount = value; }
        }
        private decimal? renxingfuAvailAmount;
        /// <summary>
        /// 任性付可用额度
        /// </summary>
        public decimal? RenxingfuAvailAmount
        {
            get { return renxingfuAvailAmount; }
            set { renxingfuAvailAmount = value; }
        }

        private List<SuningReceiveAddressEntity> _Address = new List<SuningReceiveAddressEntity>();
        [Ignore]
        public List<SuningReceiveAddressEntity> Address
        {
            get { return _Address; }
            set { _Address = value; }

        }

        private SuningCampusAccountEntity _Campus = new SuningCampusAccountEntity();
        [Ignore]
        public SuningCampusAccountEntity Campus
        {
            get { return _Campus; }
            set { _Campus = value; }

        }

        private List<SuningOrderEntity> _Order = new List<SuningOrderEntity>();
        [Ignore]
        public List<SuningOrderEntity> Order
        {
            get { return _Order; }
            set { _Order = value; }
        }

        private List<SuningCloudVipOrderEntity> _CloudVipOrder = new List<SuningCloudVipOrderEntity>();
        [Ignore]
        public List<SuningCloudVipOrderEntity> CloudVipOrder
        {
            get { return _CloudVipOrder; }
            set { _CloudVipOrder = value; }
        }

        private List<SuningCartEntity> _Cart = new List<SuningCartEntity>();

        [Ignore]
        public List<SuningCartEntity> Cart
        {
            get { return _Cart; }
            set { _Cart = value; }
        }

        private List<SuningProductCollectEntity> _ProductCollect = new List<SuningProductCollectEntity>();

        [Ignore]
        public List<SuningProductCollectEntity> ProductCollect
        {
            get { return _ProductCollect; }
            set { _ProductCollect = value; }
        }

        private List<SuningFootPrintEntity> _FootPrintList = new List<SuningFootPrintEntity>();

        [Ignore]
        public List<SuningFootPrintEntity> FootPrintList
        {
            get { return _FootPrintList; }
            set { _FootPrintList = value; }
        }
        private List<SuningReturnGoodsOrderEntity> _ReturnGoodsList = new List<SuningReturnGoodsOrderEntity>();
        [Ignore]
        public List<SuningReturnGoodsOrderEntity> ReturnGoodsList
        {
            get { return _ReturnGoodsList; }
            set { _ReturnGoodsList = value; }
        }
        private SuningEFuBaoBaicEntity _EFuBaoBaic = new SuningEFuBaoBaicEntity();
         [Ignore]
        public SuningEFuBaoBaicEntity EFuBaoBaic
        {
            get { return _EFuBaoBaic; }
            set { _EFuBaoBaic = value; }
        }

         private List<SuningEFuBaoTradeEntity> _EFuBaoTrade = new List<SuningEFuBaoTradeEntity>();

        [Ignore]
         public List<SuningEFuBaoTradeEntity> EFuBaoTrade
         {
             get { return _EFuBaoTrade; }
             set { _EFuBaoTrade = value; }
         }

        private List<SuningEFuBaoBankCardEntity> _EFuBaoBank = new List<SuningEFuBaoBankCardEntity>();

        [Ignore]
        public List<SuningEFuBaoBankCardEntity> EFuBaoBank
        {
            get { return _EFuBaoBank; }
            set { _EFuBaoBank = value; }
        }
        private List<SuningEFuBaoBillPayRecordEntity> _BillPayRecord = new List<SuningEFuBaoBillPayRecordEntity>();
        [Ignore]
        public List<SuningEFuBaoBillPayRecordEntity> BillPayRecord
        {
            get { return _BillPayRecord; }
            set { _BillPayRecord = value; }
        }
		#endregion

	}
}
