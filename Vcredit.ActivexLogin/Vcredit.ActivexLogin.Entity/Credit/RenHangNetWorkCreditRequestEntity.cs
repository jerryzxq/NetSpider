using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Entity.Credit
{
    public class RenHangNetWorkCreditRequestEntity : BaseActivexRequestEntity
    {
        public string Account;

        /// <summary>
        /// 密码生成随机参数
        /// </summary>
        public string RandomKey;
    }
}
