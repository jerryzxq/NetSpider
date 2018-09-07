using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.Entity.Service
{
    public class VerCodeRes : BaseRes
    {
         private string _VerCodeBase64 = "none";
        public string VerCodeBase64
        {
            get{
               return _VerCodeBase64;
            }
            set {
                this._VerCodeBase64 = value;
            }
        }
        public string VerCode { get; set; }
        private string _VerCodeUrl = "none";
        public string VerCodeUrl
        {
            get
            {
                return _VerCodeUrl;
            }
            set
            {
                this._VerCodeUrl = value;
            }
        }

        public string VerCodeRelativeUrl
        {
            get
            {
                if (_VerCodeUrl != "none")
                {
                    string strStr = CommonFun.GetMidStr(_VerCodeUrl, "http://", "/");
                    return _VerCodeUrl.Replace("http://" + strStr, "");
                }
                else
                {
                    return string.Empty;
                }
            }
            set { }
        }
    }
}
