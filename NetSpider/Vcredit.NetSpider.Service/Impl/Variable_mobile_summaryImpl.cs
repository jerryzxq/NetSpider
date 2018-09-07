using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Variable_mobile_summaryEntity服务对象
    internal class Variable_mobile_summaryImpl : BaseService<Variable_mobile_summaryEntity>, IVariable_mobile_summary
    {
        public void DeleteBySourceIdAndSourceType(string sourceId, string sourceType)
        {
            base.Delete("from Variable_mobile_summaryEntity where SourceId=" + sourceId + " and  SourceType='" + sourceType + "'");
        }

        public Variable_mobile_summaryEntity GetByBusIdentityNoAndMobile(string busIdentityNo, string Mobile, string BusType)
        {
            Variable_mobile_summaryEntity summaryEntity = null;
            if (BusType == null)
            {
                summaryEntity = base.FindListByHql(@"from Variable_mobile_summaryEntity where BusIdentityCard=? and  Mobile=? order by Id desc", new object[] { busIdentityNo, Mobile }, 1, 1).FirstOrDefault();
            }
            else
            {
                summaryEntity = base.FindListByHql(@"from Variable_mobile_summaryEntity where BusIdentityCard=? and  Mobile=? and BusType=? order by Id desc", new object[] { busIdentityNo, Mobile, BusType }, 1, 1).FirstOrDefault();
            }

            return summaryEntity;
        }

        public Variable_mobile_summaryEntity GetBySourceIdAndSourceType(string sourceId, string sourceType)
        {
            Variable_mobile_summaryEntity entity = base.FindListByHql(@"from Variable_mobile_summaryEntity where SourceId=? and  SourceType=? order by Id desc", new object[] { sourceId, sourceType }, 1, 1).FirstOrDefault();

            return entity;
        }

    }
}