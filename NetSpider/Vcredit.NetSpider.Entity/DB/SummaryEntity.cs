using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region SummaryEntity

    /// <summary>
    /// SummaryEntity object for NHibernate mapped table 'Summary'.
    /// </summary>
    public class SummaryEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //public virtual int Bid{get; set;}
        /// <summary>
        /// 
        /// </summary>
        public virtual int? Oper_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Mobile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? IsRealNameAuth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? OneMonthCallRecordCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? OneMonthCallRecordAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? ThreeMonthCallRecordCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? ThreeMonthCallRecordAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int? SixMonthCallRecordCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual decimal? SixMonthCallRecordAmount { get; set; }

        /// <summary>
        /// 日均被叫通话次数
        /// </summary>
        public virtual decimal? CALL_CNT_AVG_BJ { get; set; }

        /// <summary>
        /// 最大套餐金额
        /// </summary>
        public virtual decimal? MAX_PLAN_AMT { get; set; }
        /// <summary>
        /// 被叫联系人个数占比
        /// </summary>
        public virtual decimal? PHONE_NBR_BJ_RATE { get; set; }

        /// <summary>
        /// 夜间通话总时长占比
        /// </summary>
        public virtual decimal? CALL_LEN_NIT_RATE { get; set; }

        /// <summary>
        /// 日均主叫手机话费
        /// </summary>
        public virtual decimal? ZJSJ_PHN_CRG_AVG { get; set; }

        /// <summary>
        /// 90天内主叫次数
        /// </summary>
        public virtual int? DAY90_CALLING_TIMES { get; set; }

        /// <summary>
        /// 被叫联系人个数
        /// </summary>
        public virtual int? CALLED_PHONE_CNT { get; set; }

        /// <summary>
        /// 本地通话时长汇总
        /// </summary>
        public virtual decimal? LOCAL_CALL_TIME { get; set; }

        /// <summary>
        /// 近180天内累计主叫套餐外话费
        /// </summary>
        public virtual decimal? DAY180_CALLING_SUBTTL { get; set; }

        /// <summary>
        /// 近90天内累计通话时长
        /// </summary>
        public virtual decimal? DAY90_CALL_TTL_TIME { get; set; }

        /// <summary>
        /// 近90天内通话次数
        /// </summary>
        public virtual int? DAY90_CALL_TIMES { get; set; }

        /// <summary>
        /// 近90天内主叫通话时长
        /// </summary>
        public virtual decimal? DAY90_CALLING_TTL_TIME { get; set; }

        /// <summary>
        /// 近六月内累计上网流量
        /// </summary>
        public virtual decimal? NET_LSTM6_ONL_FLOW { get; set; }

        /// <summary>
        /// 累计白天主叫时长(9:00-18:00)
        /// </summary>
        public virtual decimal? DAY_CALLING_TTL_TIME { get; set; }

        /// <summary>
        /// 累计被叫次数
        /// </summary>
        public virtual int? CALLED_TIMES { get; set; }

        /// <summary>
        /// 累计被叫时长
        /// </summary>
        public virtual decimal? CALLED_TTL_TIME { get; set; }

        /// <summary>
        /// 累计上午被叫次数(9:00-13:00)
        /// </summary>
        public virtual int? MRNG_CALLED_TIMES { get; set; }

        /// <summary>
        /// 累计通话时长
        /// </summary>
        public virtual decimal? CALL_TTL_TIME { get; set; }

        /// <summary>
        /// 累计晚间被叫时长(18:00-23:00)
        /// </summary>
        public virtual decimal? NIGHT_CALLED_TTL_TIME { get; set; }

        /// <summary>
        /// 累计下午通话时长(13:00-18:00)
        /// </summary>
        public virtual decimal? AFTN_CALL_TTL_TIME { get; set; }

        /// <summary>
        /// 累计下午主叫时长(13:00-18:00)
        /// </summary>
        public virtual decimal? AFTN_CALLING_TTL_TIME { get; set; }

        /// <summary>
        /// 累计夜晚通话时长(23:00-9:00)
        /// </summary>
        public virtual decimal? NIGHT_CALL_TTL_TIME { get; set; }

        /// <summary>
        /// 累计夜晚主叫时长(23:00-9:00)
        /// </summary>
        public virtual decimal? NIGHT_CALLING_TTL_TIME { get; set; }

        /// <summary>
        /// 累计主叫时长
        /// </summary>
        public virtual decimal? CALLING_TTL_TIME { get; set; }

        /// <summary>
        /// 手机开卡时长至申请时间月数间隔
        /// </summary>
        public virtual decimal? PH_USE_MONS { get; set; }

        /// <summary>
        /// 所有联系人个数
        /// </summary>
        public virtual int? CALL_PHONE_CNT { get; set; }

        /// <summary>
        /// 通话天数
        /// </summary>
        public virtual int? CTT_DAYS_CNT { get; set; }

        /// <summary>
        /// 有被叫的天数
        /// </summary>
        public virtual int? CALLED_CTT_DAYS_CNT { get; set; }

        /// <summary>
        /// 有主叫的通话天数
        /// </summary>
        public virtual int? CALLING_CTT_DAYS_CNT { get; set; }

        /// <summary>
        /// 30天内危险号码被叫次数
        /// </summary>
        public virtual int CALLED_TIMES_IN30DAY { get; set; }

        /// <summary>
        /// 15内灰色号码被叫次数
        /// </summary>
        public virtual int CALLED_TIMES_IN15DAY { get; set; }

        /// <summary>
        /// 30天内灰色号码被叫次数
        /// </summary>
        public virtual int CALLED_TIMES_IN30DAY_Gray { get; set; }

        /// <summary>
        /// 15内危险号码被叫次数
        /// </summary>
        public virtual int CALLED_TIMES_IN15DAY_Gray { get; set; }

        /// <summary>
        /// 同业催收
        /// </summary>
        public virtual int? CallTimes { get; set; }


        private string _CreateTime = DateTime.Now.ToString();
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual string CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        public virtual OperationLogEntity OperationLog { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public virtual string Regdate { get; set; }
    }
    #endregion
}