using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Vipshop
{
	[Alias("vipshop_size")]
	public class VipshopSizeEntity
	{
		public VipshopSizeEntity() { }

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
      
		private string name  ;
        /// <summary>
        /// 
        /// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
      
		private string gender  ;
        /// <summary>
        /// 
        /// </summary>
		public string Gender
		{
			get { return gender; }
			set { gender = value; }
		}
      
		private decimal? bust  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Bust
		{
			get { return bust; }
			set { bust = value; }
		}
      
		private decimal? waist  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Waist
		{
			get { return waist; }
			set { waist = value; }
		}
      
		private decimal? hip  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Hip
		{
			get { return hip; }
			set { hip = value; }
		}
      
		private decimal? height  ;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Height
		{
			get { return height; }
			set { height = value; }
		}

        private decimal? weight;
        /// <summary>
        /// 
        /// </summary>
		public decimal? Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        private bool? isdefault  ;
        /// <summary>
        /// 
        /// </summary>
		public bool? Isdefault
		{
			get { return isdefault; }
			set { isdefault = value; }
		}
      
		private string arm  ;
        /// <summary>
        /// 
        /// </summary>
		public string Arm
		{
			get { return arm; }
			set { arm = value; }
		}
      
		private string bustDown  ;
        /// <summary>
        /// 
        /// </summary>
		public string BustDown
		{
			get { return bustDown; }
			set { bustDown = value; }
		}
      
		private string bustUp  ;
        /// <summary>
        /// 
        /// </summary>
		public string BustUp
		{
			get { return bustUp; }
			set { bustUp = value; }
		}
      
		private string foot  ;
        /// <summary>
        /// 
        /// </summary>
		public string Foot
		{
			get { return foot; }
			set { foot = value; }
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
