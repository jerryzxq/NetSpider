﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // ProvidentFundDetailEntity数据访问对象
    internal class ProvidentFundDetailDao : BaseDao<ProvidentFundDetailEntity>, IProvidentFundDetailDao 
    {
        public IDaoHelper<ProvidentFundDetailEntity> DaoHelper { get; set; }
    }
}
