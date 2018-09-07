using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.TaoBao
{
	
	public class TaobaoDistrictBll : Business<TaobaoDistrictEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoDistrictBll Initialize = new TaobaoDistrictBll();
        TaobaoDistrictBll() { }

	    public TaobaoDistrictEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoDistrictEntity> List()
        {
            return Select();
        }

        public List<TaobaoDistrictEntity> List(SqlExpression<TaobaoDistrictEntity> expression)
        {
            return Select(expression);
        }

      
	}
}
