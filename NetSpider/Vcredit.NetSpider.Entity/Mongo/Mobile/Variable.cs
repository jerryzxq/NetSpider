using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    [BsonIgnoreExtraElements]
    public class Variable : BaseMongoEntity
    {
        public string Token { get; set; }
        public string Mobile { get; set; }
        public string IdentityCard { get; set; }
        public string BusIdentityCard { get; set; }

        public string Name { get; set; }
        /// <summary>
        /// 日均被叫通话次数,被叫总通话号码个数/被叫天数
        /// </summary>
        public decimal CallCntAvgBJ { get; set; }
        /// <summary>
        /// 收入水平,月收入/当地平均月收入
        /// </summary>
        public decimal IncmLevCD { get; set; }
        /// <summary>
        /// 最大套餐金额，账单中最大的套餐金额
        /// </summary>
        public decimal MaxPlanAmt { get; set; }
        /// <summary>
        /// 被叫联系人个数占比,被叫号码总个数/通话号码总个数
        /// </summary>
        public decimal PhoneNbrBjRate { get; set; }
        /// <summary>
        /// 身份证所属省份,根据身份证号码推断
        /// </summary>
        public string CustOrginPov { get; set; }
        /// <summary>
        /// 手机开卡至申请时间长
        /// </summary>
        public string Regdate { get; set; }

        /// <summary>
        /// 夜间通话总时长占比,夜间（0:00-6:00）通话总时长/通话总时长
        /// </summary>
        public decimal CallLenNitRate { get; set; }

        /// <summary>
        /// 性别(根据身份证号码推断)
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 日均主叫手机话费,（被叫方非固话）主叫手机话费/主叫天数
        /// </summary>
        public decimal ZjsjPhnCrgAvg { get; set; }

        /// <summary>
        /// 催收次数
        /// </summary>
        public int CallTime { get; set; }

        private string _CreateTime = DateTime.Now.ToString();
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

    }
}
