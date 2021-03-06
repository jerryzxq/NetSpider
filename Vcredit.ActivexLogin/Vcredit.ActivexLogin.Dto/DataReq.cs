﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    public class DataReq
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


        private int _pageindex = 1;
        public int PageIndex
        {
            get { return this._pageindex; }
            set { this._pageindex = value; }
        }

        private int _pageSize = 30;
        public int PageSize
        {
            get { return this._pageSize; }
            set { this._pageSize = value; }
        }

        public string Sort { get; set; }
        public string Order { get; set; }
    }
}
