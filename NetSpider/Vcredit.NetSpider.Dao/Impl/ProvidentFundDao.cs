using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // ProvidentFundEntity数据访问对象
    internal class ProvidentFundDao : BaseDao<ProvidentFundEntity>, IProvidentFundDao 
    {
        public IDaoHelper<ProvidentFundEntity> DaoHelper { get; set; }
    }
}
