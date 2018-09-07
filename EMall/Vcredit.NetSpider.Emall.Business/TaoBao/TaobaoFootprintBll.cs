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
	
	public class TaobaoFootprintBll : Business<TaobaoFootprintEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoFootprintBll Initialize = new TaobaoFootprintBll();
        TaobaoFootprintBll() { }

	    public TaobaoFootprintEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoFootprintEntity> List()
        {
            return Select();
        }

        public List<TaobaoFootprintEntity> List(SqlExpression<TaobaoFootprintEntity> expression)
        {
            return Select(expression);
        }

       
	}
}
