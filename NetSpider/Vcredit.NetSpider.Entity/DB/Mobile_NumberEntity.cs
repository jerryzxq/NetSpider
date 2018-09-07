using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.DB
{
    public class Mobile_NumberEntity
    {
        public virtual int ID { set; get; }
        public virtual string Mobile { set; get; }
        public virtual string Supplier { set; get; }
        public virtual string Province { set; get; }
        public virtual string City { set; get; }
        public virtual string Area_Code { set; get; }
        public virtual string Post_Code { set; get; }
        public virtual string Memo { set; get; }
    }
}
