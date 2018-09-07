using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    public interface IMobile_Number : IBaseService<Mobile_NumberEntity>
    {
        Mobile_NumberEntity GetModel(string mobile);
    }
}
