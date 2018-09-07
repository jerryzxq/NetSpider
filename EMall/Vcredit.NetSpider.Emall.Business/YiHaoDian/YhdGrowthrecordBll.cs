using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	
	public class YhdGrowthrecordBll : Business<YhdGrowthrecordEntity, SqlConnectionFactory>
	{
		public static readonly YhdGrowthrecordBll Initialize = new YhdGrowthrecordBll();
		YhdGrowthrecordBll() { }

		public YhdGrowthrecordEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdGrowthrecordEntity> List()
		{
			return Select();
		}

		public List<YhdGrowthrecordEntity> List(SqlExpression<YhdGrowthrecordEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(YhdGrowthrecordEntity item)
		//{
		//	Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
