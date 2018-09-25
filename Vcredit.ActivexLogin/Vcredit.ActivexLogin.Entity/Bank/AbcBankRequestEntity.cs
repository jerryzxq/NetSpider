using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Entity.Bank
{
    public class AbcBankRequestEntity : BaseActivexRequestEntity
    {

        public AbcBankRequestEntity()
        {
            this.SiteType = Common.ProjectEnums.WebSiteType.AbcBank;
        }

        public string Account { get; set; }

        public string TimeStamp { get; set; }

        public string RandomText { get; set; }

        public string Abc_FormId { get; set; }

    }
}
