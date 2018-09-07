using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_FocusActivity")]
    
    public class FocusActivityEntity : BaseEntity
    {
        public string Name { get; set; }
        public DateTime?  BeginTime{ get; set; }

        public DateTime? EndTime { get; set; }
        public string State { get; set; }

    }
}
