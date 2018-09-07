using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class CRD_PI_TAXARREAREntity
    {
        public virtual int Id { get; set; }
        public virtual int ReportId { get; set; }
        public virtual string Organ_Name { get; set; }
        public virtual decimal? Tax_Arrea_Amount { get; set; }
        public virtual DateTime? Tax_Arrear_Date { get; set; }
    }
}
