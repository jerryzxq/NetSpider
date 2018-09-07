using ServiceStack.OrmLite;
using System.Collections.Generic;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdCollectshopsBll : Business<DdCollectshopsEntity, SqlConnectionFactory>
	{
		public static readonly DdCollectshopsBll Initialize = new DdCollectshopsBll();
		DdCollectshopsBll() { }

		public DdCollectshopsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdCollectshopsEntity> List()
		{
			return Select();
		}

		public List<DdCollectshopsEntity> List(SqlExpression<DdCollectshopsEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(DdCollectshopsEntity item)
		//{
		//    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
