using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // ProvidentFundDetailEntity服务对象
    internal class ProvidentFundDetailImpl : BaseService<ProvidentFundDetailEntity>, IProvidentFundDetail 
    {

        public IList<ProvidentFundDetailEntity> GetDetailListByProvidentFundId(int id)
        {
            IList<ProvidentFundDetailEntity> entityList = new List<ProvidentFundDetailEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from provident.ProvidentFundDetail WITH(NOLOCK) where ProvidentFundId=" + id).AddEntity("ProvidentFundDetailEntity", typeof(ProvidentFundDetailEntity)).List<ProvidentFundDetailEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }

     
    }
}
	
