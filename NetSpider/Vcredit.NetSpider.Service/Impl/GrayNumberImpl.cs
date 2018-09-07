using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    internal class GrayNumberImpl : BaseService<GrayNumberEntity>, IGrayNumber
    {
        public IList<GrayNumberEntity> Get()
        {
            IList<GrayNumberEntity> entityList = new List<GrayNumberEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from Gray_Number WITH(NOLOCK) ").AddEntity("Gray_NumberEntity", typeof(GrayNumberEntity)).List<GrayNumberEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}
