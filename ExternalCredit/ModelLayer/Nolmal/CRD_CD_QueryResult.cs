using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_QueryResult")]
    [Schema("credit")]
    public partial class CRD_CD_QueryResultEntity 
    {
        [Ignore]
        public decimal? ID { get; set; }
        public string FileName { get; set; }
        public int? QueryNum { get; set; }
        public int? SuccessNum { get; set; }
        public int? FailNum { get; set; }
        public string FailReason { get; set; }
 
  
        [Ignore]
        public DateTime? Time_Stamp { get; set; }
    }
}
