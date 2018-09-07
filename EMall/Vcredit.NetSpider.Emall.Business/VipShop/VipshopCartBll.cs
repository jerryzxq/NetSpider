using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Vipshop;
using System;
using Vcredit.NetSpider.Emall.Business.VipShop;

namespace Vcredit.NetSpider.Emall.Business.Vipshop
{

    public class VipshopCartBll : Business<VipshopCartEntity, SqlConnectionFactory>
	{
        public static readonly VipshopCartBll Initialize = new VipshopCartBll();
        VipshopCartBll() { }

	    public VipshopCartEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopCartEntity> List()
        {
            return Select();
        }

        public List<VipshopCartEntity> List(SqlExpression<VipshopCartEntity> expression)
        {
            return Select(expression);
        }

        public List<VipshopCartEntity> QueryList(string token)
        {
            var userInfo = VipshopUserInfoBll.Initialize.GetByToken(token);
            if (userInfo == null)
                return null;

            SqlExpression<VipshopCartEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.UserId == userInfo.Id);

            var data = Initialize.Select(sqlexp);
            return data;
        }

        //public void SaveAsync(VipshopCartEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
    }
}
