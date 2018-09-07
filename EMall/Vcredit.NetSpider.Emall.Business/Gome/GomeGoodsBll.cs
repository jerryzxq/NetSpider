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
	
	public class GomeGoodsBll : Business<GomeGoodsEntity, SqlConnectionFactory>
	{
        public static readonly GomeGoodsBll Initialize = new GomeGoodsBll();
        GomeGoodsBll() { }

	    public GomeGoodsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeGoodsEntity> List()
        {
            return Select();
        }

        public List<GomeGoodsEntity> List(SqlExpression<GomeGoodsEntity> expression)
        {
            return Select(expression);
        }

        public List<GomeGoodsEntity> GetByOrderNos(IEnumerable<string> orderNos)
        {
            SqlExpression<GomeGoodsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => Sql.In(x.OrderNo, orderNos));

            return Select(sqlexp);
        }
       
    }
}
