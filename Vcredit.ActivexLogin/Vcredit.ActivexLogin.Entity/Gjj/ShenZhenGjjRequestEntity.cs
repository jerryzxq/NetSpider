using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Entity
{
    public class ShenZhenGjjRequestEntity : BaseActivexRequestEntity
    {
        public ShenZhenGjjRequestEntity()
        {
            SiteType = Common.ProjectEnums.WebSiteType.ShenZhenGjj;
        }
        public string Account { get; set; }

    }
}
