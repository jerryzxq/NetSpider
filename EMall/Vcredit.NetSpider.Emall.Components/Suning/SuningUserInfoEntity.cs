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
        /// ���
        /// </summary>
		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string snCustomerId;
       

        /// <summary>
        /// �����ͻ����
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
        /// �˻���
        /// </summary>
		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

        private string customeType;

        /// <summary>
        /// �ͻ�����
        /// </summary>
        public string CustomeType
        {
            get { return customeType; }
            set { customeType = value; }
        }

        private int currentLevelPoints;

        /// <summary>
        /// ��ǰ����
        /// </summary>
        public int CurrentLevelPoints
        {
            get { return currentLevelPoints; }
            set { currentLevelPoints  = value; }
        }
         
        //LevelDays
        private int levelDays;

        /// <summary>
        /// ��ǰ����
        /// </summary>
        public int LevelDays
        {
            get { return levelDays; }
            set { levelDays = value; }
        }

		private string memberGrade;
       

        /// <summary>
        /// ��Ա����
        /// </summary>
        public string MemberGrade
		{
			get { return memberGrade; }
			set { memberGrade = value; }
		} 

      
		private int? cloudDrill;
       

        /// <summary>
        /// ����
        /// </summary>
		public int? CloudDrill
		{
			get { return cloudDrill; }
			set { cloudDrill = value; }
		}
       
		private decimal? creditAmount;
       

        /// <summary>
        /// ���ö��
        /// </summary>
		public decimal? CreditAmount
		{
			get { return creditAmount; }
			set { creditAmount = value; }
		}
      
		private string userName;
       

        /// <summary>
        /// �û���
        /// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}
      
		private string realName;
       

        /// <summary>
        /// ��ʵ����
        /// </summary>
		public string RealName
		{
			get { return realName; }
			set { realName = value; }
		}
      
		private string nickName;
       

        /// <summary>
        /// �ǳ�
        /// </summary>
		public string NickName
		{
			get { return nickName; }
			set { nickName = value; }
		}
      
		private string gender;
       

        /// <summary>
        /// �Ա�
        /// </summary>
		public string Gender
		{
			get { return gender; }
			set { gender = value; }
		}
      
		private string email;
       

        /// <summary>
        /// ����
        /// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}
      
		private string mobile;
       

        /// <summary>
        /// �绰
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string birthday;
       

        /// <summary>
        /// ����
        /// </summary>
        public string Birthday
		{
			get { return birthday; }
			set { birthday = value; }
		}
      
		private string constellation;
       

        /// <summary>
        /// ����
        /// </summary>
		public string Constellation
		{
			get { return constellation; }
			set { constellation = value; }
		}
      
		private string liveAdress;
       

        /// <summary>
        /// ��ס��
        /// </summary>
		public string LiveAdress
		{
			get { return liveAdress; }
			set { liveAdress = value; }
		}
      
		private string streetAdress;
       

        /// <summary>
        /// �ֵ�
        /// </summary>
		public string StreetAdress
		{
			get { return streetAdress; }
			set { streetAdress = value; }
		}
      
		private string memberNo;
       

        /// <summary>
        /// ��Ա���
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
        /// ʡ�ݴ���
        /// </summary>
		public string ProvinceCode
		{
			get { return provinceCode; }
			set { provinceCode = value; }
		} 

        private string cityCode;


        /// <summary>
        /// ���д���
        /// </summary>
        public string CityCode
        {
            get { return cityCode; }
            set { cityCode = value; }
        }

        private string twonCode;


        /// <summary>
        /// �������
        /// </summary>
        public string TwonCode
        {
            get { return twonCode; }
            set { twonCode = value; }
        }

        private string streetCode;


        /// <summary>
        /// �ֵ�����
        /// </summary>
        public string StreetCode
        {
            get { return streetCode; }
            set { streetCode = value; }
        }
        private decimal? giftCart;

        /// <summary>
        /// ���˻���� 
        /// </summary>
        public decimal? GiftCart
        {
            get { return giftCart; }
            set { giftCart = value; }
        }
        private int? accountPoint;

        /// <summary>
        /// �˻�����
        /// </summary>
        public int? AccountPoint
        {
            get { return accountPoint; }
            set { accountPoint = value; }
        }
        private int? cartQty;

        //AccountPoint
        /// <summary>
        /// ������
        /// </summary>
		public int? CartQty
		{
			get { return cartQty; }
			set { cartQty = value; }
		}

        private string isRealName;


        /// <summary>
        /// �Ƿ���֤
        /// </summary>
        public string IsRealName
        {
            get { return isRealName; }
            set { isRealName = value; }
        }
        
		private DateTime? createTime = DateTime.Now;
       

        /// <summary>
        /// ����ʱ��
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}

        private decimal? accountBalance;


        /// <summary>
        /// �˻����
        /// </summary>
        public decimal? AccountBalance
        {
            get { return accountBalance; }
            set { accountBalance = value; }
        }

        private decimal? changeBao;


        /// <summary>
        /// ��Ǯ��
        /// </summary>
        public decimal? ChangeBao
        {
            get { return changeBao; }
            set { changeBao = value; }
        }

        private decimal? yiFbaoProfit;


        /// <summary>
        /// �׸����ۼ�����
        /// </summary>
        public decimal? YiFbaoProfit
        {
            get { return yiFbaoProfit; }
            set { yiFbaoProfit = value; }
        }

        private decimal? renxingfuTotalAmount;
        /// <summary>
        /// ���Ը��ܶ��
        /// </summary>
        public decimal? RenxingfuTotalAmount
        {
            get { return renxingfuTotalAmount; }
            set { renxingfuTotalAmount = value; }
        }
        private decimal? renxingfuUsedAmount;
        /// <summary>
        /// ���Ը����ö��
        /// </summary>
        public decimal? RenxingfuUsedAmount
        {
            get { return renxingfuUsedAmount; }
            set { renxingfuUsedAmount = value; }
        }
        private decimal? renxingfuAvailAmount;
        /// <summary>
        /// ���Ը����ö��
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
