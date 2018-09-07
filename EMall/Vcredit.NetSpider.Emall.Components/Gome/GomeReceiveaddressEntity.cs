using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity.Gome
{
	[Alias("gome_receiveaddress")]
	public class GomeReceiveaddressEntity
	{
		public GomeReceiveaddressEntity() { }

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
      
		private string receiver  ;
        /// <summary>
        /// 
        /// </summary>
		public string Receiver
		{
			get { return receiver; }
			set { receiver = value; }
		}
      
		private string address  ;
        /// <summary>
        /// 
        /// </summary>
		public string Address
		{
			get { return address; }
			set { address = value; }
		}
      
		private string mobile  ;
        /// <summary>
        /// 
        /// </summary>
		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; }
		}
      
		private string email  ;
        /// <summary>
        /// 
        /// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}
      
		private bool? isDefault  ;
        /// <summary>
        /// 
        /// </summary>
		public bool? IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
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
