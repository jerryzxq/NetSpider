using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.VipShop;
using System;

namespace Vcredit.NetSpider.Emall.Business.VipShop
{

    public class VipshopLogisticsBll : Business<VipshopLogisticsEntity, SqlConnectionFactory>
	{
        public static readonly VipshopLogisticsBll Initialize = new VipshopLogisticsBll();
        VipshopLogisticsBll() { }

	    public VipshopLogisticsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopLogisticsEntity> List()
        {
            return Select();
        }

        public List<VipshopLogisticsEntity> List(SqlExpression<VipshopLogisticsEntity> expression)
        {
            return Select(expression);
        }

        public List<VipshopLogisticsEntity> QueryList(string token)
        {
            var userInfo =VipshopUserInfoBll.Initialize.GetByToken(token);
            if (userInfo == null)
                return null;

            SqlExpression<VipshopLogisticsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userInfo.Account);

            var data = Initialize.Select(sqlexp);
            return data;
        }
    }
}
