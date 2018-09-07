using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.BusinessLayer
{
    public enum RequestState
    {
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
        /// 查询失败
        /// </summary>
        QueryFail = 3,

        /// <summary>
        /// 解析或入库失败
        /// </summary>
        AnalysisFail = 4,

        /// <summary>
        /// 成功获取征信信息
        /// </summary>
        SuccessCome = 5,

        /// <summary>
        /// 正在上传
        /// </summary>
        UpLoading = 6,
        /// <summary>
        /// 征信空白
        /// </summary>
        HaveNoData = 7,

        /// <summary>
        /// 连接失败
        /// </summary>
        ConnectionFailed = 8,
    }
}
