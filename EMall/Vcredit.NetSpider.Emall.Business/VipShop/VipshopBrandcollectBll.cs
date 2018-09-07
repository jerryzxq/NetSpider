using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Vipshop;

namespace Vcredit.NetSpider.Emall.Business.Vipshop
{

    public class VipshopBrandcollectBll : Business<VipshopBrandcollectEntity, SqlConnectionFactory>
	{
        public static readonly VipshopBrandcollectBll Initialize = new VipshopBrandcollectBll();
        VipshopBrandcollectBll() { }

	    public VipshopBrandcollectEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopBrandcollectEntity> List()
        {
            return Select();
        }

        public List<VipshopBrandcollectEntity> List(SqlExpression<VipshopBrandcollectEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(VipshopBrandcollectEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
