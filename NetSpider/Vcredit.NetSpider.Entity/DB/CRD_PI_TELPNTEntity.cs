using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class CRD_PI_TELPNTEntity
    {
        public virtual int Id { get; set; }
        public virtual int ReportId { get; set; }
        public virtual string Organ_Name { get; set; }
        public virtual string Type_Dw { get; set; }
        public virtual DateTime? Register_Date { get; set; }
        public virtual string State { get; set; }
        public virtual decimal? Arrear_Money { get; set; }
        public virtual string Arrear_Months { get; set; }
        public virtual string Status24 { get; set; }
        public virtual string Get_Time { get; set; }
    }
}
