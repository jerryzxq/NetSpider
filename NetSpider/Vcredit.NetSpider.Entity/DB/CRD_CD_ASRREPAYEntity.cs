using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class CRD_CD_ASRREPAYEntity
    {
        public virtual int Id { get; set; }
        public virtual int ReportId { get; set; }
        public virtual string Organ_Name { get; set; }
        public virtual DateTime? Latest_Assurer_Repay_Date { get; set; }
        public virtual decimal? Money { get; set; }
        public virtual DateTime? Latest_Repay_Date { get; set; }
        public virtual decimal? Balance { get; set; }
    }
}
