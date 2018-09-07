using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdLogisticsBll : Business<YhdLogisticsEntity, SqlConnectionFactory>
	{
		public static readonly YhdLogisticsBll Initialize = new YhdLogisticsBll();
		YhdLogisticsBll() { }

		public YhdLogisticsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdLogisticsEntity> List()
		{
			return Select();
		}

		public List<YhdLogisticsEntity> List(SqlExpression<YhdLogisticsEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 物流数据查询
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public List<YhdLogisticsEntity> QueryList(string token)
		{
			var userInfo = YhdUserinfoBll.Initialize.GetByToken(token);
			if (userInfo == null)
				return null;

			SqlExpression<YhdLogisticsEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => x.AccountName == userInfo.Account);

			var data = Initialize.Select(sqlexp);
			return data;
		}

	}
}
