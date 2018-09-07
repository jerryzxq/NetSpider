using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CreditAccountEntity数据访问对象
    internal class CreditAccountDao : BaseDao<CreditAccountEntity>, ICreditAccountDao 
    {
        public IDaoHelper<CreditAccountEntity> DaoHelper { get; set; }
    }
}
