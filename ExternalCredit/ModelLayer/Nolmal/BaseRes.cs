﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExtTrade.ModelLayer.Nolmal
{
    public class BaseRes
    {
        private int _StatusCode = 1;
        public int StatusCode
        {
            get { return _StatusCode; }
            set { this._StatusCode = value; }
        }
        public string StatusDescription { get; set; }

        public string Result { get; set; }
        public string Token { get; set; }

        /// <summary>
        /// 下一步
        /// </summary>
        public string nextProCode { get; set; }

        private string _StartTime = DateTime.Now.ToString();
        public string StartTime
        {
            get { return _StartTime; }
            set { this._StartTime = value; }
        }
        private string _EndTime = DateTime.Now.ToString();
        public string EndTime
        {
            get { return _StartTime; }
            set { this._StartTime = value; }
        }
        /// <summary>
        /// 采集网站
        /// </summary>
        public string Website { get; set; }
    }
}