using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdReceiveaddressBll : Business<YhdReceiveaddressEntity, SqlConnectionFactory>
	{
		public static readonly YhdReceiveaddressBll Initialize = new YhdReceiveaddressBll();
		YhdReceiveaddressBll() { }

		public YhdReceiveaddressEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdReceiveaddressEntity> List()
		{
			return Select();
		}

		public List<YhdReceiveaddressEntity> List(SqlExpression<YhdReceiveaddressEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(YhdReceiveaddressEntity item)
		//{
		//	Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
