using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdCartBll : Business<YhdCartEntity, SqlConnectionFactory>
	{
		public static readonly YhdCartBll Initialize = new YhdCartBll();
		YhdCartBll() { }

		public YhdCartEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdCartEntity> List()
		{
			return Select();
		}

		public List<YhdCartEntity> List(SqlExpression<YhdCartEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 根据token查询用户购物车数据
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public List<YhdCartEntity> QueryList(string token)
		{
			var userInfo = YhdUserinfoBll.Initialize.GetByToken(token);
			if (userInfo == null)
				return null;

			SqlExpression<YhdCartEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => x.UserId == userInfo.Id);

			var data = Initialize.Select(sqlexp);
			return data;
		}
	}
}
