using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Entity.Bank
{
    public class BocBankRequestEntity : BaseActivexRequestEntity
    {

        public BocBankRequestEntity()
        {
            this.SiteType = Common.ProjectEnums.WebSiteType.BocBank;
        }

        public string Account { get; set; }

        public string ConversationId;

        public string RandomKey_S { get; set; }

    }
}
