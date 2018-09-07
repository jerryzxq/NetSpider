using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Vcredit.NetSpider.Entity.Mongo.Mobile
{
    [BsonIgnoreExtraElements]
    public class Basic : BaseMongoEntity
    {
        /// <summary>
        /// 会话令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 业务姓名
        /// </summary>
        public string BusName { get; set; }

        /// <summary>
        /// 业务证件号
        /// </summary>
        public string BusIdentityCard { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 套餐品牌
        /// </summary>
        public string PackageBrand { get; set; }

        /// <summary>
        /// 当前手机套餐
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// 当前手机星级
        /// </summary>
        public string StarLevel { get; set; }

        /// <summary>
        /// 当前积分
        /// </summary>
        public string Integral { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string Postcode { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public string Idtype { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        public string Idcard { get; set; }

        /// <summary>
        /// PUK码
        /// </summary>
        public string PUK { get; set; }

        /// <summary>
        /// 入网时间
        /// </summary>
        public string Regdate { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string UpdateTime { get; set; }


        private string _CreateTime = DateTime.Now.ToString();
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        private List<MonthBill> _BillList = new List<MonthBill>();
        /// <summary>
        /// 月消费情况集合
        /// </summary>
        public List<MonthBill> BillList
        {
            get { return _BillList; }
            set { _BillList = value; }
        }

        private List<Call> _CallList = new List<Call>();
        /// <summary>
        /// 通话详单集合
        /// </summary>
        public List<Call> CallList
        {
            get { return _CallList; }
            set { _CallList = value; }
        }

        private List<Sms> _SmsList = new List<Sms>();
        /// <summary>
        /// 短信详单集合
        /// </summary>
        public List<Sms> SmsList
        {
            get { return _SmsList; }
            set { _SmsList = value; }
        }

        private List<Net> _NetList = new List<Net>();
        /// <summary>
        /// 流量详单集合
        /// </summary>
        public List<Net> NetList
        {
            get { return _NetList; }
            set { _NetList = value; }
        }
    }
}
