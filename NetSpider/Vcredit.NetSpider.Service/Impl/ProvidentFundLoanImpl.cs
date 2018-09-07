using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;

namespace Vcredit.NetSpider.Service
{
    // ProvidentFundLoanEntity服务对象
    internal class ProvidentFundLoanImpl : BaseService<ProvidentFundLoanEntity>, IProvidentFundLoan
    {
        IProvidentFundLoanDetailDao detailDao;
        public override object Save(ProvidentFundLoanEntity entity)
        {
            base.Save(entity);

            if (entity.ProvidentFundLoanDetailList != null)
            {
                foreach (var detail in entity.ProvidentFundLoanDetailList)
                {
                    detail.ProvidentFundId = entity.ProvidentFundId;
                    detail.ProvidentFundLoanId = entity.Id;
                    detailDao.Save(detail);
                }
            }

            return entity.Id;
        }

        public ProvidentFundLoanEntity GetByProvidentFundId(long Id)
        {
            var ls = base.FindListByHql("from ProvidentFundLoanEntity where ProvidentFundId=? order by Id desc", new object[] { Id }, 1, 1);

            return ls.FirstOrDefault();
        }
    }
}
