using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Linq;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.DangDang;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdUserinfoBll : Business<DdUserinfoEntity, SqlConnectionFactory>
	{
		public static readonly DdUserinfoBll Initialize = new DdUserinfoBll();
		DdUserinfoBll() { }

		public DdUserinfoEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdUserinfoEntity> List()
		{
			return Select();
		}

		public List<DdUserinfoEntity> List(SqlExpression<DdUserinfoEntity> expression)
		{
			return Select(expression);
		}

		public DdUserinfoEntity GetByToken(string token)
		{
			return this.Select(x => x.Token == token).FirstOrDefault();
		}

	}
}
