using ServiceStack.OrmLite;
using System.Collections.Generic;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdGoodsBll : Business<DdGoodsEntity, SqlConnectionFactory>
	{
		public static readonly DdGoodsBll Initialize = new DdGoodsBll();
		DdGoodsBll() { }

		public DdGoodsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdGoodsEntity> List()
		{
			return Select();
		}

		public List<DdGoodsEntity> List(SqlExpression<DdGoodsEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 根据订单号获取
		/// </summary>
		/// <param name="orderNos"></param>
		/// <returns></returns>
		public List<DdGoodsEntity> GetByOrderNos(IEnumerable<string> orderNos)
		{
			SqlExpression<DdGoodsEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => Sql.In(x.OrderNo, orderNos));

			return Select(sqlexp);
		}
	}
}
