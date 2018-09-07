﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Dao
{
    // ProvidentFundLoanEntity数据访问对象
    internal class ProvidentFundLoanDao : BaseDao<ProvidentFundLoanEntity>, IProvidentFundLoanDao
    {
        public IDaoHelper<ProvidentFundLoanEntity> DaoHelper { get; set; }
    }
}
