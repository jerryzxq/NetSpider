using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_OperatorLog")]
    [Schema("credit")]
    public partial class CRD_CD_OperatorLogEntity 
    {
        [Ignore]
        public decimal? ID { get; set; }
        public string BatchNo { get; set; }
        public  byte FileType { get; set; }
        public int? OperateTotalNum { get; set; }
        public int? SuccessNum { get; set; }
        public int? FailNum { get; set; }
        public string FailReason { get; set; }


        [Ignore]
        public DateTime? Time_Stamp { get; set; }
    }
}
