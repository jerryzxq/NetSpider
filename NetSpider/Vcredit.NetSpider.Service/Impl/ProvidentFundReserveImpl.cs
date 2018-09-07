using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;

namespace Vcredit.NetSpider.Service
{
    // ProvidentFundReserveEntity服务对象
    internal class ProvidentFundReserveImpl : BaseService<ProvidentFundReserveEntity>, IProvidentFundReserve
    {
        IProvidentFundReserveDetailDao detailDao;
        public override object Save(ProvidentFundReserveEntity entity)
        {
            base.Save(entity);

            if (entity.ProvidentReserveFundDetailList != null)
            {
                foreach (var detail in entity.ProvidentReserveFundDetailList)
                {
                    detail.ProvidentFundId = entity.ProvidentFundId;
                    detail.ProvidentFundReserveId = entity.Id;
                    detailDao.Save(detail);
                }
            }

            return entity.Id;
        }

        public ProvidentFundReserveEntity GetByProvidentFundId(long Id)
        {
            var ls = base.FindListByHql("from ProvidentFundReserveEntity where ProvidentFundId=? order by Id desc", new object[] { Id }, 1, 1);

            return ls.FirstOrDefault();
        }
    }
}
