﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.Framework.Server.Dto
{
    public class BaseReq
    {
        /// <summary>
        /// 会话令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string BusType { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public string BusId { get; set; }
    }
}
