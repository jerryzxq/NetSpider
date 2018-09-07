using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class CRD_PI_ADMINPNSHMEntity
    {
        public virtual int Id { get; set; }
        public virtual int ReportId { get; set; }
        public virtual string Organ_Name { get; set; }
        public virtual string Case_No { get; set; }
        public virtual string Content { get; set; }
        public virtual decimal? Money { get; set; }
        public virtual DateTime? Begin_Date { get; set; }
        public virtual DateTime? End_Date { get; set; }
        public virtual string Result_Dw { get; set; }
    }
}
