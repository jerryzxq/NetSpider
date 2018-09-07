using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // SocialSecurityDetailEntity数据访问对象
    internal class SocialSecurityDetailDao : BaseDao<SocialSecurityDetailEntity>, ISocialSecurityDetailDao 
    {
        public IDaoHelper<SocialSecurityDetailEntity> DaoHelper { get; set; }
    }
}
