using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    // ProvidentFundLoanDetailEntity服务对象
    internal class ProvidentFundLoanDetailImpl : BaseService<ProvidentFundLoanDetailEntity>, IProvidentFundLoanDetail
    {
        public IList<ProvidentFundLoanDetailEntity> GetDetailListByProvidentFundId(long id)
        {
            IList<ProvidentFundLoanDetailEntity> entityList = new List<ProvidentFundLoanDetailEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from provident.ProvidentFundLoanDetail WITH(NOLOCK) where ProvidentFundId=" + id).AddEntity("ProvidentFundLoanDetailEntity", typeof(ProvidentFundLoanDetailEntity)).List<ProvidentFundLoanDetailEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}
