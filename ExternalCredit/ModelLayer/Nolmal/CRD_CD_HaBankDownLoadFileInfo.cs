using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.ExtTrade.ModelLayer.Nolmal
{
    [Alias("CRD_CD_HaBankDownLoadFileInfo")]
    [Schema("credit")]
    public class CRD_CD_HaBankDownLoadFileInfoEntity
    {

        [AutoIncrement]
        public decimal? HaBankDownLoadFileInfo_Id { get; set; }

        public string Filepackagename { get; set; }

        public string DecKey { get; set; }

        public byte State { get; set; }
        [Ignore]
        public DateTime? Time_Stamp { get; set; }


    }
}
