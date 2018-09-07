using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
    public class Error
    {
        public string  dealDesc { get; set; }
        public string  idNo { get; set; }
    }
    public class BatchResEntity
    {
        public string batchCount { get; set; }
        public string batchNo { get; set; }

        public List<Error>  errorList{ get; set; }
    }
}
