using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;


namespace  Vcredit.NetSpider.Service
{
    // CreditAccountEntity服务对象
    internal class CreditAccountImpl : BaseService<CreditAccountEntity>, ICreditAccount 
    {
        private ICreditAccountDao CreditAccountDao;
    }
}
	
