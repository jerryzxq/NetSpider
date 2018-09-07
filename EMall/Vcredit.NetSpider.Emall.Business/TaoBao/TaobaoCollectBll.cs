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
	
	public class TaobaoCollectBll : Business<TaobaoCollectEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoCollectBll Initialize = new TaobaoCollectBll();
        TaobaoCollectBll() { }

	    public TaobaoCollectEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoCollectEntity> List()
        {
            return Select();
        }

        public List<TaobaoCollectEntity> List(SqlExpression<TaobaoCollectEntity> expression)
        {
            return Select(expression);
        }

     
	}
}
