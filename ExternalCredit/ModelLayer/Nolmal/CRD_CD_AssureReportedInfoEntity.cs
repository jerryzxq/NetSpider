using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_AssureReportedInfo")]
    [Schema("credit")]
    public partial class CRD_CD_AssureReportedInfoEntity
    {
        [AutoIncrement]
        [PrimaryKey]
        public long Id { get; set; }

        /// <summary>
        /// 担保业务编号
        /// </summary>
        public string GuaranteeLetterCode { get; set; }

        /// <summary>
        /// 担保合同号码
        /// </summary>
        public string GuaranteeContractCode { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string WarranteeName { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        public string WarranteeCertNo { get; set; }

        /// <summary>
        /// 担保起始日期
        /// </summary>
        public DateTime GuaranteeStartDate { get; set; }

        /// <summary>
        /// 担保到期日期
        /// </summary>
        public DateTime GuaranteeStopDate { get; set; }

        /// <summary>
        /// 担保金额
        /// </summary>
        public decimal GuaranteeSum { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// 在保余额/剩余本金
        /// </summary>
        public decimal InkeepBalance { get; set; }

        private int _state;
        /// <summary>
        /// 状态值 (AssureReportState.Default=0; AssureReportState.UpLoadFail=1; AssureReportState.UpLoadSuccess=2， UpLoading=6)
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

        //private int _type = 0;
        ///// <summary>
        ///// 上传数据类型 0=担保上传，1=担保后上传
        ///// </summary>
        //public int Type
        //{
        //    get { return _type; }
        //    set { _type = value; }
        //}

        ///// <summary>
        ///// 代偿金额
        ///// </summary>
        //public decimal? BalanceTransferSum { get; set; }
        ///// <summary>
        ///// 代偿日期 
        ///// </summary>
        //public DateTime? BalanceTransferDate { get; set; }

        public string Ukey { get; set; }

        /// <summary>
        /// 金融机构（放款方）
        /// </summary>
        public String CreditorName { get; set; }

        /// <summary>
        /// 新增字段--主合同号码
        /// </summary>
        public string MainContractCode { get; set; }

        /// <summary>
        /// 新增字段--主合同编号
        /// </summary>
        public string MainCreditorCode { get; set; }
    }
}