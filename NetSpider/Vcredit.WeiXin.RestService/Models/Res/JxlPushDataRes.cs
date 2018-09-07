using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.WeiXin.RestService.Models.Res
{
    public class JxlPushDataRes
    {
        private int _code = 1;
        public int code
        {
            get { return _code; }
            set { this._code = value; }
        }
        public string note { get; set; }

    }
}
