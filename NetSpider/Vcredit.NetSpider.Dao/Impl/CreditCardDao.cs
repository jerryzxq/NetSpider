using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CreditCardEntity数据访问对象
    internal class CreditCardDao : BaseDao<CreditCardEntity>, ICreditCardDao 
    {
        public IDaoHelper<CreditCardEntity> DaoHelper { get; set; }
    }
}
