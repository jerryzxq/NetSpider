using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Dao
{
    // ProvidentFundLoanDetailEntity数据访问对象
    internal class ProvidentFundLoanDetailDao : BaseDao<ProvidentFundLoanDetailEntity>, IProvidentFundLoanDetailDao
    {
        public IDaoHelper<ProvidentFundLoanDetailEntity> DaoHelper { get; set; }
    }
}
