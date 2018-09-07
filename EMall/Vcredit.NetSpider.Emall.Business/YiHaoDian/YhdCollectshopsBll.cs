using Vcredit.NetSpider.Emall.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdCollectshopsBll : Business<YhdCollectshopsEntity, SqlConnectionFactory>
	{
		public static readonly YhdCollectshopsBll Initialize = new YhdCollectshopsBll();
		YhdCollectshopsBll() { }

		public YhdCollectshopsEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdCollectshopsEntity> List()
		{
			return Select();
		}

		public List<YhdCollectshopsEntity> List(SqlExpression<YhdCollectshopsEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(YhdCollectshopsEntity item)
		//{
		//    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
