using ServiceStack.OrmLite;
using System.Collections.Generic;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdCollectgoodsBll : Business<DdCollectgoodsEntity, SqlConnectionFactory>
	{
		public static readonly DdCollectgoodsBll Initialize = new DdCollectgoodsBll();
		DdCollectgoodsBll() { }

		public DdCollectgoodsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdCollectgoodsEntity> List()
		{
			return Select();
		}

		public List<DdCollectgoodsEntity> List(SqlExpression<DdCollectgoodsEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(DdCollectgoodsEntity item)
		//{
		//    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
