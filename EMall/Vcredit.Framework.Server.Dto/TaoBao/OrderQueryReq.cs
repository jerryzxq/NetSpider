using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto.TaoBao
{
    public class OrderQueryReq : BaseReq
    {
        private int _pageindex = 1;
        public int PageIndex
        {
            get { return this._pageindex; }
            set { this._pageindex = value; }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get { return this._pageSize; }
            set { this._pageSize = value; }
        }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

    }
}
