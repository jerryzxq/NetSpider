using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vcredit.ExtTrade.Services.Models
{
    internal class WMFailReason
    {
        public WMFailReasonState FailReason { get; set; }
    }
    internal enum WMFailReasonState
    {
        实体校验失败 = 101,
        不能重复查询 = 102,
        没有手写签名 = 103,
        申请次数超出 = 104,
        超出时间限制 = 105,
        数据保存失败 = 106,
        默认值 = 100,
        没有查询到数据 = 107,
        更新数据失败 = 108,
        已经存在数据 = 109,
        序列化出现问题 = 110,
        BusType没有权限 = 111,

    }
}