﻿using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using Newtonsoft.Json;
using Vcredit.ExternalCredit.CommonLayer.Utility;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_PI_ACCFUND")]
    [Schema("credit")]
    public partial class CRD_PI_ACCFUNDEntity : BaseEntity
    {


        /// <summary>
        /// 参缴地
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 参缴日期
        /// </summary>
        public string Register_Date { get; set; }
        /// <summary>
        /// 初缴月份
        /// </summary>
        public string First_Month { get; set; }
        /// <summary>
        /// 缴至月份
        /// </summary>
        public string To_Month { get; set; }
        /// <summary>
        /// 缴费状态
        /// </summary>
        public string State { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 月缴存额
        /// </summary>
        public decimal? Pay { get; set; }
        /// <summary>
        /// 个人缴存比例
        /// </summary>
        public string Own_Percent { get; set; }
        /// <summary>
        /// 单位缴存比例
        /// </summary>
        public string Com_Percent { get; set; }
        /// <summary>
        /// 缴费单位
        /// </summary>
        public string Organ_Name { get; set; }
        [JsonConverter(typeof(DesJsonConverter))]
        /// <summary>
        /// 信息更新日期
        /// </summary>
        public DateTime? Get_Time { get; set; }


    }
}