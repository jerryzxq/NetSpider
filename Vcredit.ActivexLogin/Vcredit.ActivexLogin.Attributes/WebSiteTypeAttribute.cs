using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vcredit.ActivexLogin.Common.ProjectEnums;

namespace Vcredit.ActivexLogin.Attributes
{
    /// <summary>
    /// 站点约束
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RestraintSiteAttribute : Attribute
    {
        private WebSiteType _targetWebSite;

        public WebSiteType TargetWebSite
        {
            get { return _targetWebSite; }
            set { _targetWebSite = value; }
        }

    }
}
