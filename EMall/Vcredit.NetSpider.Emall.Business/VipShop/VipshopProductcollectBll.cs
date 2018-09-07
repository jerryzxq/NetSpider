using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.TaoBao
{
	
	public class VipshopProductcollectBll : Business<VipshopProductcollectEntity, SqlConnectionFactory>
	{
        public static readonly VipshopProductcollectBll Initialize = new VipshopProductcollectBll();
        VipshopProductcollectBll() { }

	    public VipshopProductcollectEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopProductcollectEntity> List()
        {
            return Select();
        }

        public List<VipshopProductcollectEntity> List(SqlExpression<VipshopProductcollectEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(VipshopProductcollectEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
