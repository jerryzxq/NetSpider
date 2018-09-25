using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Common;

namespace Vcredit.ActivexLogin.Entity
{
    public class TianJinGjjRequestEntity : BaseActivexRequestEntity
    {
        public TianJinGjjRequestEntity()
        {
            SiteType = ProjectEnums.WebSiteType.TianJinGjj;
        }

        public string Account { get; set; }

        public string Randnum { get; set; }
    }
}
