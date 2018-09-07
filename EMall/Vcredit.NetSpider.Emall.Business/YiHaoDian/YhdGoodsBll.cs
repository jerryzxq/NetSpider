using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdGoodsBll : Business<YhdGoodsEntity, SqlConnectionFactory>
	{
		public static readonly YhdGoodsBll Initialize = new YhdGoodsBll();
		YhdGoodsBll() { }

		public YhdGoodsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdGoodsEntity> List()
		{
			return Select();
		}

		public List<YhdGoodsEntity> List(SqlExpression<YhdGoodsEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 根据订单号获取
		/// </summary>
		/// <param name="orderNos"></param>
		/// <returns></returns>
		public List<YhdGoodsEntity> GetByOrderNos(IEnumerable<string> orderNos)
		{
			SqlExpression<YhdGoodsEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => Sql.In(x.OrderNo, orderNos));

			return Select(sqlexp);
		}
	}
}
