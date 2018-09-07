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
	
	public class TaobaoRefundBll : Business<TaobaoRefundEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoRefundBll Initialize = new TaobaoRefundBll();
        TaobaoRefundBll() { }

	    public TaobaoRefundEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoRefundEntity> List()
        {
            return Select();
        }

        public List<TaobaoRefundEntity> List(SqlExpression<TaobaoRefundEntity> expression)
        {
            return Select(expression);
        }

       
	}
}
