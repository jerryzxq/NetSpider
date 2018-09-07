using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Vcredit.ExternalCredit.CommonLayer
{
    public class SysEnums
    {
        /// <summary>
        /// 数据源类型
        /// </summary>
        public enum SourceType
        {
            /// <summary>
            /// 外贸征信
            /// </summary>
            [Description("外贸征信")]
            Trade = 10,

            /// <summary>
            /// 担保征信
            /// </summary>
            [Description("担保征信")]
            Assure = 11,

            /// <summary>
            /// 新外贸征信
            /// </summary>
            [Description("上海小贷")]
            ShangHai = 12
        }

        /// <summary>
        /// 担保上报状态
        /// </summary>
        public enum AssureReportState
        {
            /// <summary>
            /// 导入入库（需要确认数据无误）
            /// </summary>
            NeedCheck = -1,

            /// <summary>
            /// 默认状态
            /// </summary>
            Default = 0,

            /// <summary>
            /// 上传失败
            /// </summary>
            UpLoadFail = 1,

            /// <summary>
            /// 上传成功
            /// </summary>
            UpLoadSuccess = 2,

            /// <summary>
            /// 需要重新提交
            /// </summary>
            NeedReUpload = 3,

            /// <summary>
            /// 正在上传
            /// </summary>
            UpLoading = 6,

        }

        /// <summary>
        /// 文件上传状态
        /// </summary>
        public enum AssureFileState
        {
            解析失败 = -1,
            保存成功 = 0,
            解析成功 = 1,
            确认无误 = 2,
            数据已删除 = 3,
        }

        /// <summary>
        /// 担保提交失败原因
        /// </summary>
        public enum AssureFailReasonState
        {
            默认 = 100,
            实体校验失败 = 101,
            不能重复查询 = 102,
            没有手写签名 = 103,
            申请次数超出 = 104,
            超出时间限制 = 105,
            数据保存失败 = 106,
            BusType没有权限 = 111,
        }

        /// <summary>
        /// 接口状态
        /// </summary>
        public enum ServiceStatus
        {
            /// <summary>
            /// 操作异常
            /// </summary>
            [Description("操作异常")]
            Error = 110,
            /// <summary>
            /// 操作失败
            /// </summary>
            [Description("操作失败")]
            Fail = 1,
            /// <summary>
            /// 
            /// </summary>
            [Description("HTTP请求失败")]
            Httpfail = 500,
            /// <summary>
            /// 操作成功
            /// </summary>
            [Description("操作成功")]
            Success = 0,

        }
    }
}
