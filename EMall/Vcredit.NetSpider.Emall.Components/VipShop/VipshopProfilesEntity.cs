using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Vipshop
{
	[Alias("vipshop_profiles")]
	public class VipshopProfilesEntity
	{
		public VipshopProfilesEntity() { }

		#region Attributes
      
		private long id  ;
        /// <summary>
        /// 
        /// </summary>
        [AutoIncrement]
 		public long Id
		{
			get { return id; }
			set { id = value; }
		}
      
		private long userId  ;
        /// <summary>
        /// 
        /// </summary>
		public long UserId
		{
			get { return userId; }
			set { userId = value; }
		}
      
		private bool isMarry  ;
        /// <summary>
        /// 
        /// </summary>
		public bool IsMarry
		{
			get { return isMarry; }
			set { isMarry = value; }
		}
      
		private string vocation  ;
        /// <summary>
        /// 
        /// </summary>
		public string Vocation
		{
			get { return vocation; }
			set { vocation = value; }
		}
      
		private string education  ;
        /// <summary>
        /// 
        /// </summary>
		public string Education
		{
			get { return education; }
			set { education = value; }
		}
      
		private string income  ;
        /// <summary>
        /// 
        /// </summary>
		public string Income
		{
			get { return income; }
			set { income = value; }
		}
      
		private string consumeGravity  ;
        /// <summary>
        /// 
        /// </summary>
		public string ConsumeGravity
		{
			get { return consumeGravity; }
			set { consumeGravity = value; }
		}
      
		private string shippingStandard  ;
        /// <summary>
        /// 
        /// </summary>
		public string ShippingStandard
		{
			get { return shippingStandard; }
			set { shippingStandard = value; }
		}
      
		private string favPromotion  ;
        /// <summary>
        /// 
        /// </summary>
		public string FavPromotion
		{
			get { return favPromotion; }
			set { favPromotion = value; }
		}
      
		private string favInform  ;
        /// <summary>
        /// 
        /// </summary>
		public string FavInform
		{
			get { return favInform; }
			set { favInform = value; }
		}
      
		private string favCategory  ;
        /// <summary>
        /// 
        /// </summary>
		public string FavCategory
		{
			get { return favCategory; }
			set { favCategory = value; }
		}
      
		private string shippingDoubt  ;
        /// <summary>
        /// 
        /// </summary>
		public string ShippingDoubt
		{
			get { return shippingDoubt; }
			set { shippingDoubt = value; }
		}
      
		private string monthlyConsume  ;
        /// <summary>
        /// 
        /// </summary>
		public string MonthlyConsume
		{
			get { return monthlyConsume; }
			set { monthlyConsume = value; }
		}
      
		private string surfTime  ;
        /// <summary>
        /// 
        /// </summary>
		public string SurfTime
		{
			get { return surfTime; }
			set { surfTime = value; }
		}
      
		private DateTime? createTime = DateTime.Now ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? CreateTime
		{
			get { return createTime; }
			set { createTime = value; }
		}
      
		private DateTime? updateTime  ;
        /// <summary>
        /// 
        /// </summary>
		public DateTime? UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}
		#endregion
	}
}
