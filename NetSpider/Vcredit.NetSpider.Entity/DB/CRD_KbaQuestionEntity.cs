using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
	#region CRD_KbaQuestionEntity

	/// <summary>
	/// CRD_KbaQuestionEntity object for NHibernate mapped table 'CRD_KbaQuestion'.
	/// </summary>
	public class CRD_KbaQuestionEntity
	{
		public virtual int Id{get; set; }

		 /// <summary>
		 /// 
		 /// </summary>
		public virtual int ? AccountId{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Question{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Answer{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
        public virtual string Answerresult { get; set; }
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Derivativecode{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Businesstype{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Questionno{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Kbanum{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Options1{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Options2{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Options3{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Options4{get; set;}
		 /// <summary>
		 /// 
		 /// </summary>
		public virtual string Options5{get; set;}

        private DateTime _CreateTime = DateTime.Now;
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
	}
	#endregion
}