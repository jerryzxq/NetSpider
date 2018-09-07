using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;


namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_GoodsBll : Business<GoodsEntity, SqlConnectionFactory>
    {
        /// <summary>
        /// 根据订单号获取商品
        /// </summary>
        /// <param name="orderNos"></param>
        /// <returns></returns>
        public List<GoodsEntity> GetByOrderNos(IEnumerable<string> orderNos)
        {
            SqlExpression<GoodsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => Sql.In(x.OrderNo, orderNos));

            return Select(sqlexp);
        }
    }
}
