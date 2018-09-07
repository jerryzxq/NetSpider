using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Spd_LoginEntity服务对象接口
    public interface ISpd_Login : IBaseService<Spd_LoginEntity>
    {
        Spd_LoginEntity GetByIdentityCard(string IdentityCard,string SpiderType);
    }
}

