using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    /// <summary>
    /// 担保剩余本金维护实体
    /// </summary>
    [Alias("CRD_CD_AssureMaintainInfo")]
    [Schema("credit")]
    public partial class CRD_CD_AssureMaintainEntity
    {
        [AutoIncrement]
        [PrimaryKey]
        public long Id { get; set; }

        /// <summary>
        /// 担保业务编号
        /// </summary>
        public string GuaranteeLetterCode { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Warranteename { get; set; }

        /// <summary>
        /// 在保余额
        /// </summary>
        public int InkeepBalance { get; set; }

        /// <summary>
        /// 余额变化日期
        /// </summary>
        public DateTime Balancechangedate { get; set; }

        /// <summary>
        /// 是否清贷
        /// </summary>
        public bool IsEnd { get; set; }


        private int _state;
        /// <summary>
        /// 状态值 (RequestState.Default; RequestState.UpLoadFail; RequestState.UpLoadSuccess)
        /// </summary>
        public int State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// 状态说明
        /// </summary>
        public string StateDescription { get; set; }

        private DateTime _createtime = DateTime.Now;
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime Createtime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        
        /// <summary>
        /// 数据导入文件Id
        /// </summary>
        public long? FileId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public string Ukey { get; set; }
    }
}