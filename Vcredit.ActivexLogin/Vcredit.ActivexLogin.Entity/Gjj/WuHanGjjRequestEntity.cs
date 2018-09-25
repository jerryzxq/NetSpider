using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Common;

namespace Vcredit.ActivexLogin.Entity
{
    public class WuHanGjjRequestEntity : BaseActivexRequestEntity
    {
        public WuHanGjjRequestEntity()
        {
            SiteType = ProjectEnums.WebSiteType.WuHanGjj;
        }

        public string Account { get; set; }

    }
}
