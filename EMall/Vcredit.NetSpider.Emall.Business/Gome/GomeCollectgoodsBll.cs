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
	
	public class GomeCollectgoodsBll : Business<GomeCollectgoodsEntity, SqlConnectionFactory>
	{
        public static readonly GomeCollectgoodsBll Initialize = new GomeCollectgoodsBll();
        GomeCollectgoodsBll() { }

	    public GomeCollectgoodsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeCollectgoodsEntity> List()
        {
            return Select();
        }

        public List<GomeCollectgoodsEntity> List(SqlExpression<GomeCollectgoodsEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(GomeCollectgoodsEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
