using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Dto
{
    public class VerCodeRes : BaseRes
    {
        private string _VerCodeBase64 = "none";

        public string VerCodeBase64
        {
            get
            {
                return _VerCodeBase64;
            }
            set
            {
                this._VerCodeBase64 = value;
            }
        }
        /// <summary>
        /// 验证码路径
        /// </summary>
        private string _VerCodeUrl;

        public string VerCodeUrl
        {
            get { return _VerCodeUrl; }
            set { _VerCodeUrl = value; }
        }
        /// <summary>
        /// 验证码刷新路径
        /// </summary>
        private string _VerCodeRefreshUrl;

        public string VerCodeRefreshUrl
        {
            get { return _VerCodeRefreshUrl; }
            set { _VerCodeRefreshUrl = value; }
        }
    }
}
