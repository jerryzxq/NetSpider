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

    public class TaobaoGoodsBll : Business<TaobaoGoodsEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoGoodsBll Initialize = new TaobaoGoodsBll();
        TaobaoGoodsBll() { }

        public TaobaoGoodsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoGoodsEntity> List()
        {
            return Select();
        }

        public List<TaobaoGoodsEntity> List(SqlExpression<TaobaoGoodsEntity> expression)
        {
            return Select(expression);
        }

        /// <summary>
        /// 根据订单号获取
        /// </summary>
        /// <param name="orderNos"></param>
        /// <returns></returns>
        public List<TaobaoGoodsEntity> GetByOrderNos(IEnumerable<string> orderNos)
        {
            SqlExpression<TaobaoGoodsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => Sql.In(x.OrderNo, orderNos));

            return Select(sqlexp);
        }

    }
}
