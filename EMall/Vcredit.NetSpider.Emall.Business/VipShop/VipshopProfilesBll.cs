using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Vipshop;

namespace Vcredit.NetSpider.Emall.Business.Vipshop
{

    public class VipshopProfilesBll : Business<VipshopProfilesEntity, SqlConnectionFactory>
	{
        public static readonly VipshopProfilesBll Initialize = new VipshopProfilesBll();
        VipshopProfilesBll() { }

	    public VipshopProfilesEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopProfilesEntity> List()
        {
            return Select();
        }

        public List<VipshopProfilesEntity> List(SqlExpression<VipshopProfilesEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(VipshopProfilesEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
