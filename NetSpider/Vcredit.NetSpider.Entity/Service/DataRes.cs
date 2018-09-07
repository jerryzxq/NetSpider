using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Entity.Service
{
    public class DataRes<T>
    {
       public T Data { get; set; }
       public long Total { get; set; }
       public int Start { get; set; }
       public string Sort { get; set; }
       public string Order { get; set; }
       public int Size { get; set; }
    }
}
