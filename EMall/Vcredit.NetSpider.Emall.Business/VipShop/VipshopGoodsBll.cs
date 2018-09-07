using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.VipShop;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.VipShop
{

    public class VipshopGoodsBll : Business<VipshopGoodsEntity, SqlConnectionFactory>
    {
        public static readonly VipshopGoodsBll Initialize = new VipshopGoodsBll();
        VipshopGoodsBll() { }

        public VipshopGoodsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopGoodsEntity> List()
        {
            return Select();
        }

        public List<VipshopGoodsEntity> List(SqlExpression<VipshopGoodsEntity> expression)
        {
            return Select(expression);
        }

        public List<VipshopGoodsEntity> GetByOrderNos(IEnumerable<string> orderNos)
        {
            SqlExpression<VipshopGoodsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => Sql.In(x.OrderNo, orderNos));

            return Select(sqlexp);
        }
    }
}
