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
	
	public class TaobaoTradeStatusBll : Business<TaobaoTradeStatusEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoTradeStatusBll Initialize = new TaobaoTradeStatusBll();
        TaobaoTradeStatusBll() { }

	    public TaobaoTradeStatusEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoTradeStatusEntity> List()
        {
            return Select();
        }

        public List<TaobaoTradeStatusEntity> List(SqlExpression<TaobaoTradeStatusEntity> expression)
        {
            return Select(expression);
        }

       
	}
}
