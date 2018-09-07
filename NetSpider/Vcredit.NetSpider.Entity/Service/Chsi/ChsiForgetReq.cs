using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Chsi
{
    public class ChsiForgetReq : ChsiRegisterReq
    {
        /// <summary>
        ///  姓名
        /// </summary>
        public string Username { get; set; }
    }
}
