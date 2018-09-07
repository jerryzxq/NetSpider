using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdCollectgoodsBll : Business<YhdCollectgoodsEntity, SqlConnectionFactory>
	{
		public static readonly YhdCollectgoodsBll Initialize = new YhdCollectgoodsBll();
		YhdCollectgoodsBll() { }

		public YhdCollectgoodsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdCollectgoodsEntity> List()
		{
			return Select();
		}

		public List<YhdCollectgoodsEntity> List(SqlExpression<YhdCollectgoodsEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(YhdCollectgoodsEntity item)
		//{
		//    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
