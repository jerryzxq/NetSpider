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
    [Alias("CRD_CD_AssureFile")]
    [Schema("credit")]
    public partial class CRD_CD_AssureFileEntity
    {
        [AutoIncrement]
        [PrimaryKey]
        public long Id { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// FilePath
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// FileType
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// FileSize
        /// </summary>
        public int FileSize { get; set; }


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
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 文件对应数据类型（0=清贷、非清贷，1=代偿）
        /// </summary>
        public int DataType { get; set; }
    }
}