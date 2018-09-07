using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // SocialSecurityEntity数据访问对象
    internal class SocialSecurityDao : BaseDao<SocialSecurityEntity>, ISocialSecurityDao 
    {
        public IDaoHelper<SocialSecurityEntity> DaoHelper { get; set; }
    }
}
