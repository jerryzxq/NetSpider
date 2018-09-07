using System;
using System.Collections.Generic;
using Vcredit.Framework.Server.Entity;

namespace Vcredit.NetSpider.Entity.DB
{
	#region Variable_mobile_summaryEntity

	/// <summary>
	/// Variable_mobile_summaryEntity object for NHibernate mapped table 'variable_mobile_summary'.
	/// </summary>
	public class Variable_mobile_summaryEntity:BaseEntity
	{
		public virtual long Id{get; set; }

		 /// <summary>
		 /// 手机号码
		 /// </summary>
		public virtual string Mobile{get; set;}
		 /// <summary>
		 /// 业务姓名
		 /// </summary>
		public virtual string BusName{get; set;}
		 /// <summary>
		 /// 业务证件号
		 /// </summary>
		public virtual string BusIdentityCard{get; set;}
		 /// <summary>
		 /// 姓名
		 /// </summary>
		public virtual string Name{get; set;}
		 /// <summary>
		 /// 证件号
		 /// </summary>
		public virtual string IdentityCard{get; set;}
		 /// <summary>
		 /// 注册时间
		 /// </summary>
		public virtual string Regdate{get; set;}
		 /// <summary>
		 /// 城市
		 /// </summary>
		public virtual string City{get; set;}
		 /// <summary>
		 /// 是否实名认证
		 /// </summary>
		public virtual int ? IsRealNameAuth{get; set;}
		 /// <summary>
		 /// 业务类型
		 /// </summary>
		public virtual string BusType{get; set;}
		 /// <summary>
		 /// 业务id
		 /// </summary>
		public virtual string BusId{get; set;}
		 /// <summary>
		 /// 来源ID
		 /// </summary>
		public virtual int ? SourceId{get; set;}
		 /// <summary>
		 /// 来源类型
		 /// </summary>
		public virtual string SourceType{get; set;}
		 /// <summary>
		 /// 近一个月通话次数
		 /// </summary>
		public virtual int ? One_Month_Call_Record_Count{get; set;}
		 /// <summary>
		 /// 近一个月通话金额
		 /// </summary>
		public virtual decimal ? One_Month_Call_Record_Amount{get; set;}
		 /// <summary>
		 /// 近三个月通话次数
		 /// </summary>
		public virtual int ? Three_Month_Call_Record_Count{get; set;}
		 /// <summary>
		 /// 近三个月通话金额
		 /// </summary>
		public virtual decimal ? Three_Month_Call_Record_Amount{get; set;}
		 /// <summary>
		 /// 近6个月通话次数
		 /// </summary>
		public virtual int ? Six_Month_Call_Record_Count{get; set;}
		 /// <summary>
		 /// 近6个月通话金额
		 /// </summary>
		public virtual decimal ? Six_Month_Call_Record_Amount{get; set;}
		 /// <summary>
		 /// 日均被叫通话次数
		 /// </summary>
		public virtual decimal ? CALL_CNT_AVG_BJ{get; set;}
		 /// <summary>
		 /// 最大套餐金额
		 /// </summary>
		public virtual decimal ? MAX_PLAN_AMT{get; set;}
		 /// <summary>
		 /// 被叫联系人个数占比
		 /// </summary>
		public virtual decimal ? PHONE_NBR_BJ_RATE{get; set;}
		 /// <summary>
		 /// 夜间通话总时长占比
		 /// </summary>
		public virtual decimal ? CALL_LEN_NIT_RATE{get; set;}
		 /// <summary>
		 /// 日均主叫手机话费
		 /// </summary>
		public virtual decimal ? ZJSJ_PHN_CRG_AVG{get; set;}
		 /// <summary>
		 /// 90天内主叫次数
		 /// </summary>
		public virtual int ? DAY90_CALLING_TIMES{get; set;}
		 /// <summary>
		 /// 被叫联系人个数
		 /// </summary>
		public virtual int ? CALLED_PHONE_CNT{get; set;}
		 /// <summary>
		 /// 本地通话时长汇总
		 /// </summary>
		public virtual decimal ? LOCAL_CALL_TIME{get; set;}
		 /// <summary>
		 /// 近180天内累计主叫套餐外话费
		 /// </summary>
		public virtual decimal ? DAY180_CALLING_SUBTTL{get; set;}
		 /// <summary>
		 /// 近90天内累计通话时长
		 /// </summary>
		public virtual decimal ? DAY90_CALL_TTL_TIME{get; set;}
		 /// <summary>
		 /// 近90天内通话次数
		 /// </summary>
		public virtual int ? DAY90_CALL_TIMES{get; set;}
		 /// <summary>
		 /// 近90天内主叫通话时长
		 /// </summary>
		public virtual decimal ? DAY90_CALLING_TTL_TIME{get; set;}
		 /// <summary>
		 /// 近六月内累计上网流量
		 /// </summary>
		public virtual decimal ? NET_LSTM6_ONL_FLOW{get; set;}
		 /// <summary>
		 /// 累计白天主叫时长(9:00-18:00)
		 /// </summary>
		public virtual decimal ? DAY_CALLING_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计被叫次数
		 /// </summary>
		public virtual int ? CALLED_TIMES{get; set;}
		 /// <summary>
		 /// 累计被叫时长
		 /// </summary>
		public virtual decimal ? CALLED_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计上午被叫次数(9:00-13:00)
		 /// </summary>
		public virtual int ? MRNG_CALLED_TIMES{get; set;}
		 /// <summary>
		 /// 累计通话时长
		 /// </summary>
		public virtual decimal ? CALL_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计晚间被叫时长(18:00-23:00)
		 /// </summary>
		public virtual decimal ? NIGHT_CALLED_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计下午通话时长(13:00-18:00)
		 /// </summary>
		public virtual decimal ? AFTN_CALL_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计下午主叫时长(13:00-18:00)
		 /// </summary>
		public virtual decimal ? AFTN_CALLING_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计夜晚通话时长(23:00-9:00)
		 /// </summary>
		public virtual decimal ? NIGHT_CALL_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计夜晚主叫时长(23:00-9:00)
		 /// </summary>
		public virtual decimal ? NIGHT_CALLING_TTL_TIME{get; set;}
		 /// <summary>
		 /// 累计主叫时长
		 /// </summary>
		public virtual decimal ? CALLING_TTL_TIME{get; set;}
		 /// <summary>
		 /// 手机开卡时长至申请时间月数间隔
		 /// </summary>
		public virtual decimal ? PH_USE_MONS{get; set;}
		 /// <summary>
		 /// 所有联系人个数
		 /// </summary>
		public virtual int ? CALL_PHONE_CNT{get; set;}
		 /// <summary>
		 /// 通话天数
		 /// </summary>
		public virtual int ? CTT_DAYS_CNT{get; set;}
		 /// <summary>
		 /// 有被叫的天数
		 /// </summary>
		public virtual int ? CALLED_CTT_DAYS_CNT{get; set;}
		 /// <summary>
		 /// 有主叫的通话天数
		 /// </summary>
		public virtual int ? CALLING_CTT_DAYS_CNT{get; set;}
		 /// <summary>
		 /// 30天内危险号码被叫次数
		 /// </summary>
		public virtual int ? CALLED_TIMES_IN30DAY{get; set;}
		 /// <summary>
		 /// 15内危险号码被叫次数
		 /// </summary>
        public virtual int? CALLED_TIMES_IN15DAY { get; set; }
        /// <summary>
        /// 30天内灰色号码被叫次数
        /// </summary>
        public virtual int? CALLED_TIMES_IN30DAY_Gray { get; set; }
        /// <summary>
        /// 15内灰色号码被叫次数
        /// </summary>
        public virtual int? CALLED_TIMES_IN15DAY_Gray { get; set; }
        
        /// <summary>
        /// 其他时间发送短信联系人个数
        /// </summary>
        public virtual int? OTHER_SMS_PHONE_CNT { get; set; }

        /// <summary>
        /// 总短信联系人个数
        /// </summary>
        public virtual int? SMS_PHONE_CNT { get; set; }

        /// <summary>
        /// 漫游通话次数
        /// </summary>
        public virtual int? ROAMING_CALL_TIMES { get; set; }

        /// <summary>
        /// 通话总次数
        /// </summary>
        public virtual int? ALL_CALL_TIMES { get; set; }

        /// <summary>
        /// 下午通话次数
        /// </summary>
        public virtual int? AFTN_CALL_TIMES { get; set; }

        /// <summary>
        /// 主叫号码总个数
        /// </summary>
        public virtual int? CALLING_PHONE_CNT { get; set; }

        /// <summary>
        /// 连续超过3天未通话次数
        /// </summary>
        public virtual int? DAY3_CHECK_CALL_TIMES { get; set; }

        /// <summary>
        /// 主叫手机话费总额
        /// </summary>
        public virtual decimal? CALLING_SUBTTL { get; set; }

        /// <summary>
        /// 近180天内主叫次数
        /// </summary>
        public virtual int? CALL_LSTM6_CALL_CNT { get; set; }

        /// <summary>
        /// 近180天白天被叫次数(09:00:00-18:00:00)
        /// </summary>
        public virtual int? ANS_DAY_CNT { get; set; }

        /// <summary>
        ///  近180天内夜晚主叫次数(23:00-9:00)
        /// </summary>
        public virtual int? CALL_SMY_CALL_NIGHT_CNT { get; set; }

        /// <summary>
        /// 近90天内主叫话费
        /// </summary>
        public virtual decimal? CALL_LSTM3_SMY_CALL_MN { get; set; }

        /// <summary>
        /// 近180天被叫次数
        /// </summary>
        public virtual int? CALL_D180_CALLED_CNT { get; set; }

        /// <summary>
        /// 近180天内夜晚通话次数(23:00-9:00)
        /// </summary>
        public virtual int? CALL_SMY_CTT_NIGHT_CNT { get; set; }

        /// <summary>
        /// 近180天被叫时长
        /// </summary>
        public virtual decimal? CALL_D180_CALLED_TIME { get; set; }

        /// <summary>
        /// 近180天主叫时长
        /// </summary>
        public virtual decimal? CALL_D180_CALL_TIME { get; set; }

        /// <summary>
        /// 近180天晚间被叫时长(18:00-23:00)
        /// </summary>
        public virtual decimal? D180_NIGHT_CALLED_TIME { get; set; }

        /// <summary>
        /// 近180天内通话次数
        /// </summary>
        public virtual int? CALL_SMY_CTT_CNT_TOTAL { get; set; }


		 /// <summary>
		 /// 催收次数
		 /// </summary>
		public virtual int ? CallTimes{get; set;}
		 /// <summary>
		 /// 更新时间
		 /// </summary>
		public virtual DateTime ? UpdateTime{get; set;}
		 /// <summary>
		 /// 创建时间
		 /// </summary>
		public virtual DateTime ? CreateTime{get; set;}
	}
	#endregion
}