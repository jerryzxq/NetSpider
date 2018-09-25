using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Common;

namespace Vcredit.ActivexLogin.Entity
{
    public class GuangZhouGjjRequestEntity : BaseActivexRequestEntity
    {
        public GuangZhouGjjRequestEntity()
        {
            SiteType = ProjectEnums.WebSiteType.GuangZhouGjj;
        }

        public string Account;

        public string SRand;

        public string Name;

        private LoginType _loginType = LoginType.Account;
        public LoginType LoginType
        {
            get { return _loginType; }
            set { _loginType = value; }
        }
    }

    /// <summary>
    /// 枚举赋值是根据页面来的
    /// </summary>
    public enum LoginType
    {
        [Description("账号登录")]
        Account = 1,

        [Description("证件号登录")]
        CertNo = 2
    }
}
