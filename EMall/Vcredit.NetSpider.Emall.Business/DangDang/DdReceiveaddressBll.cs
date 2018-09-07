using ServiceStack.OrmLite;
using System.Collections.Generic;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdReceiveaddressBll : Business<DdReceiveaddressEntity, SqlConnectionFactory>
	{
		public static readonly DdReceiveaddressBll Initialize = new DdReceiveaddressBll();
		DdReceiveaddressBll() { }

		public DdReceiveaddressEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdReceiveaddressEntity> List()
		{
			return Select();
		}

		public List<DdReceiveaddressEntity> List(SqlExpression<DdReceiveaddressEntity> expression)
		{
			return Select(expression);
		}

		//public void SaveAsync(DdReceiveaddressEntity item)
		//{
		//    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
		//}
	}
}
