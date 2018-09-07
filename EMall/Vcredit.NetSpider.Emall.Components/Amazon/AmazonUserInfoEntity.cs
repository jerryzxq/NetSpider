using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
	[Alias("amazon_userinfo")]
	public class AmazonUserInfoEntity
	{
		public AmazonUserInfoEntity() { }

		#region Attributes
      
		private long iD  ;
        /// <summary>
        /// 编号
        /// </summary>
        [AutoIncrement]
 		public long ID
		{
			get { return iD; }
			set { iD = value; }
		}
      
		private string customerId  ;
        /// <summary>
        /// 客户系统编号
        /// </summary>
		public string CustomerId
		{
			get { return customerId; }
			set { customerId = value; }
		}

        private string accountId;

        public string AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }
		private string token  ;
        /// <summary>
        /// 标志
        /// </summary>
		public string Token
		{
			get { return token; }
			set { token = value; }
		}
        private string accountName;

        /// <summary>
        /// 账号
        /// </summary>
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }
		private string customerName  ;
        /// <summary>
        /// 客户姓名
        /// </summary>
		public string CustomerName
		{
			get { return customerName; }
			set { customerName = value; }
		}
      
		private int? orderingGoods  ;
        /// <summary>
        /// 订购中的商品数量
        /// </summary>
		public int? OrderingGoods
		{
			get { return orderingGoods; }
			set { orderingGoods = value; }
		}
        private string nextDeliver;

        /// <summary>
        /// 下次配送
        /// </summary>
        public string NextDeliver
        {
            get { return nextDeliver; }
            set { nextDeliver = value; }
        }
		private decimal? giftCartAmount  ;
        /// <summary>
        /// 礼卡金额
        /// </summary>
		public decimal? GiftCartAmount
		{
			get { return giftCartAmount; }
			set { giftCartAmount = value; }
		}
      
		private string registerDate  ;
        /// <summary>
        /// 注册年份
        /// </summary>
		public string RegisterDate
		{
			get { return registerDate; }
			set { registerDate = value; }
		}
      
		private string publicName  ;
        /// <summary>
        /// 公开名称
        /// </summary>
		public string PublicName
		{
			get { return publicName; }
			set { publicName = value; }
		}
      
		private string resumeDoc  ;
        /// <summary>
        /// 简历
        /// </summary>
		public string ResumeDoc
		{
			get { return resumeDoc; }
			set { resumeDoc = value; }
		}
      
		private string occupation  ;
        /// <summary>
        /// 职业
        /// </summary>
		public string Occupation
		{
			get { return occupation; }
			set { occupation = value; }
		}
      
		private string email  ;
        /// <summary>
        /// 电子邮箱
        /// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}

        private string webSite;
        /// <summary>
        /// 分享站点
        /// </summary>
		public string WebSite
		{
			get { return webSite; }
            set { webSite = value; }
		}
        private string location;
        /// <summary>
        /// 地址
        /// </summary>
        public string Location
        {
            get { return location; }
            set { location = value; }
        }
		private string signature  ;
        /// <summary>
        /// 签名
        /// </summary>
		public string Signature
		{
			get { return signature; }
			set { signature = value; }
		}
      
		private string interest  ;
        /// <summary>
        /// 兴趣
        /// </summary>
		public string Interest
		{
			get { return interest; }
			set { interest = value; }
		}
      
		private string facebook  ;
        /// <summary>
        /// FaceBook地址
        /// </summary>
		public string Facebook
		{
			get { return facebook; }
			set { facebook = value; }
		}
      
		private string pinterest  ;
        /// <summary>
        /// Pinterest
        /// </summary>
		public string Pinterest
		{
			get { return pinterest; }
			set { pinterest = value; }
		}
      
		private string twitter  ;
        /// <summary>
        /// Twitter
        /// </summary>
		public string Twitter
		{
			get { return twitter; }
			set { twitter = value; }
		}
      
		private string instagram  ;
        /// <summary>
        /// Instagram
        /// </summary>
		public string Instagram
		{
			get { return instagram; }
			set { instagram = value; }
		}
      
		private string youtobe  ;
        /// <summary>
        /// Youtobe
        /// </summary>
		public string Youtobe
		{
			get { return youtobe; }
			set { youtobe = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 创建时间
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
		#endregion
	}
}
