using ServiceStack.OrmLite;
using System.Collections.Generic;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdCartBll : Business<DdCartEntity, SqlConnectionFactory>
	{
		public static readonly DdCartBll Initialize = new DdCartBll();
		DdCartBll() { }

		public DdCartEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdCartEntity> List()
		{
			return Select();
		}

		public List<DdCartEntity> List(SqlExpression<DdCartEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 根据token查询用户购物车数据
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public List<DdCartEntity> QueryList(string token)
		{
			var userInfo = DdUserinfoBll.Initialize.GetByToken(token);
			if (userInfo == null)
				return null;

			SqlExpression<DdCartEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => x.UserId == userInfo.Id);

			var data = Initialize.Select(sqlexp);
			return data;
		}
	}
}
