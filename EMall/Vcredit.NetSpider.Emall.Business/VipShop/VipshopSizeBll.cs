using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Vipshop;

namespace Vcredit.NetSpider.Emall.Business.Vipshop
{

    public class VipshopSizeBll : Business<VipshopSizeEntity, SqlConnectionFactory>
	{
        public static readonly VipshopSizeBll Initialize = new VipshopSizeBll();
        VipshopSizeBll() { }

	    public VipshopSizeEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopSizeEntity> List()
        {
            return Select();
        }

        public List<VipshopSizeEntity> List(SqlExpression<VipshopSizeEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(VipshopSizeEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
