using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class CRD_PI_CIVILJDGMEntity
    {
        public virtual int Id { get; set; }
        public virtual int ReportId { get; set; }
        public virtual string Court { get; set; }
        public virtual string Case_No { get; set; }
        
        public virtual string Case_Reason { get; set; }
        public virtual DateTime? Register_Date { get; set; }
        public virtual string Closed_Type { get; set; }
        public virtual string Case_Result { get; set; }
        public virtual DateTime? Case_Validate_Date { get; set; }
        public virtual string Suit_Object { get; set; }
        public virtual decimal? Suit_Object_Money { get; set; }
    }
}
