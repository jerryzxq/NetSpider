using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class CRD_PI_FORCEEXCTNEntity
    {
        public virtual int Id { get; set; }
        public virtual int ReportId { get; set; }
        public virtual string Court { get; set; }
        public virtual string Case_No { get; set; }
        
        public virtual string Case_Reason { get; set; }
        public virtual DateTime? Register_Date { get; set; }
        public virtual string Closed_Type { get; set; }
        public virtual string Case_State { get; set; }
        public virtual DateTime? Closed_Date { get; set; }
        public virtual string Enforce_Object { get; set; }
        public virtual decimal? Enforce_Object_Money { get; set; }
        public virtual string Already_Enforce_Object { get; set; }
        public virtual decimal? Already_Enforce_Object_Money { get; set; }

    }
}
