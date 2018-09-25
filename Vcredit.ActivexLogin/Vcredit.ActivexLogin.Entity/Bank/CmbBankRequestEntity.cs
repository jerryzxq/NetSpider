using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Entity.Bank
{
    public class CmbBankRequestEntity: BaseActivexRequestEntity
    {
        public CmbBankRequestEntity()
        {
            this.SiteType = Common.ProjectEnums.WebSiteType.CmbBank;
        }

        public string Account { get; set; }


        public string GenShell_ClientNo { get; set; }
    }
}
