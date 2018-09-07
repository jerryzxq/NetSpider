using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Spd_LoginEntity服务对象
    internal class Spd_LoginImpl : BaseService<Spd_LoginEntity>, ISpd_Login
    {

        public Spd_LoginEntity GetByIdentityCard(string IdentityCard, string SpiderType)
        {
            var ls = base.FindListByHql("from Spd_LoginEntity where  IdentityCard=? and SpiderType=? and StatusCode=0 order by CreateTime desc", new object[] { IdentityCard, SpiderType }, 1, 1);

            return ls.FirstOrDefault();
        }
    }
}

