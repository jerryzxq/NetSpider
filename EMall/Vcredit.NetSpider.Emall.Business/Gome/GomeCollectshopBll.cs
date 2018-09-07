using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Gome;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.Gome
{
	
	public class GomeCollectshopBll : Business<GomeCollectshopEntity, SqlConnectionFactory>
	{
        public static readonly GomeCollectshopBll Initialize = new GomeCollectshopBll();
        GomeCollectshopBll() { }

	    public GomeCollectshopEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeCollectshopEntity> List()
        {
            return Select();
        }

        public List<GomeCollectshopEntity> List(SqlExpression<GomeCollectshopEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(GomeCollectshopEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
