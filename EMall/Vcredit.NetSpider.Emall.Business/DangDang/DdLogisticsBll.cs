using ServiceStack.OrmLite;
using System.Collections.Generic;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdLogisticsBll : Business<DdLogisticsEntity, SqlConnectionFactory>
	{
		public static readonly DdLogisticsBll Initialize = new DdLogisticsBll();
		DdLogisticsBll() { }

		public DdLogisticsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdLogisticsEntity> List()
		{
			return Select();
		}

		public List<DdLogisticsEntity> List(SqlExpression<DdLogisticsEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 物流数据查询
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public List<DdLogisticsEntity> QueryList(string token)
		{
			var userInfo = DdUserinfoBll.Initialize.GetByToken(token);
			if (userInfo == null)
				return null;

			SqlExpression<DdLogisticsEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => x.AccountName == userInfo.Account);

			var data = Initialize.Select(sqlexp);
			return data;
		}
	}
}
